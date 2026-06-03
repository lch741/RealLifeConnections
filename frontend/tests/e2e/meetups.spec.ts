import { expect, test } from "@playwright/test";
import { mockMeetups, mockProfile, seedAuth } from "./helpers";

test.describe("meetups e2e", () => {
  test("loads the meetups hub and shows hosted meetups", async ({ page }) => {
    await seedAuth(page);
    await mockProfile(page);
    await mockMeetups(page);

    await page.goto("/meetups?tab=manage");

    await expect(page.getByText(/Meetups Hub/i)).toBeVisible();
    await expect(page.getByText(/Control your hosted events/i)).toBeVisible();
    await expect(page.getByRole("heading", { name: "Board game brunch" })).toBeVisible();
  });

  test("creates a meetup with mocked API responses", async ({ page }) => {
    await seedAuth(page);
    await mockProfile(page);
    await mockMeetups(page);

    await page.goto("/meetups");

    await expect(page.getByText(/Plan a new experience/i)).toBeVisible();

    await page.getByLabel(/^Title$/i).fill("Sketch walk");
    await page.getByLabel(/Location name/i).fill("Waterfront");
    await page.getByLabel(/Event date/i).fill("2026-06-30");
    await page.getByLabel(/Start time/i).fill("10:30");
    await page.getByLabel(/^Name$/i).fill("Sketching");

    await page.getByRole("button", { name: /Create meetup/i }).click();

    await expect(page.getByText(/Meetup created/i)).toBeVisible();
    await expect(page.getByText(/Control your hosted events/i)).toBeVisible();
  });
});
