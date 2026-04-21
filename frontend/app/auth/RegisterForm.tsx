"use client";

import { SubmitEvent, useMemo, useState } from "react";
import Link from "next/link";
import Toast, { type ToastState } from "./Toast";
import { registerUser, saveAuthSession } from "../lib/auth-api";

const categories = [
  { id: 1, name: "Sports" },
  { id: 2, name: "Art" },
  { id: 3, name: "Music" },
  { id: 4, name: "Technology" },
  { id: 5, name: "Gaming" },
  { id: 6, name: "Fitness" },
  { id: 7, name: "Travel" },
  { id: 8, name: "Other" },
];

export default function RegisterForm() {
  const [email, setEmail] = useState("");
  const [userName, setUserName] = useState("");
  const [password, setPassword] = useState("");
  const [city, setCity] = useState("");
  const [bio, setBio] = useState("");
  const [categoryId, setCategoryId] = useState(1);
  const [interests, setInterests] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [toast, setToast] = useState<ToastState | null>(null);

  const parsedInterests = useMemo(
    () =>
      interests
        .split(",")
        .map((interest) => interest.trim())
        .filter(Boolean)
        .slice(0, 3),
    [interests],
  );

  function showToast(nextToast: ToastState) {
    setToast(nextToast);
    globalThis.setTimeout(() => setToast(null), 3600);
  }

  async function handleSubmit(event: SubmitEvent<HTMLFormElement>) {
    event.preventDefault();

    if (parsedInterests.length === 0) {
      showToast({
        tone: "error",
        message: "Please add at least one interest.",
      });
      return;
    }

    setIsSubmitting(true);

    try {
      const auth = await registerUser({
        email,
        userName,
        password,
        city,
        bio: bio.trim() || undefined,
        interestSelections: [
          {
            categoryId,
            interests: parsedInterests,
          },
        ],
      });

      saveAuthSession(auth);
      showToast({
        tone: "success",
        message: auth.message || "Registration successful.",
      });
    } catch (error) {
      showToast({
        tone: "error",
        message:
          error instanceof Error
            ? error.message
            : "Unable to register right now.",
      });
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <>
      <Toast toast={toast} />
      <form
        className="rounded-lg border border-zinc-200 bg-white p-6 shadow-xl shadow-zinc-200/70 sm:p-8"
        onSubmit={handleSubmit}
      >
        <div className="grid gap-5 sm:grid-cols-2">
          <label className="block">
            <span className="text-sm font-semibold text-zinc-800">Email</span>
            <input
              className="mt-2 h-12 w-full rounded-md border border-zinc-300 px-3 text-base outline-none transition focus:border-emerald-600 focus:ring-4 focus:ring-emerald-100"
              type="email"
              value={email}
              onChange={(event) => setEmail(event.target.value)}
              autoComplete="email"
              required
            />
          </label>

          <label className="block">
            <span className="text-sm font-semibold text-zinc-800">
              Username
            </span>
            <input
              className="mt-2 h-12 w-full rounded-md border border-zinc-300 px-3 text-base outline-none transition focus:border-emerald-600 focus:ring-4 focus:ring-emerald-100"
              type="text"
              value={userName}
              onChange={(event) => setUserName(event.target.value)}
              autoComplete="username"
              maxLength={30}
              required
            />
          </label>

          <label className="block">
            <span className="text-sm font-semibold text-zinc-800">
              Password
            </span>
            <input
              className="mt-2 h-12 w-full rounded-md border border-zinc-300 px-3 text-base outline-none transition focus:border-emerald-600 focus:ring-4 focus:ring-emerald-100"
              type="password"
              value={password}
              onChange={(event) => setPassword(event.target.value)}
              autoComplete="new-password"
              minLength={6}
              required
            />
          </label>

          <label className="block">
            <span className="text-sm font-semibold text-zinc-800">City</span>
            <input
              className="mt-2 h-12 w-full rounded-md border border-zinc-300 px-3 text-base outline-none transition focus:border-emerald-600 focus:ring-4 focus:ring-emerald-100"
              type="text"
              value={city}
              onChange={(event) => setCity(event.target.value)}
              autoComplete="address-level2"
              required
            />
          </label>
        </div>

        <div className="mt-5 grid gap-5 sm:grid-cols-[0.8fr_1.2fr]">
          <label className="block">
            <span className="text-sm font-semibold text-zinc-800">
              Interest category
            </span>
            <select
              className="mt-2 h-12 w-full rounded-md border border-zinc-300 bg-white px-3 text-base outline-none transition focus:border-emerald-600 focus:ring-4 focus:ring-emerald-100"
              value={categoryId}
              onChange={(event) => setCategoryId(Number(event.target.value))}
            >
              {categories.map((category) => (
                <option key={category.id} value={category.id}>
                  {category.name}
                </option>
              ))}
            </select>
          </label>

          <label className="block">
            <span className="text-sm font-semibold text-zinc-800">
              Interests
            </span>
            <input
              className="mt-2 h-12 w-full rounded-md border border-zinc-300 px-3 text-base outline-none transition focus:border-emerald-600 focus:ring-4 focus:ring-emerald-100"
              type="text"
              value={interests}
              onChange={(event) => setInterests(event.target.value)}
              placeholder="hiking, photography, coffee"
              required
            />
          </label>
        </div>

        <label className="mt-5 block">
          <span className="text-sm font-semibold text-zinc-800">Bio</span>
          <textarea
            className="mt-2 min-h-28 w-full resize-y rounded-md border border-zinc-300 px-3 py-3 text-base outline-none transition focus:border-emerald-600 focus:ring-4 focus:ring-emerald-100"
            value={bio}
            onChange={(event) => setBio(event.target.value)}
            maxLength={300}
          />
        </label>

        <div className="mt-4 rounded-md bg-zinc-100 px-3 py-2 text-sm text-zinc-700">
          Selected interests:{" "}
          <span className="font-semibold">
            {parsedInterests.length > 0 ? parsedInterests.join(", ") : "None"}
          </span>
        </div>

        <button
          className="mt-7 flex h-12 w-full items-center justify-center gap-2 rounded-md bg-emerald-700 px-4 text-base font-semibold text-white transition hover:bg-emerald-800 disabled:cursor-not-allowed disabled:bg-zinc-400"
          type="submit"
          disabled={isSubmitting}
        >
          <svg
            aria-hidden="true"
            className="h-5 w-5"
            fill="none"
            viewBox="0 0 24 24"
            stroke="currentColor"
            strokeWidth="2"
          >
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              d="M18 7.5v3m0 0v3m0-3h3m-3 0h-3M12 6.75A3.75 3.75 0 1 1 4.5 6.75a3.75 3.75 0 0 1 7.5 0ZM3 20.25a7.5 7.5 0 0 1 15 0"
            />
          </svg>
          {isSubmitting ? "Creating account..." : "Create account"}
        </button>

        <p className="mt-5 text-center text-sm text-zinc-600">
          Already have an account?{" "}
          <Link className="font-semibold text-emerald-800" href="/login">
            Sign in
          </Link>
        </p>
      </form>
    </>
  );
}
