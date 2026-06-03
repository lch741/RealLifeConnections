import { expect, test } from "@playwright/test";

test.describe("protected routes e2e", () => {
  test("redirects unauthenticated users from meetups to login", async ({ page }) => {
    await page.goto("/meetups");

    await expect(page).toHaveURL(/\/login$/);
    await expect(
      page.getByRole("heading", {
        name: /Welcome back to your real-life circle/i,
      }),
    ).toBeVisible();
  });

  test("redirects home visitors without a token to login", async ({ page }) => {
    await page.goto("/");

    await expect(page).toHaveURL(/\/login$/);
  });
});
