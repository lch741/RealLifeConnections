import { expect, test } from "@playwright/test";
import { mockChat, mockProfile, seedAuth } from "./helpers";

test.describe("chat e2e", () => {
  test("loads meetup conversations", async ({ page }) => {
    await seedAuth(page);
    await mockProfile(page);
    await mockChat(page);

    await page.goto("/conversations");

    await expect(page.getByRole("heading", { name: /Meetup chats/i })).toBeVisible();
    await expect(page.getByText("Board game brunch")).toBeVisible();
    await expect(page.getByText("Taylor")).toBeVisible();
  });

  test("opens a chat and sends a message", async ({ page }) => {
    await seedAuth(page);
    await mockProfile(page);
    await mockChat(page);

    await page.goto("/conversations/10/2");

    await expect(page.getByText("Chat with Taylor")).toBeVisible();
    await expect(page.getByText("See you near the front desk?")).toBeVisible();

    const messageBox = page.getByPlaceholder(/Write a message/i);
    await messageBox.fill("I'll bring the cards.");
    await page.getByRole("button", { name: /^Send$/i }).click();

    await expect(messageBox).toHaveValue("");
  });
});
