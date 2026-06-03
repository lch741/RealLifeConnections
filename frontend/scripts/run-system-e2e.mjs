import path from "node:path";
import {
  spawnProcess,
  stopProcessTree,
  waitForServer,
} from "./process-utils.mjs";

const frontendHost = "localhost";
const frontendPort = "3000";
const backendHost = "localhost";
const backendPort = "5118";
const frontendUrl = `http://${frontendHost}:${frontendPort}`;
const backendUrl = `http://${backendHost}:${backendPort}`;
const rootDir = path.resolve(process.cwd(), "..");
const backendProject = path.join(rootDir, "backend", "backend.csproj");
const backendDll = path.join(rootDir, "backend", "bin", "Debug", "net8.0", "backend.dll");
const args = process.argv.slice(2);
const useBuiltBackend = process.env.SYSTEM_E2E_BUILD_BACKEND !== "true";

const backend = spawnProcess(
  "dotnet",
  useBuiltBackend
    ? [backendDll]
    : ["run", "--no-restore", "--project", backendProject],
  {
    cwd: path.join(rootDir, "backend"),
    detached: process.platform !== "win32",
    env: {
      ...process.env,
      ASPNETCORE_ENVIRONMENT: "Development",
      ASPNETCORE_URLS: backendUrl,
    },
  },
);

let frontend;
let exitCode = 1;

try {
  await waitForServer(`${backendUrl}/swagger/index.html`);

  frontend = spawnProcess(
    "npx",
    ["next", "dev", "--hostname", frontendHost, "--port", frontendPort],
    {
      detached: process.platform !== "win32",
      env: {
        ...process.env,
        NEXT_PUBLIC_API_BASE_URL: backendUrl,
        PLAYWRIGHT_BASE_URL: frontendUrl,
        SYSTEM_E2E_API_BASE_URL: backendUrl,
        SYSTEM_E2E: "true",
      },
    },
  );

  await waitForServer(frontendUrl);

  exitCode = await new Promise((resolve) => {
    const playwright = spawnProcess("npx", [
      "playwright",
      "test",
      "tests/e2e/system",
      ...args,
    ], {
      env: {
        ...process.env,
        PLAYWRIGHT_BASE_URL: frontendUrl,
        SYSTEM_E2E_API_BASE_URL: backendUrl,
        SYSTEM_E2E: "true",
      },
    });
    playwright.on("exit", (code) => resolve(code ?? 1));
    playwright.on("error", () => resolve(1));
  });
} finally {
  if (frontend) {
    await stopProcessTree(frontend);
  }
  await stopProcessTree(backend);
}

process.exit(exitCode);
