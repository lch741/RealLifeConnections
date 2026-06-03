import { expect, test } from "@playwright/test";

const apiBaseUrl =
  process.env.SYSTEM_E2E_API_BASE_URL ?? "http://localhost:5118";

function uniqueUser() {
  const suffix = `${Date.now()}-${Math.random().toString(16).slice(2)}`;
  return {
    email: `system-e2e-${suffix}@example.com`,
    userName: `SystemE2E${suffix.slice(-8)}`,
    password: "Password123!!",
  };
}

async function verifyCurrentUser(page: import("@playwright/test").Page) {
  const token = await page.evaluate(() => localStorage.getItem("authToken"));
  expect(token).toBeTruthy();

  const headers = {
    Authorization: `Bearer ${token}`,
  };

  const avatarResponse = await page.request.post(`${apiBaseUrl}/api/user/avatar`, {
    headers,
    data: {
      avatarUrl: "https://example.com/system-e2e-avatar.jpg",
    },
  });
  expect(avatarResponse.ok()).toBeTruthy();

  const verificationResponse = await page.request.post(
    `${apiBaseUrl}/api/user/verify-face`,
    {
      headers,
      data: {
        liveCaptureUrl: "https://example.com/system-e2e-live.jpg",
      },
    },
  );
  expect(verificationResponse.ok()).toBeTruthy();
}

test.describe("full system happy path", () => {
  test("registers, updates profile, verifies, and creates a meetup", async ({
    page,
  }) => {
    const user = uniqueUser();

    await page.goto("/register");

    await page.getByLabel(/Email/i).fill(user.email);
    await page.getByLabel(/Username/i).fill(user.userName);
    await page.getByLabel(/Password/i).fill(user.password);

    const regionSelect = page
      .locator("label")
      .filter({ has: page.locator("span", { hasText: /^Region$/ }) })
      .locator("select");
    const citySelect = page
      .locator("label")
      .filter({ has: page.locator("span", { hasText: /^City$/ }) })
      .locator("select");

    await regionSelect.selectOption("Auckland");
    await expect(
      citySelect.locator("option", { hasText: "Auckland City" }),
    ).toHaveCount(1);
    await citySelect.selectOption("Auckland City");

    await page.getByLabel(/^Interests$/i).fill("coffee, hiking");
    await page.getByRole("button", { name: /Create account/i }).click();

    await expect(page).toHaveURL(/\/profile$/);
    await expect(
      page.getByRole("heading", { name: /Edit your information/i }),
    ).toBeVisible();

    await page.getByLabel(/Bio/i).fill("Created by the full system smoke test.");
    await page.getByRole("button", { name: /Save changes/i }).click();
    await expect(page.getByText(/Profile updated successfully/i)).toBeVisible();

    await verifyCurrentUser(page);

    await page.goto("/meetups");
    await expect(page.getByText(/Plan a new experience/i)).toBeVisible();

    const meetupTitle = `System smoke meetup ${Date.now()}`;
    await page.getByLabel(/^Title$/i).fill(meetupTitle);
    await page.getByLabel(/^Description$/i).first().fill("Created by a full system test.");
    await page.getByLabel(/Location name/i).fill("System Test Cafe");
    await page.getByLabel(/Event date/i).fill("2026-07-15");
    await page.getByLabel(/Start time/i).fill("10:00");
    await page.getByLabel(/End time/i).fill("11:30");
    await page.getByLabel(/^Name$/i).fill("Coffee");
    await page.getByRole("button", { name: /Create meetup/i }).click();

    await expect(page.getByText(/Meetup created/i)).toBeVisible();
    await expect(page.getByText(/Control your hosted events/i)).toBeVisible();
    await expect(page.getByRole("heading", { name: meetupTitle })).toBeVisible();
  });
});
