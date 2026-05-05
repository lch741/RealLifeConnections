"use client";

import { SubmitEvent, useEffect, useMemo, useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import Toast, { type ToastState } from "../../components/Toast";
import { registerUser, saveAuthSession } from "../lib/auth-api";
import { categories, cultures, genders } from "../lib/profile-options";
import { nzLocations } from "../lib/nz-locations";

export default function RegisterForm() {
  const router = useRouter();
  const [email, setEmail] = useState("");
  const [userName, setUserName] = useState("");
  const [password, setPassword] = useState("");
  const [region, setRegion] = useState<string>(nzLocations[0]?.region ?? "Auckland");
  const [city, setCity] = useState<string>("");
  const citiesForRegion = useMemo(
    () => nzLocations.find((l) => l.region === region)?.cities ?? [],
    [region],
  );
  const [gender, setGender] = useState("NotToTell");
  const [age, setAge] = useState<string>("");
  const [culture, setCulture] = useState("");
  const [bio] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [toast, setToast] = useState<ToastState | null>(null);

  useEffect(() => {
    const token = localStorage.getItem("authToken");
    if (token) {
      router.replace("/profile");
    }
  }, [router]);


  type InterestSelectionState = {
    categoryId: number;
    interests: string;
  };

  const [interestSelections, setInterestSelections] = useState<InterestSelectionState[]>([
    { categoryId: 1, interests: "" },
  ]);
  
  const parsedInterestSelections = useMemo(
  () =>
    interestSelections
      .map((selection) => ({
        categoryId: selection.categoryId,
        interests: selection.interests
          .split(",")
          .map((interest) => interest.trim())
          .filter(Boolean)
          .slice(0, 3),
      }))
      .filter((selection) => selection.interests.length > 0),
  [interestSelections],
  );

  function showToast(nextToast: ToastState) {
    setToast(nextToast);
    globalThis.setTimeout(() => setToast(null), 3600);
  }

  async function handleSubmit(event: SubmitEvent<HTMLFormElement>) {
    event.preventDefault();

    if (parsedInterestSelections.length === 0) {
      showToast({
        tone: "error",
        message: "Please add at least one interest category.",
      });
      return;
    }

    setIsSubmitting(true);

    try {
      const auth = await registerUser({
        email,
        userName,
        password,
        region: region.trim() || undefined,
        suburb: city.trim() || undefined,
        bio: bio.trim() || undefined,
        gender: gender || undefined,
        age: age ? Number(age) : undefined,
        culture: (culture as any) || undefined,
        interestSelections: parsedInterestSelections,
      });

      saveAuthSession(auth);
      router.replace("/profile");
      showToast({
        tone: "success",
        message: auth.message || "Registration successful.",
      });
    } catch (error) {
      showToast({
        tone: "error",
        message:
          error instanceof Error ? error.message : "Unable to register right now.",
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
            <span className="text-sm font-semibold text-zinc-800">Region</span>
            <select
              className="mt-2 h-12 w-full rounded-md border border-zinc-300 bg-white px-3 text-base outline-none transition focus:border-emerald-600 focus:ring-4 focus:ring-emerald-100"
              value={region}
              onChange={(e) => {
                setRegion(e.target.value);
                setCity("");
              }}
              required
            >
              {nzLocations.map((l) => (
                <option key={l.region} value={l.region}>
                  {l.region}
                </option>
              ))}
            </select>
          </label>

          <label className="block">
            <span className="text-sm font-semibold text-zinc-800">City</span>
            <select
              className="mt-2 h-12 w-full rounded-md border border-zinc-300 bg-white px-3 text-base outline-none transition focus:border-emerald-600 focus:ring-4 focus:ring-emerald-100"
              value={city}
              onChange={(e) => setCity(e.target.value)}
              required
            >
              <option value="">Select city</option>
              {citiesForRegion.map((c) => (
                <option key={c} value={c}>
                  {c}
                </option>
              ))}
            </select>
          </label>
          <label className="block">
            <span className="text-sm font-semibold text-zinc-800">Gender</span>
            <select
              className="mt-2 h-12 w-full rounded-md border border-zinc-300 bg-white px-3 text-base outline-none transition focus:border-emerald-600 focus:ring-4 focus:ring-emerald-100"
              value={gender}
              onChange={(e) => setGender(e.target.value)}
            >
              {genders.map((g) => (
                <option key={g.value} value={g.value}>
                  {g.label}
                </option>
              ))}
            </select>
          </label>

          <label className="block">
            <span className="text-sm font-semibold text-zinc-800">Age</span>
            <input
              type="number"
              min={18}
              max={120}
              className="mt-2 h-12 w-full rounded-md border border-zinc-300 px-3 text-base outline-none transition focus:border-emerald-600 focus:ring-4 focus:ring-emerald-100"
              value={age}
              onChange={(e) => setAge(e.target.value)}
              placeholder="e.g. 28"
            />
          </label>
        </div>

        <label className="block mt-4">
          <span className="text-sm font-semibold text-zinc-800">Culture</span>
          <select
            className="mt-2 h-12 w-full rounded-md border border-zinc-300 bg-white px-3 text-base outline-none transition focus:border-emerald-600 focus:ring-4 focus:ring-emerald-100"
            value={culture}
            onChange={(e) => setCulture(e.target.value)}
          >
            <option value="">Select (optional)</option>
            {cultures.map((c) => (
              <option key={c} value={c}>
                {c}
              </option>
            ))}
          </select>
        </label>

        <div className="mt-5 space-y-4">
          {interestSelections.map((selection, index) => (
          <div
            key={`${selection.categoryId}-${index}`}
            className="grid gap-4 sm:grid-cols-[0.8fr_1.2fr] rounded-md border border-zinc-200 p-4"
          >
            <label className="block">
              <span className="text-sm font-semibold text-zinc-800">
                Interest category {index + 1}
              </span>
              <select
                className="mt-2 h-12 w-full rounded-md border border-zinc-300 bg-white px-3 text-base outline-none transition focus:border-emerald-600 focus:ring-4 focus:ring-emerald-100"
                value={selection.categoryId}
                onChange={(event) => {
                  const nextSelections = [...interestSelections];
                  nextSelections[index] = {
                    ...selection,
                    categoryId: Number(event.target.value),
                  };
                  setInterestSelections(nextSelections);
                }}
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
                value={selection.interests}
                onChange={(event) => {
                  const nextSelections = [...interestSelections];
                  nextSelections[index] = {
                    ...selection,
                    interests: event.target.value,
                  };
                  setInterestSelections(nextSelections);
                }}
                placeholder="hiking, photography, coffee"
                required
              />
            </label>
          </div>
        ))}
        </div>
        
        <button
          type="button"
          className="mt-4 rounded-md border border-emerald-700 px-4 py-2 text-sm font-semibold text-emerald-800 disabled:opacity-50"
          onClick={() => {
            if (interestSelections.length < 3) {
              setInterestSelections([
                ...interestSelections,
                { categoryId: 1, interests: "" },
              ]);
            }
          }}
          disabled={interestSelections.length >= 3}
        >
          Add category
        </button>

        <button
          type="button"
          className="mt-4 ml-4 rounded-md border border-red-600 px-4 py-2 text-sm font-semibold text-red-600 disabled:opacity-50"
          onClick={() => {
            if (interestSelections.length > 1) {
              setInterestSelections(interestSelections.slice(0, -1));
            }
          }}
          disabled={interestSelections.length <= 1}
        >
          Remove category
        </button>



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
