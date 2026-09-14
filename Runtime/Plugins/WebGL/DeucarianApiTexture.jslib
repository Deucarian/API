mergeInto(LibraryManager.library, {
  $DeucarianApiTextures: {
    nextId: 1,
    jobs: {},
    ready: [],
    scheduled: false,
    // One bounded tile per browser frame across all requests in this player.
    tilePixels: 65536,
    current: function(job) { return DeucarianApiTextures.jobs[job.id] === job; },
    close: function(job) {
      if (job.tile) job.tile.close();
      if (job.bitmap) job.bitmap.close();
      job.tile = job.bitmap = null;
    },
    fail: function(job) {
      if (!DeucarianApiTextures.current(job)) return;
      job.state = -1;
      DeucarianApiTextures.close(job);
    },
    prepare: function(job) {
      if (!DeucarianApiTextures.current(job)) return;
      job.rows = Math.min(job.height - job.y, Math.max(1, Math.floor(DeucarianApiTextures.tilePixels / job.width)));
      createImageBitmap(job.bitmap, 0, job.y, job.width, job.rows,
        { premultiplyAlpha: 'none', colorSpaceConversion: 'none' }).then(function(tile) {
        if (!DeucarianApiTextures.current(job)) { tile.close(); return; }
        job.tile = tile;
        DeucarianApiTextures.ready.push(job);
        DeucarianApiTextures.schedule();
      }).catch(function() { DeucarianApiTextures.fail(job); });
    },
    schedule: function() {
      if (DeucarianApiTextures.scheduled || !DeucarianApiTextures.ready.length) return;
      DeucarianApiTextures.scheduled = true;
      requestAnimationFrame(function() {
        DeucarianApiTextures.scheduled = false;
        var job;
        while (DeucarianApiTextures.ready.length) {
          var candidate = DeucarianApiTextures.ready.shift();
          if (DeucarianApiTextures.current(candidate)) { job = candidate; break; }
        }
        if (job) {
          var previous = GLctx.getParameter(GLctx.TEXTURE_BINDING_2D);
          try {
            if (!GL.textures[job.texture] || GLctx.isContextLost()) throw new Error('Texture unavailable');
            GLctx.bindTexture(GLctx.TEXTURE_2D, GL.textures[job.texture]);
            GLctx.texSubImage2D(GLctx.TEXTURE_2D, 0, 0, job.y, GLctx.RGBA, GLctx.UNSIGNED_BYTE, job.tile);
            job.tile.close(); job.tile = null;
            job.y += job.rows;
            if (job.y === job.height) {
              job.state = 3;
              DeucarianApiTextures.close(job);
            } else DeucarianApiTextures.prepare(job);
          } catch (_) { DeucarianApiTextures.fail(job); }
          finally { GLctx.bindTexture(GLctx.TEXTURE_2D, previous); }
        }
        DeucarianApiTextures.schedule();
      });
    }
  },
  DeucarianApiTextureBegin__deps: ['$DeucarianApiTextures'],
  DeucarianApiTextureBegin: function(bytes, length) {
    var id = DeucarianApiTextures.nextId++;
    var job = { id: id, state: 0, width: 0, height: 0, y: 0, tile: null, bitmap: null };
    DeucarianApiTextures.jobs[id] = job;
    try {
      if (typeof createImageBitmap !== 'function') throw new Error('ImageBitmap unavailable');
      var blob = new Blob([HEAPU8.subarray(bytes, bytes + length)]);
      createImageBitmap(blob, { imageOrientation: 'flipY', premultiplyAlpha: 'none', colorSpaceConversion: 'none' }).then(function(bitmap) {
        if (!DeucarianApiTextures.current(job)) { bitmap.close(); return; }
        job.bitmap = bitmap; job.width = bitmap.width; job.height = bitmap.height; job.state = 1;
      }).catch(function() { DeucarianApiTextures.fail(job); });
    } catch (_) { DeucarianApiTextures.fail(job); }
    return id;
  },
  DeucarianApiTextureState__deps: ['$DeucarianApiTextures'],
  DeucarianApiTextureState: function(id) { var job = DeucarianApiTextures.jobs[id]; return job ? job.state : -1; },
  DeucarianApiTextureWidth__deps: ['$DeucarianApiTextures'],
  DeucarianApiTextureWidth: function(id) { return DeucarianApiTextures.jobs[id].width; },
  DeucarianApiTextureHeight__deps: ['$DeucarianApiTextures'],
  DeucarianApiTextureHeight: function(id) { return DeucarianApiTextures.jobs[id].height; },
  DeucarianApiTextureUpload__deps: ['$DeucarianApiTextures', '$GL'],
  DeucarianApiTextureUpload: function(id, texture) {
    var job = DeucarianApiTextures.jobs[id];
    if (!job || job.state !== 1) return;
    job.texture = texture; job.state = 2;
    DeucarianApiTextures.prepare(job);
  },
  DeucarianApiTextureRelease__deps: ['$DeucarianApiTextures'],
  DeucarianApiTextureRelease: function(id) {
    var job = DeucarianApiTextures.jobs[id];
    if (!job) return;
    delete DeucarianApiTextures.jobs[id];
    DeucarianApiTextures.close(job);
  }
});
