const { spawnSync } = require("child_process");
const fs = require("fs");
const path = require("path");

const root = path.resolve(__dirname, "..");
const candidates = [
  process.env.DEUCARIAN_PACKAGE_REGISTRY_ROOT,
  path.resolve(root, "..", "Package-Registry"),
  path.resolve(root, "..", "..", "Package-Registry"),
  "C:\\Repositories\\Package-Registry"
].filter(Boolean);

const registryRoot = candidates.find(candidate =>
  fs.existsSync(path.join(candidate, "Tools", "deucarian_package_validator.py"))
);

if (!registryRoot) {
  console.error("Could not find Package Registry shared validator. Set DEUCARIAN_PACKAGE_REGISTRY_ROOT.");
  process.exit(1);
}

const validator = path.join(registryRoot, "Tools", "deucarian_package_validator.py");
const config = path.join(root, "deucarian-package.json");
const result = spawnSync("python", [
  validator,
  "--registry-root", registryRoot,
  "--repository-root", root,
  "--config", config
], { stdio: "inherit" });

process.exit(result.status ?? 1);
