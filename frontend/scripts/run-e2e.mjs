import { spawn } from "node:child_process";

const host = "127.0.0.1";
const port = "3000";
const baseUrl = `http://${host}:${port}`;
const args = process.argv.slice(2);

function spawnProcess(command, processArgs, options = {}) {
  const isWindows = process.platform === "win32";
  const finalCommand = isWindows ? "cmd.exe" : command;
  const finalArgs = isWindows
    ? ["/d", "/s", "/c", command, ...processArgs]
    : processArgs;

  return spawn(finalCommand, finalArgs, {
    cwd: process.cwd(),
    env: process.env,
    stdio: "inherit",
    ...options,
  });
}

async function waitForServer(timeoutMs = 120_000) {
  const startedAt = Date.now();
  while (Date.now() - startedAt < timeoutMs) {
    try {
      const response = await fetch(baseUrl);
      if (response.status < 500) return;
    } catch {
      // Keep waiting while Next boots.
    }
    await new Promise((resolve) => setTimeout(resolve, 500));
  }

  throw new Error(`Timed out waiting for ${baseUrl}`);
}

function stopProcessTree(child) {
  if (!child.pid || child.killed) return Promise.resolve();

  return new Promise((resolve) => {
    if (process.platform === "win32") {
      const killer = spawn("taskkill", ["/pid", String(child.pid), "/T", "/F"], {
        stdio: "ignore",
      });
      killer.on("exit", () => resolve());
      killer.on("error", () => resolve());
      return;
    }

    try {
      process.kill(-child.pid, "SIGTERM");
    } catch {
      try {
        child.kill("SIGTERM");
      } catch {
        // Process is already gone.
      }
    }
    resolve();
  });
}

const server = spawnProcess(
  "npx",
  ["next", "dev", "--hostname", host, "--port", port],
  { detached: process.platform !== "win32" },
);

let exitCode = 1;

try {
  await waitForServer();

  exitCode = await new Promise((resolve) => {
    const playwright = spawnProcess("npx", ["playwright", "test", ...args]);
    playwright.on("exit", (code) => resolve(code ?? 1));
    playwright.on("error", () => resolve(1));
  });
} finally {
  await stopProcessTree(server);
}

process.exit(exitCode);
