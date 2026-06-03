import { expect, test } from "@playwright/test";
import { e2eProfile } from "./helpers";

async function mockSuccessfulLogin(page: import("@playwright/test").Page) {
  await page.route("**/api/user/login", async (route) => {
    await route.fulfill({
      status: 200,
      contentType: "application/json",
      body: JSON.stringify({
        message: "Login successful.",
        token: "e2e-token",
        user: {
          id: 1,
          email: "alex@example.com",
          userName: "Alex",
        },
      }),
    });
  });

  await page.route("**/api/user/profile", async (route) => {
    await route.fulfill({
      status: 200,
      contentType: "application/json",
      body: JSON.stringify({
        id: 1,
        email: "alex@example.com",
        userName: "Alex",
        region: "Auckland",
        suburb: "Auckland City",
        avatarUrl: null,
        gender: "NotToTell",
        age: 28,
        culture: null,
        isVerified: true,
        verificationStatus: "Verified",
        canMatch: true,
        personality: {
          chillToEnergetic: 50,
          talkativeToQuiet: 50,
          plannerToSpontaneous: 50,
          introvertToExtrovert: 50,
          preferredDaysOfWeek: "Weekday",
          preferredTimeOfDay: "Morning",
          preferredDistanceKm: 10,
        },
        interestSelections: [
          e2eProfile.interestSelections[0],
        ],
      }),
    });
  });
}

test.describe("auth e2e", () => {
  test("lets visitors move between login and register pages", async ({ page }) => {
    await page.goto("/login");

    await expect(
      page.getByRole("heading", {
        name: /Welcome back to your real-life circle/i,
      }),
    ).toBeVisible();

    await page.getByRole("link", { name: /Create an account/i }).click();
    await expect(page).toHaveURL(/\/register$/);
    await expect(
      page.getByRole("heading", { name: /Build a profile that feels like you/i }),
    ).toBeVisible();

    await page.getByRole("link", { name: /Sign in/i }).click();
    await expect(page).toHaveURL(/\/login$/);
  });

  test("logs in with a mocked backend response and opens profile", async ({
    page,
  }) => {
    await mockSuccessfulLogin(page);
    await page.goto("/login");

    await page.getByLabel(/Email/i).fill("alex@example.com");
    await page.getByLabel(/Password/i).fill("Password123!");
    await page.getByRole("button", { name: /Sign in/i }).click();

    await expect(page).toHaveURL(/\/profile$/);
    await expect(page.getByText(/Edit your information\./i)).toBeVisible();
  });
});
