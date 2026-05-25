"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import type { UserProfile } from "@/app/lib/profile-api";

const verificationStyles = {
  verified: "bg-emerald-100 text-emerald-700",
  unverified: "bg-amber-100 text-amber-700",
};

type TopBarProps = {
  profile: UserProfile | null;
  showBackLink?: boolean;
};

export default function TopBar({ profile, showBackLink = true }: Readonly<TopBarProps>) {
  const router = useRouter();

  function handleLogout() {
    localStorage.removeItem("authToken");
    localStorage.removeItem("authUser");
    router.replace("/login");
  }

  const isVerified = Boolean(profile?.isVerified);
  const verificationClass = isVerified
    ? verificationStyles.verified
    : verificationStyles.unverified;

  return (
    <div className="flex flex-wrap items-center justify-between gap-4 rounded-2xl border border-zinc-200 bg-white/80 px-4 py-3 shadow-sm">
      <div className="flex items-center gap-3">
        <span className={`rounded-full px-3 py-1 text-xs font-semibold ${verificationClass}`}>
          {isVerified ? "Verified" : "Unverified"}
        </span>
        {showBackLink ? (
          <Link
            className="rounded-full border border-zinc-300 bg-white px-3 py-1 text-xs font-semibold text-zinc-700 shadow-sm transition hover:border-zinc-400 hover:text-zinc-900"
            href="/profile"
          >
            Back to profile
          </Link>
        ) : null}
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
