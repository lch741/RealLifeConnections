"use client";

import Link from "next/link";
import { usePathname, useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import type { UserProfile } from "@/app/lib/profile-api";
import { getConversations } from "@/app/lib/chat-api";

const verificationStyles = {
  verified: "bg-emerald-100 text-emerald-700",
  unverified: "bg-amber-100 text-amber-700",
};

type TopBarProps = {
  profile: UserProfile | null;
  showBackLink?: boolean;
  showConversationsLink?: boolean;
};

function getLastSeenKey(meetupId: number, otherUserId: number) {
  return `chat:lastSeen:${meetupId}:${otherUserId}`;
}

function getLastSeenAt(meetupId: number, otherUserId: number) {
  const value = localStorage.getItem(getLastSeenKey(meetupId, otherUserId));
  if (!value) return null;
  const date = new Date(value);
  return Number.isNaN(date.getTime()) ? null : date;
}

export default function TopBar({
  profile,
  showBackLink = true,
  showConversationsLink = true,
}: Readonly<TopBarProps>) {
  const router = useRouter();
  const pathname = usePathname();
  const [unreadCount, setUnreadCount] = useState(0);

  function handleLogout() {
    localStorage.removeItem("authToken");
    localStorage.removeItem("authUser");
    router.replace("/login");
  }

  const isVerified = Boolean(profile?.isVerified);
  const verificationClass = isVerified
    ? verificationStyles.verified
    : verificationStyles.unverified;
  const buttonBaseClass =
    "relative rounded-md border border-emerald-600 bg-emerald-600 px-3 py-1 text-xs font-semibold text-white shadow-sm transition hover:border-emerald-700 hover:bg-emerald-700";
  const buttonActiveClass = "border-emerald-800 bg-emerald-800";
  const isProfileActive = pathname === "/profile";
  const isMeetupsActive = pathname.startsWith("/meetups");
  const isConversationsActive = pathname.startsWith("/conversations");

  useEffect(() => {
    let isMounted = true;

    async function refreshUnread() {
      try {
        const conversations = await getConversations();
        let count = 0;
        conversations.forEach((conversation) => {
          if (!conversation.meetupEventId) return;
          const lastMessageAt = new Date(conversation.lastMessageAt);
          if (Number.isNaN(lastMessageAt.getTime())) return;
          const lastSeenAt = getLastSeenAt(
            conversation.meetupEventId,
            conversation.otherUserId,
          );
          if (!lastSeenAt || lastMessageAt > lastSeenAt) {
            count += 1;
          }
        });
        if (isMounted) {
          setUnreadCount(count);
        }
      } catch {
        if (isMounted) {
          setUnreadCount(0);
        }
      }
    }

    function handleVisibilityChange() {
      if (document.visibilityState === "visible") {
        refreshUnread();
      }
    }

    function handleStorage(event: StorageEvent) {
      if (!event.key || event.key.startsWith("chat:lastSeen:")) {
        refreshUnread();
      }
    }

    refreshUnread();
    const intervalId = globalThis.setInterval(refreshUnread, 12000);
    document.addEventListener("visibilitychange", handleVisibilityChange);
    globalThis.addEventListener("storage", handleStorage);
    return () => {
      isMounted = false;
      globalThis.clearInterval(intervalId);
      document.removeEventListener("visibilitychange", handleVisibilityChange);
      globalThis.removeEventListener("storage", handleStorage);
    };
  }, []);

  return (
    <div className="flex flex-wrap items-center justify-between gap-4 rounded-2xl border border-zinc-200 bg-white/80 px-4 py-3 shadow-sm">
      <div className="flex items-center gap-3">
        <span className={`rounded-full px-3 py-1 text-xs font-semibold ${verificationClass}`}>
          {isVerified ? "Verified" : "Unverified"}
        </span>
        {showBackLink ? (
          <Link
            className={`${buttonBaseClass} ${isProfileActive ? buttonActiveClass : ""}`}
            href="/profile"
          >
            Back to profile
          </Link>
        ) : null}
        {showConversationsLink ? (
          <Link
            className={`${buttonBaseClass} ${isConversationsActive ? buttonActiveClass : ""}`}
            href="/conversations"
          >
            Conversations
            {unreadCount > 0 && !isConversationsActive ? (
              <span className="absolute -right-2 -top-2 rounded-full bg-rose-500 px-1.5 py-0.5 text-[10px] font-semibold text-white">
                {unreadCount}
              </span>
            ) : null}
          </Link>
        ) : null}
        <Link
          className={`${buttonBaseClass} ${isMeetupsActive ? buttonActiveClass : ""}`}
          href="/meetups"
        >
          Meetups
        </Link>
      </div>
      <div className="flex items-center gap-3">
        <div className="flex flex-col items-end">
          <span className="text-xs text-zinc-500">Signed in as</span>
          <span className="text-sm font-semibold text-zinc-900">
            {profile?.userName ?? "User"}
          </span>
        </div>
        {profile?.avatarUrl ? (
          <img
            src={profile.avatarUrl}
            alt={profile.userName ?? "User"}
            className="h-10 w-10 rounded-full object-cover"
          />
        ) : (
          <div className="flex h-10 w-10 items-center justify-center rounded-full bg-zinc-200 text-sm font-semibold text-zinc-700">
            {(profile?.userName ?? "U").slice(0, 1).toUpperCase()}
          </div>
        )}
        <button
          type="button"
          className="rounded-md border border-zinc-300 bg-white px-3 py-2 text-xs font-semibold text-zinc-900 shadow-sm transition hover:border-zinc-500"
          onClick={handleLogout}
        >
          Logout
        </button>
      </div>
    </div>
  );
}
