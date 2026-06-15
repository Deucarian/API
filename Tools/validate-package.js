const fs = require("fs");
const path = require("path");

const root = process.cwd();
const errors = [];

function fail(message) {
  errors.push(message);
}

function exists(relativePath) {
  return fs.existsSync(path.join(root, relativePath));
}

function readJson(relativePath) {
  const filePath = path.join(root, relativePath);
  try {
    return JSON.parse(fs.readFileSync(filePath, "utf8"));
  } catch (error) {
    fail(`${relativePath} is not valid JSON: ${error.message}`);
    return null;
  }
}

function walk(directory, files = []) {
  if (!fs.existsSync(directory)) {
    return files;
  }

  for (const entry of fs.readdirSync(directory, { withFileTypes: true })) {
    const fullPath = path.join(directory, entry.name);
    if (entry.isDirectory()) {
      walk(fullPath, files);
    } else {
      files.push(fullPath);
    }
  }

  return files;
}

function assertAsmdefContains(relativePath, predicate, message) {
  const asmdef = readJson(relativePath);
  if (!asmdef) {
    return;
  }

  if (!predicate(asmdef)) {
    fail(message);
  }
}

if (!exists("package.json")) fail("package.json is missing.");
if (!exists("README.md")) fail("README.md is missing.");
if (!exists("CHANGELOG.md")) fail("CHANGELOG.md is missing.");
if (!exists("LICENSE.md")) fail("LICENSE.md is missing.");
if (!exists("Runtime/Deucarian.API.asmdef")) fail("Runtime/Deucarian.API.asmdef is missing.");

const pkg = readJson("package.json");
if (pkg) {
  if (pkg.name !== "com.deucarian.api") {
    fail(`package name must be com.deucarian.api, got ${pkg.name}`);
  }

  if (!/^\d+\.\d+\.\d+(?:-[0-9A-Za-z.-]+)?$/.test(pkg.version || "")) {
    fail(`package version must be valid SemVer, got ${pkg.version}`);
  }

  const requiredDependencies = [
    "com.deucarian.logging",
    "com.unity.nuget.newtonsoft-json",
    "com.unity.modules.unitywebrequest",
    "com.unity.modules.unitywebrequesttexture",
    "com.unity.modules.unitywebrequestwww"
  ];
  for (const dependency of requiredDependencies) {
    if (!pkg.dependencies || !pkg.dependencies[dependency]) {
      fail(`package.json must declare ${dependency} dependency.`);
    }
  }
}

assertAsmdefContains(
  "Runtime/Deucarian.API.asmdef",
  asmdef => Array.isArray(asmdef.includePlatforms) && asmdef.includePlatforms.length === 0,
  "Runtime assembly must not be editor-only."
);

assertAsmdefContains(
  "Runtime/Deucarian.API.asmdef",
  asmdef => Array.isArray(asmdef.references) && asmdef.references.includes("Deucarian.Logging"),
  "Runtime assembly must reference Deucarian.Logging."
);

if (exists("Editor/Deucarian.API.Editor.asmdef")) {
  assertAsmdefContains(
    "Editor/Deucarian.API.Editor.asmdef",
    asmdef => Array.isArray(asmdef.includePlatforms) && asmdef.includePlatforms.includes("Editor"),
    "Editor assembly must include only the Editor platform."
  );
  assertAsmdefContains(
    "Editor/Deucarian.API.Editor.asmdef",
    asmdef => Array.isArray(asmdef.references) && asmdef.references.includes("Deucarian.Logging"),
    "Editor assembly must reference Deucarian.Logging."
  );
}

if (exists("Samples~/ExampleScene/Deucarian.API.Samples.asmdef")) {
  assertAsmdefContains(
    "Samples~/ExampleScene/Deucarian.API.Samples.asmdef",
    asmdef => Array.isArray(asmdef.references) && asmdef.references.includes("Deucarian.Logging"),
    "Sample assembly must reference Deucarian.Logging."
  );
}

if (exists("Tests/Editor/Deucarian.API.Tests.asmdef")) {
  assertAsmdefContains(
    "Tests/Editor/Deucarian.API.Tests.asmdef",
    asmdef =>
      Array.isArray(asmdef.includePlatforms) &&
      asmdef.includePlatforms.includes("Editor") &&
      Array.isArray(asmdef.optionalUnityReferences) &&
      asmdef.optionalUnityReferences.includes("TestAssemblies"),
    "Tests assembly must be Editor-only and reference TestAssemblies."
  );
}

if (exists("Samples") || exists("Assets/Deucarian/API/Samples")) {
  fail("Package samples must live under Samples~.");
}

for (const bannedDirectory of ["Library", "Temp", "Obj", "Build", "Builds", "ProjectSettings", "Packages"]) {
  if (exists(bannedDirectory)) {
    fail(`Generated/project directory must not be committed: ${bannedDirectory}`);
  }
}

const allFiles = walk(root).map(file => path.relative(root, file).replace(/\\/g, "/"));
const metaGuids = new Map();
for (const file of allFiles) {
  if (file.startsWith(".git/")) continue;
  if (file.endsWith(".zip") || file.endsWith(".unitypackage")) {
    fail(`Generated archive must not be committed: ${file}`);
  }
  if (file.includes("/Library/") || file.includes("/Temp/") || file.includes("/Obj/")) {
    fail(`Generated Unity artifact must not be committed: ${file}`);
  }

  if (file.endsWith(".meta")) {
    const content = fs.readFileSync(path.join(root, file), "utf8");
    const match = content.match(/^guid:\s*([a-fA-F0-9]+)/m);
    if (match) {
      const guid = match[1].toLowerCase();
      if (metaGuids.has(guid)) {
        fail(`Duplicate Unity meta guid ${guid}: ${metaGuids.get(guid)} and ${file}`);
      } else {
        metaGuids.set(guid, file);
      }
    }
  }
}

if (errors.length > 0) {
  console.error("Package validation failed:");
  for (const error of errors) {
    console.error(`- ${error}`);
  }
  process.exit(1);
}

console.log("Package validation passed.");
