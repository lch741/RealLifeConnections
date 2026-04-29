"use client";

import Link from "next/link";
import { useEffect, useMemo, useState } from "react";
import Toast, { type ToastState } from "../../components/Toast";
import {
  getProfile,
  type InterestSelection,
  type UpdateProfilePayload,
  updateProfile,
  type UserProfile,
} from "../lib/profile-api";
import { categories, cultures, genders } from "../lib/profile-options";
import ProtectedRoute from "@/components/ProtectedRoute";

type InterestSelectionState = {
  categoryId: number;
  interests: string;
};

function logoutAndRedirect() {
  localStorage.removeItem("authToken");
  localStorage.removeItem("authUser");
  globalThis.location.replace("/login");
}

function mapProfileToInterestState(profile: UserProfile): InterestSelectionState[] {
  if (profile.interestSelections.length === 0) {
    return [{ categoryId: 1, interests: "" }];
  }

  return profile.interestSelections.map((selection) => ({
    categoryId: selection.categoryId,
    interests: selection.interests.join(", "),
  }));
}

function normalizeInterestSelections(
  selections: InterestSelectionState[],
): InterestSelection[] {
  return selections
    .map((selection) => ({
      categoryId: selection.categoryId,
      interests: selection.interests
        .split(",")
        .map((interest) => interest.trim())
        .filter(Boolean)
        .slice(0, 3),
    }))
    .filter((selection) => selection.interests.length > 0);
}

function normalizeGender(value?: string | null) {
  return value && value.trim().length > 0 ? value : "NotToTell";
}

