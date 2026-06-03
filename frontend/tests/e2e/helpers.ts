import type { Page } from "@playwright/test";

export const e2eProfile = {
  id: 1,
  email: "alex@example.com",
  userName: "Alex",
  region: "Auckland",
  suburb: "Auckland City",
  bio: "Coffee, walks, and small creative projects.",
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
    preferredDaysOfWeek: "Weekend",
    preferredTimeOfDay: "Morning",
    preferredDistanceKm: 15,
  },
  interestSelections: [
    {
      categoryId: 1,
      categoryName: "Sports",
      interests: ["hiking"],
    },
  ],
};

export const e2eMeetup = {
  id: 10,
  title: "Board game brunch",
  description: "A relaxed weekend meetup.",
  region: "Auckland",
  suburb: "Auckland City",
  locationName: "Central Library",
  activities: [{ id: 1, name: "Board games", description: null, type: "Cafe" }],
  eventDate: "2026-06-12T00:00:00Z",
  startTime: "10:00:00",
  endTime: null,
  maxParticipants: 6,
  currentParticipants: 2,
  maxDistanceKm: 15,
  status: "Open",
  confirmedAt: null,
  completedAt: null,
  createdAt: "2026-06-01T00:00:00Z",
  updatedAt: "2026-06-01T00:00:00Z",
  creatorId: 1,
  creatorName: "Alex",
  participants: [
    {
      id: 1,
      userId: 1,
      userName: "Alex",
      status: "Approved",
      joinedAt: "2026-06-01T00:00:00Z",
      isConfirmed: true,
      confirmedAt: null,
    },
    {
      id: 2,
      userId: 2,
      userName: "Taylor",
      status: "Approved",
      joinedAt: "2026-06-02T00:00:00Z",
      isConfirmed: true,
      confirmedAt: null,
    },
  ],
  locationSuggestions: [],
};

export async function seedAuth(page: Page) {
  await page.addInitScript(() => {
    window.localStorage.setItem("authToken", "e2e-token");
    window.localStorage.setItem(
      "authUser",
      JSON.stringify({
        id: 1,
        email: "alex@example.com",
        userName: "Alex",
      }),
    );
  });
}

export async function mockProfile(page: Page, profile = e2eProfile) {
  await page.route("**/api/user/profile", async (route) => {
    if (route.request().method() === "PUT") {
      await route.fulfill({
        status: 200,
        contentType: "application/json",
        body: JSON.stringify({
          ...profile,
          ...(await route.request().postDataJSON()),
        }),
      });
      return;
    }

    await route.fulfill({
      status: 200,
      contentType: "application/json",
      body: JSON.stringify(profile),
    });
  });
}

export async function mockMeetups(page: Page) {
  await page.route("**/api/meetups/created", async (route) => {
    await route.fulfill({
      status: 200,
      contentType: "application/json",
      body: JSON.stringify([e2eMeetup]),
    });
  });

  await page.route("**/api/meetups/joined", async (route) => {
    await route.fulfill({
      status: 200,
      contentType: "application/json",
      body: JSON.stringify([e2eMeetup]),
    });
  });

  await page.route("**/api/meetups", async (route) => {
    await route.fulfill({
      status: 200,
      contentType: "application/json",
      body: JSON.stringify(e2eMeetup),
    });
  });
}

export async function mockChat(page: Page) {
  await page.route("**/api/chat/conversations", async (route) => {
    await route.fulfill({
      status: 200,
      contentType: "application/json",
      body: JSON.stringify([
        {
          conversationId: 7,
          meetupEventId: 10,
          meetupTitle: "Board game brunch",
          meetupStatus: "Open",
          otherUserId: 2,
          otherUserName: "Taylor",
          lastMessageAt: "2026-06-02T12:00:00Z",
          isClosed: false,
          isExpired: false,
          endsAt: null,
        },
      ]),
    });
  });

  await page.route("**/api/chat/meetups/10/2", async (route) => {
    await route.fulfill({
      status: 200,
      contentType: "application/json",
      body: JSON.stringify([
        {
          id: 1,
          senderId: 2,
          content: "See you near the front desk?",
          createdAt: "2026-06-02T12:00:00Z",
        },
      ]),
    });
  });

  await page.route("**/api/chat/send", async (route) => {
    await route.fulfill({
      status: 200,
      contentType: "application/json",
      body: JSON.stringify({ message: "Message sent." }),
    });
  });

  await page.route("**/api/meetups/10", async (route) => {
    await route.fulfill({
      status: 200,
      contentType: "application/json",
      body: JSON.stringify(e2eMeetup),
    });
  });
}
