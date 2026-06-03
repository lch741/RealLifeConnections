import {
  spawnProcess,
  stopProcessTree,
  waitForServer,
} from "./process-utils.mjs";

const host = "127.0.0.1";
const port = "3000";
const baseUrl = `http://${host}:${port}`;
const args = process.argv.slice(2);

const server = spawnProcess(
  "npx",
  ["next", "dev", "--hostname", host, "--port", port],
  { detached: process.platform !== "win32" },
);

let exitCode = 1;

try {
  await waitForServer(baseUrl);

  exitCode = await new Promise((resolve) => {
    const playwright = spawnProcess("npx", ["playwright", "test", ...args]);
    playwright.on("exit", (code) => resolve(code ?? 1));
    playwright.on("error", () => resolve(1));
  });
} finally {
  await stopProcessTree(server);
}

process.exit(exitCode);
