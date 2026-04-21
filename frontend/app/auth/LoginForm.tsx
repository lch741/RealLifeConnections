"use client";

import { SubmitEvent, useState } from "react";
import Link from "next/link";
import Toast, { type ToastState } from "./Toast";
import { loginUser, saveAuthSession } from "../lib/auth-api";

export default function LoginForm() {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [toast, setToast] = useState<ToastState | null>(null);

  function showToast(nextToast: ToastState) {
    setToast(nextToast);
    globalThis.setTimeout(() => setToast(null), 3600);
  }

  async function handleSubmit(event: SubmitEvent<HTMLFormElement>) {
    event.preventDefault();
    setIsSubmitting(true);

    try {
      const auth = await loginUser({ email, password });
      saveAuthSession(auth);
      showToast({
        tone: "success",
        message: auth.message || "Login successful.",
      });
    } catch (error) {
      showToast({
        tone: "error",
        message:
          error instanceof Error ? error.message : "Unable to login right now.",
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
        <div className="space-y-5">
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
              Password
            </span>
            <input
              className="mt-2 h-12 w-full rounded-md border border-zinc-300 px-3 text-base outline-none transition focus:border-emerald-600 focus:ring-4 focus:ring-emerald-100"
              type="password"
              value={password}
              onChange={(event) => setPassword(event.target.value)}
              autoComplete="current-password"
              required
            />
          </label>
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
              d="M15.75 9V5.25A2.25 2.25 0 0 0 13.5 3h-6A2.25 2.25 0 0 0 5.25 5.25v13.5A2.25 2.25 0 0 0 7.5 21h6a2.25 2.25 0 0 0 2.25-2.25V15m3-3H9.75m9 0-3-3m3 3-3 3"
            />
          </svg>
          {isSubmitting ? "Signing in..." : "Sign in"}
        </button>

        <p className="mt-5 text-center text-sm text-zinc-600">
          New here?{" "}
          <Link className="font-semibold text-emerald-800" href="/register">
            Create an account
          </Link>
        </p>
      </form>
    </>
  );
}
