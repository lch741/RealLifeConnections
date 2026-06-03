import { spawn } from "node:child_process";

export function spawnProcess(command, processArgs, options = {}) {
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

export async function waitForServer(url, timeoutMs = 120_000) {
  const startedAt = Date.now();
  while (Date.now() - startedAt < timeoutMs) {
    try {
      const response = await fetch(url);
      if (response.status < 500) return;
    } catch {
      // Keep waiting while the server boots.
    }
    await new Promise((resolve) => setTimeout(resolve, 500));
  }

  throw new Error(`Timed out waiting for ${url}`);
}

export function stopProcessTree(child) {
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
