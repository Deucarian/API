const { test } = require('node:test');
const assert = require('node:assert/strict');
const vm = require('node:vm');
const fs = require('node:fs');
const path = require('node:path');
function runtime() {
 const raf=[], decoding=[], uploads=[], closed=[];
 const c={LibraryManager:{library:{}},mergeInto:(a,b)=>Object.assign(a,b),HEAPU8:new Uint8Array(16),Blob,
  requestAnimationFrame:f=>raf.push(f),
  createImageBitmap:(source,x,y,w,h)=>new Promise(resolve=>decoding.push({resolve,source,x,y,w,h})),
  GL:{textures:{7:{name:'target'}}},GLctx:{TEXTURE_BINDING_2D:1,TEXTURE_2D:2,RGBA:3,UNSIGNED_BYTE:4,
   getParameter:()=>({name:'previous'}),isContextLost:()=>false,bindTexture:()=>{},
   texSubImage2D:(...args)=>uploads.push(args)}};
 vm.createContext(c);vm.runInContext(fs.readFileSync(path.join(__dirname,'../../Runtime/Plugins/WebGL/DeucarianApiTexture.jslib'),'utf8'),c);
 c.DeucarianApiTextures=c.LibraryManager.library.$DeucarianApiTextures;
 const api=c.LibraryManager.library;
 function bitmap(width,height){return {width,height,close(){closed.push(this);}};}
 async function decode(){const d=decoding.shift();d.resolve(bitmap(d.w||1920,d.h||1080));await Promise.resolve();await Promise.resolve();}
 return {c,api,raf,decoding,uploads,closed,decode};
}
test('large images upload in bounded tiles and preserve full dimensions',async()=>{
 const r=runtime(),id=r.api.DeucarianApiTextureBegin(0,16);await r.decode();
 assert.equal(r.api.DeucarianApiTextureWidth(id),1920);assert.equal(r.api.DeucarianApiTextureHeight(id),1080);
 r.api.DeucarianApiTextureUpload(id,7);
 let frames=0;
 while(r.api.DeucarianApiTextureState(id)!==3){await r.decode();const prior=r.uploads.length;r.raf.shift()();frames++;assert.equal(r.uploads.length-prior,1);assert.ok(frames<100);}
 assert.ok(frames>1);assert.equal(r.uploads.reduce((sum,args)=>sum+args[6].height,0),1080);
 for(const args of r.uploads)assert.ok(args[6].width*args[6].height<=65536);
 assert.equal(r.closed.length,frames+1);r.api.DeucarianApiTextureRelease(id);assert.equal(Object.keys(r.c.DeucarianApiTextures.jobs).length,0);
});
test('cancellation during decoding closes a late bitmap without uploading',async()=>{
 const r=runtime(),id=r.api.DeucarianApiTextureBegin(0,16);r.api.DeucarianApiTextureRelease(id);await r.decode();
 assert.equal(r.closed.length,1);assert.equal(r.uploads.length,0);assert.equal(r.api.DeucarianApiTextureState(id),-1);
});
test('cancellation of queued uploads cannot touch a released Unity texture',async()=>{
 const r=runtime(),id=r.api.DeucarianApiTextureBegin(0,16);await r.decode();r.api.DeucarianApiTextureUpload(id,7);await r.decode();
 r.api.DeucarianApiTextureRelease(id);r.c.GL.textures[7]=null;r.raf.shift()();assert.equal(r.uploads.length,0);assert.equal(r.closed.length,2);
});
test('concurrent images share one upload per frame',async()=>{
 const r=runtime(),a=r.api.DeucarianApiTextureBegin(0,16),b=r.api.DeucarianApiTextureBegin(0,16);
 await r.decode();await r.decode();r.api.DeucarianApiTextureUpload(a,7);r.api.DeucarianApiTextureUpload(b,7);await r.decode();await r.decode();
 assert.equal(r.raf.length,1);r.raf.shift()();assert.equal(r.uploads.length,1);assert.equal(r.raf.length,1);
 r.api.DeucarianApiTextureRelease(a);r.api.DeucarianApiTextureRelease(b);
});
