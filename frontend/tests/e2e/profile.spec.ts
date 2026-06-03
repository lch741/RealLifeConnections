import { expect, test } from "@playwright/test";
import { mockProfile, seedAuth } from "./helpers";

test.describe("profile e2e", () => {
  test("loads profile data and saves a simple profile edit", async ({ page }) => {
    await seedAuth(page);
    await mockProfile(page);

    await page.goto("/profile");

    await expect(
      page.getByRole("heading", { name: /Edit your information/i }),
    ).toBeVisible();

    const username = page.getByLabel(/Username/i);
    await expect(username).toHaveValue("Alex");
    await username.fill("Alex E2E");
    await page.getByLabel(/Bio/i).fill("Updated from an e2e smoke test.");

    await page.getByRole("button", { name: /Save changes/i }).click();

    await expect(page.getByText(/Profile updated successfully/i)).toBeVisible();
    await expect(username).toHaveValue("Alex E2E");
  });
});