export default function ProfilePage() {
  const [profile, setProfile] = useState<UserProfile | null>(null);
  const [userName, setUserName] = useState("");
  const [city, setCity] = useState("online");
  const [bio, setBio] = useState("");
  const [gender, setGender] = useState("NotToTell");
  const [age, setAge] = useState("");
  const [culture, setCulture] = useState("");
  const [interestSelections, setInterestSelections] = useState<InterestSelectionState[]>([
    { categoryId: 1, interests: "" },
  ]);
  const [isLoading, setIsLoading] = useState(true);
  const [isSaving, setIsSaving] = useState(false);
  const [toast, setToast] = useState<ToastState | null>(null);

  const parsedInterestSelections = useMemo(
    () => normalizeInterestSelections(interestSelections),
    [interestSelections],
  );

  function showToast(nextToast: ToastState) {
    setToast(nextToast);
    globalThis.setTimeout(() => setToast(null), 3600);
  }

  useEffect(() => {
    let isMounted = true;

    async function loadProfile() {
      try {
        const data = await getProfile();

        if (!isMounted) {
          return;
        }

        setProfile(data);
        setUserName(data.userName ?? "");
        setCity(data.city || "online");
        setBio(data.bio ?? "");
        setGender(normalizeGender(data.gender));
        setAge(data.age?.toString() ?? "");
        setCulture(data.culture ?? "");
        setInterestSelections(mapProfileToInterestState(data));
      } catch (error) {
        if (!isMounted) {
          return;
        }

        showToast({
          tone: "error",
          message:
            error instanceof Error ? error.message : "Unable to load profile.",
        });
      } finally {
        if (isMounted) {
          setIsLoading(false);
        }
      }
    }

    loadProfile();

    return () => {
      isMounted = false;
    };
  }, []);

  async function handleSubmit(event: { preventDefault(): void }) {
    event.preventDefault();

    if (parsedInterestSelections.length === 0) {
      showToast({
        tone: "error",
        message: "Please add at least one interest category.",
      });
      return;
    }

    setIsSaving(true);

    try {
      const payload: UpdateProfilePayload = {
        userName: userName.trim() || undefined,
        city: city.trim() || "online",
        bio: bio.trim() || undefined,
        gender: gender || undefined,
        age: age ? Number(age) : undefined,
        culture: (culture as any) || undefined,
        interestSelections: parsedInterestSelections,
      };

      const updatedProfile = await updateProfile(payload);
      setProfile(updatedProfile);
      setUserName(updatedProfile.userName ?? "");
      setCity(updatedProfile.city || "online");
      setBio(updatedProfile.bio ?? "");
      setGender(normalizeGender(updatedProfile.gender));
      setAge(updatedProfile.age?.toString() ?? "");
      setCulture(updatedProfile.culture ?? "");
      setInterestSelections(mapProfileToInterestState(updatedProfile));

      showToast({
        tone: "success",
        message: "Profile updated successfully.",
      });
    } catch (error) {
      showToast({
        tone: "error",
        message:
          error instanceof Error ? error.message : "Unable to save profile.",
      });
    } finally {
      setIsSaving(false);
    }
  }

  async function cancelEdit() {
    if (!profile) {
      return;
    }
    try {
        const data = await getProfile();
        setProfile(data);
        setUserName(data.userName ?? "");
        setCity(data.city || "online");
        setBio(data.bio ?? "");
        setGender(normalizeGender(data.gender));
        setAge(data.age?.toString() ?? "");
        setCulture(data.culture ?? "");
        setInterestSelections(mapProfileToInterestState(data));
        showToast({
          tone: "info",
          message: "Changes discarded.",
        });
      } catch (error) {
        showToast({
          tone: "error",
          message:
            error instanceof Error ? error.message : "Unable to load profile.",
        });
      }
  }

  return (
    <ProtectedRoute>
    <main className="min-h-screen bg-[#f8faf7] text-zinc-950">
      <div className="mx-auto flex min-h-screen w-full max-w-5xl flex-col px-5 py-6 sm:px-8 lg:px-10">
        <header className="flex items-center justify-between gap-4 border-b border-zinc-200 pb-5">
          <Link className="text-lg font-semibold tracking-wide" href="/">
            RealLifeConnections
          </Link>
          <button
            type="button"
            className="rounded-md border border-zinc-300 bg-white px-4 py-2 text-sm font-semibold text-zinc-900 shadow-sm transition hover:border-zinc-500"
            onClick={logoutAndRedirect}
          >
            Logout
          </button>
        </header>

        <Toast toast={toast} />

        <section className="grid flex-1 gap-8 py-8 lg:grid-cols-[0.9fr_1.1fr]">
          <div className="max-w-xl">
            <p className="text-sm font-semibold uppercase tracking-[0.18em] text-emerald-700">
              Profile
            </p>
            <h1 className="mt-4 text-4xl font-bold leading-tight text-zinc-950 sm:text-5xl">
              Edit your information.
            </h1>
            <p className="mt-5 text-lg leading-8 text-zinc-650">
              Update your city, bio, username, and interests from one place.
            </p>

            {profile ? (
              <div className="mt-8 rounded-2xl border border-zinc-200 bg-white p-5 shadow-sm">
                <p className="text-sm font-semibold text-zinc-500">Current status</p>
                <div className="mt-3 space-y-2 text-sm text-zinc-700">
                  <p>
                    Verification: <span className="font-semibold">{profile.verificationStatus}</span>
                  </p>
                  <p>
                    Can match: <span className="font-semibold">{profile.canMatch ? "Yes" : "No"}</span>
                  </p>
                </div>
              </div>
            ) : null}
          </div>

          <div className="w-full">
            <form
              className="rounded-3xl border border-zinc-200 bg-white p-6 shadow-xl shadow-zinc-200/70 sm:p-8"
              onSubmit={handleSubmit}
            >
              {isLoading ? (
                <p className="text-sm text-zinc-600">Loading profile...</p>
              ) : (
                <div className="space-y-5">
                  <div className="grid gap-5 sm:grid-cols-2">
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
                      />
                    </label>

                    <label className="block">
                      <span className="text-sm font-semibold text-zinc-800">
                        City
                      </span>
                      <input
                        className="mt-2 h-12 w-full rounded-md border border-zinc-300 px-3 text-base outline-none transition focus:border-emerald-600 focus:ring-4 focus:ring-emerald-100"
                        type="text"
                        value={city}
                        onChange={(event) => setCity(event.target.value)}
                        autoComplete="address-level2"
                        required
                      />
                    </label>

                    <label className="block">
                      <span className="text-sm font-semibold text-zinc-800">
                        Gender
                      </span>
                      <select
                        className="mt-2 h-12 w-full rounded-md border border-zinc-300 bg-white px-3 text-base outline-none transition focus:border-emerald-600 focus:ring-4 focus:ring-emerald-100"
                        value={gender}
                        onChange={(event) => setGender(event.target.value)}
                      >
                        {genders.map((option) => (
                          <option key={option.value} value={option.value}>
                            {option.label}
                          </option>
                        ))}
                      </select>
                    </label>

                    <label className="block">
                      <span className="text-sm font-semibold text-zinc-800">
                        Age
                      </span>
                      <input
                        className="mt-2 h-12 w-full rounded-md border border-zinc-300 px-3 text-base outline-none transition focus:border-emerald-600 focus:ring-4 focus:ring-emerald-100"
                        type="number"
                        min={18}
                        max={120}
                        value={age}
                        onChange={(event) => setAge(event.target.value)}
                        placeholder="e.g. 28"
                      />
                    </label>
                  </div>

                  <label className="block">
                    <span className="text-sm font-semibold text-zinc-800">
                      Culture
                    </span>
                    <select
                      className="mt-2 h-12 w-full rounded-md border border-zinc-300 bg-white px-3 text-base outline-none transition focus:border-emerald-600 focus:ring-4 focus:ring-emerald-100"
                      value={culture}
                      onChange={(event) => setCulture(event.target.value)}
                    >
                      <option value="">Select (optional)</option>
                      {cultures.map((option) => (
                        <option key={option} value={option}>
                          {option}
                        </option>
                      ))}
                    </select>
                  </label>

                  <label className="block">
                    <span className="text-sm font-semibold text-zinc-800">
                      Bio
                    </span>
                    <textarea
                      className="mt-2 min-h-28 w-full rounded-md border border-zinc-300 px-3 py-3 text-base outline-none transition focus:border-emerald-600 focus:ring-4 focus:ring-emerald-100"
                      value={bio}
                      onChange={(event) => setBio(event.target.value)}
                      maxLength={300}
                      placeholder="Tell people a bit about yourself"
                    />
                  </label>

                  <div className="space-y-4">
                    {interestSelections.map((selection, index) => (
                      <div
                        key={`${selection.categoryId}-${index}`}
                        className="grid gap-4 rounded-2xl border border-zinc-200 p-4 sm:grid-cols-[0.8fr_1.2fr]"
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

                  <div className="flex flex-wrap gap-3">
                    <button
                      type="button"
                      className="rounded-md border border-emerald-700 px-4 py-2 text-sm font-semibold text-emerald-800 transition hover:bg-emerald-50 disabled:opacity-50"
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
                      className="rounded-md border border-red-600 px-4 py-2 text-sm font-semibold text-red-600 transition hover:bg-red-50 disabled:opacity-50"
                      onClick={() => {
                        if (interestSelections.length > 1) {
                          setInterestSelections(interestSelections.slice(0, -1));
                        }
                      }}
                      disabled={interestSelections.length <= 1}
                    >
                      Remove category
                    </button>
                  </div>

                  <div className="mt-2 flex gap-3">
                    <button
                      className="flex h-12 flex-1 items-center justify-center gap-2 rounded-md bg-emerald-700 px-4 text-base font-semibold text-white transition hover:bg-emerald-800 disabled:cursor-not-allowed disabled:bg-zinc-400"
                      type="submit"
                      disabled={isSaving}
                    >
                      {isSaving ? "Saving changes..." : "Save changes"}
                    </button>
                    <button
                      className="flex h-12 flex-1 items-center justify-center gap-2 rounded-md bg-slate-600 px-4 text-base font-semibold text-white transition hover:bg-slate-700 disabled:cursor-not-allowed disabled:bg-zinc-400"
                      type="button"
                      onClick={cancelEdit}
                    >
                      Discard changes
                    </button>
                  </div>
                </div>
              )}
            </form>
          </div>
        </section>
      </div>
    </main>
    </ProtectedRoute>
  );
}