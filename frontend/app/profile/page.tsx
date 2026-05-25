"use client";

import Link from "next/link";
import { useEffect, useMemo, useRef, useState } from "react";
import { useRouter } from "next/navigation";
import Toast, { type ToastState } from "../../components/Toast";
import {
  getProfile,
  type InterestSelection,
  saveAvatarUpload,
  type UpdateProfilePayload,
  updateProfile,
  type UserProfile,
  verifyFaceUpload,
} from "../lib/profile-api";
import { categories, cultures, genders, type CultureOption } from "../lib/profile-options";
import { nzLocations } from "../lib/nz-locations";
import ProtectedRoute from "@/components/ProtectedRoute";
import TopBar from "@/components/TopBar";
type InterestSelectionState = {
  categoryId: number;
  interests: string;
};

type PersonalityState = {
  chillToEnergetic: number;
  talkativeToQuiet: number;
  plannerToSpontaneous: number;
  introvertToExtrovert: number;
  preferredDaysOfWeek: string;
  preferredTimeOfDay: string;
  preferredDistanceKm: string;
};

const personalityTraits: Array<{
  key: keyof Pick<
    PersonalityState,
    | "chillToEnergetic"
    | "talkativeToQuiet"
    | "plannerToSpontaneous"
    | "introvertToExtrovert"
  >;
  label: string;
  leftLabel: string;
  rightLabel: string;
}> = [
  {
    key: "chillToEnergetic",
    label: "Chill ↔ Energetic",
    leftLabel: "Chill",
    rightLabel: "Energetic",
  },
  {
    key: "talkativeToQuiet",
    label: "Talkative ↔ Quiet",
    leftLabel: "Talkative",
    rightLabel: "Quiet",
  },
  {
    key: "plannerToSpontaneous",
    label: "Planner ↔ Spontaneous",
    leftLabel: "Planner",
    rightLabel: "Spontaneous",
  },
  {
    key: "introvertToExtrovert",
    label: "Introvert ↔ Extrovert",
    leftLabel: "Introvert",
    rightLabel: "Extrovert",
  },
];

const preferredDaysOptions = ["Weekday", "Weekend", "Anytime"] as const;
const preferredTimeOptions = ["Morning", "Afternoon", "Evening", "Anytime"] as const;

function createDefaultPersonalityState(): PersonalityState {
  return {
    chillToEnergetic: 50,
    talkativeToQuiet: 50,
    plannerToSpontaneous: 50,
    introvertToExtrovert: 50,
    preferredDaysOfWeek: "",
    preferredTimeOfDay: "",
    preferredDistanceKm: "",
  };
}

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

function mapProfileToPersonalityState(profile: UserProfile): PersonalityState {
  const personality = profile.personality ?? {};

  return {
    ...createDefaultPersonalityState(),
    chillToEnergetic: personality.chillToEnergetic ?? 50,
    talkativeToQuiet: personality.talkativeToQuiet ?? 50,
    plannerToSpontaneous: personality.plannerToSpontaneous ?? 50,
    introvertToExtrovert: personality.introvertToExtrovert ?? 50,
    preferredDaysOfWeek: personality.preferredDaysOfWeek ?? "",
    preferredTimeOfDay: personality.preferredTimeOfDay ?? "",
    preferredDistanceKm: personality.preferredDistanceKm?.toString() ?? "",
  };
}

function buildPersonalityPayload(personality: PersonalityState) {
  return {
    chillToEnergetic: personality.chillToEnergetic,
    talkativeToQuiet: personality.talkativeToQuiet,
    plannerToSpontaneous: personality.plannerToSpontaneous,
    introvertToExtrovert: personality.introvertToExtrovert,
    preferredDaysOfWeek: personality.preferredDaysOfWeek || undefined,
    preferredTimeOfDay: personality.preferredTimeOfDay || undefined,
    preferredDistanceKm: personality.preferredDistanceKm
      ? Number(personality.preferredDistanceKm)
      : undefined,
  };
}

// eslint-disable-next-line sonarjs/cognitive-complexity
export default function ProfilePage() {
  const router = useRouter();
  const [profile, setProfile] = useState<UserProfile | null>(null);
  const [userName, setUserName] = useState("");
  const [region, setRegion] = useState(nzLocations[0]?.region ?? "");
  const [suburb, setSuburb] = useState("");
  const [bio, setBio] = useState("");
  const [gender, setGender] = useState("NotToTell");
  const [age, setAge] = useState("");
  const [culture, setCulture] = useState<CultureOption | "">("");
  const [personality, setPersonality] = useState<PersonalityState>(createDefaultPersonalityState());
  const [interestSelections, setInterestSelections] = useState<InterestSelectionState[]>([
    { categoryId: 1, interests: "" },
  ]);
  const [isLoading, setIsLoading] = useState(true);
  const [isSaving, setIsSaving] = useState(false);
  const [isSavingAvatar, setIsSavingAvatar] = useState(false);
  const [isVerifyingFace, setIsVerifyingFace] = useState(false);
  const [avatarFile, setAvatarFile] = useState<File | null>(null);
  const [avatarPreviewUrl, setAvatarPreviewUrl] = useState<string | null>(null);
  const [cameraCaptureBlob, setCameraCaptureBlob] = useState<Blob | null>(null);
  const [cameraCaptureUrl, setCameraCaptureUrl] = useState<string | null>(null);
  const [cameraError, setCameraError] = useState<string | null>(null);
  const [isCameraActive, setIsCameraActive] = useState(false);
  const [toast, setToast] = useState<ToastState | null>(null);
  const videoRef = useRef<HTMLVideoElement | null>(null);
  const canvasRef = useRef<HTMLCanvasElement | null>(null);
  const streamRef = useRef<MediaStream | null>(null);

  const parsedInterestSelections = useMemo(
    () => normalizeInterestSelections(interestSelections),
    [interestSelections],
  );

  const suburbsForRegion = useMemo(
    () => nzLocations.find((location) => location.region === region)?.cities ?? [],
    [region],
  );

  function showToast(nextToast: ToastState) {
    setToast(nextToast);
    globalThis.setTimeout(() => setToast(null), 3600);
  }

  function applyProfileState(data: UserProfile) {
    setProfile(data);
    setUserName(data.userName ?? "");
    setRegion(data.region || nzLocations[0]?.region || "");
    setSuburb(data.suburb || "");
    setBio(data.bio ?? "");
    setGender(normalizeGender(data.gender));
    setAge(data.age?.toString() ?? "");
    setCulture(data.culture ?? "");
    setPersonality(mapProfileToPersonalityState(data));
    setInterestSelections(mapProfileToInterestState(data));
    setAvatarPreviewUrl(data.avatarUrl ?? null);
  }

  function stopCamera() {
    if (streamRef.current) {
      streamRef.current.getTracks().forEach((track) => track.stop());
      streamRef.current = null;
    }

    if (videoRef.current) {
      videoRef.current.srcObject = null;
    }

    setIsCameraActive(false);
  }

  async function startCamera() {
    try {
      stopCamera();
      setCameraError(null);

      const stream = await navigator.mediaDevices.getUserMedia({
        video: { facingMode: "user" },
        audio: false,
      });

      streamRef.current = stream;

      if (videoRef.current) {
        videoRef.current.srcObject = stream;
      }

      setIsCameraActive(true);
    } catch {
      setCameraError("Unable to access camera. Please allow camera permission.");
      setIsCameraActive(false);
    }
  }

  function clearCameraCapture() {
    setCameraCaptureBlob(null);
    if (cameraCaptureUrl) {
      URL.revokeObjectURL(cameraCaptureUrl);
    }
    setCameraCaptureUrl(null);
  }

  function captureFromCamera() {
    if (!videoRef.current || !canvasRef.current) {
      return;
    }

    const video = videoRef.current;
    const canvas = canvasRef.current;

    if (video.videoWidth === 0 || video.videoHeight === 0) {
      showToast({ tone: "error", message: "Camera is not ready yet." });
      return;
    }

    canvas.width = video.videoWidth;
    canvas.height = video.videoHeight;

    const ctx = canvas.getContext("2d");
    if (!ctx) {
      showToast({ tone: "error", message: "Unable to capture camera frame." });
      return;
    }

    ctx.drawImage(video, 0, 0, canvas.width, canvas.height);
    canvas.toBlob(
      (blob) => {
        if (!blob) {
          showToast({ tone: "error", message: "Failed to capture image." });
          return;
        }

        setCameraCaptureBlob(blob);
        if (cameraCaptureUrl) {
          URL.revokeObjectURL(cameraCaptureUrl);
        }
        setCameraCaptureUrl(URL.createObjectURL(blob));
        stopCamera();
        showToast({ tone: "info", message: "Live photo captured. You can verify now." });
      },
      "image/jpeg",
      0.9,
    );
  }

  async function uploadAvatarImage() {
    if (!avatarFile) {
      showToast({ tone: "error", message: "Please select an avatar image first." });
      return;
    }

    setIsSavingAvatar(true);
    try {
      const updatedProfile = await saveAvatarUpload(avatarFile);
      applyProfileState(updatedProfile);
      showToast({ tone: "success", message: "Avatar uploaded. Please verify your face." });
    } catch (error) {
      showToast({
        tone: "error",
        message:
          error instanceof Error ? error.message : "Unable to upload avatar.",
      });
    } finally {
      setIsSavingAvatar(false);
    }
  }

  async function verifyCapturedFace() {
    if (!cameraCaptureBlob) {
      showToast({ tone: "error", message: "Please capture a live photo first." });
      return;
    }

    setIsVerifyingFace(true);
    try {
      const file = new File([cameraCaptureBlob], "live-capture.jpg", {
        type: "image/jpeg",
      });
      const result = await verifyFaceUpload(file);
      const refreshedProfile = await getProfile();
      applyProfileState(refreshedProfile);

      showToast({
        tone: result.isVerified ? "success" : "error",
        message: result.message,
      });
    } catch (error) {
      showToast({
        tone: "error",
        message:
          error instanceof Error ? error.message : "Unable to verify face.",
      });
    } finally {
      setIsVerifyingFace(false);
    }
  }

  useEffect(() => {
    let isMounted = true;

    async function loadProfile() {
      try {
        const data = await getProfile();

        if (!isMounted) {
          return;
        }

        applyProfileState(data);
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
      stopCamera();
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
        region: region.trim() || nzLocations[0]?.region || "",
        suburb: suburb.trim() || "",
        bio: bio.trim() || undefined,
        gender: gender || undefined,
        age: age ? Number(age) : undefined,
        culture: culture || undefined,
        personality: buildPersonalityPayload(personality),
        interestSelections: parsedInterestSelections,
      };

      const updatedProfile = await updateProfile(payload);
      applyProfileState(updatedProfile);

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
      applyProfileState(data);
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
        <TopBar profile={profile} showBackLink={false} />
        <header className="mt-6 flex items-center justify-between gap-4 border-b border-zinc-200 pb-5">
          <div className="flex items-center gap-4">
            <Link className="text-lg font-semibold tracking-wide" href="/">
              RealLifeConnections
            </Link>
            <button
              type="button"
              className={profile?.isVerified
                ? "hidden sm:inline-flex items-center gap-2 rounded-full bg-emerald-600 px-4 py-2 text-sm font-semibold text-white shadow-lg transition hover:bg-emerald-700"
                : "hidden sm:inline-flex items-center gap-2 rounded-full bg-red-50 border border-red-300 px-4 py-2 text-sm font-semibold text-red-700 shadow-sm transition cursor-not-allowed opacity-80"
              }
              onClick={() => {
                if (profile?.isVerified) {
                  router.push("/meetups");
                } else {
                  showToast({ tone: "error", message: "You must verify your avatar before creating or matching events." });
                }
              }}
            >
              Manage / Match Event
            </button>
            <button
              type="button"
              className={profile?.isVerified
                ? "sm:hidden inline-flex items-center gap-2 rounded-full bg-emerald-600 px-3 py-1 text-sm font-semibold text-white shadow transition hover:bg-emerald-700"
                : "sm:hidden inline-flex items-center gap-2 rounded-full bg-red-50 border border-red-300 px-3 py-1 text-sm font-semibold text-red-700 shadow-sm transition cursor-not-allowed opacity-80"
              }
              onClick={() => {
                if (profile?.isVerified) {
                  router.push("/meetups/create");
                } else {
                  showToast({ tone: "error", message: "You must verify your avatar before creating or matching events." });
                }
              }}
            >
              Create
            </button>
          </div>
        </header>

        <Toast toast={toast} />

        <section className="grid flex-1 gap-8 py-8 lg:grid-cols-[0.9fr_1.1fr]">
          <div className="order-first lg:order-none">
            <div className="rounded-2xl border border-zinc-200 bg-zinc-50 p-4">
              <p className="text-sm font-semibold uppercase tracking-[0.18em] text-emerald-700">Avatar</p>
              <h2 className="mt-4 text-xl font-bold leading-tight text-zinc-950 sm:text-2xl">Verify your avatar.</h2>
              <p className="mt-3 text-base leading-7 text-zinc-650">
                Upload your real-person avatar and capture a live photo from camera to verify.
              </p>

              <div className="mt-4 grid gap-4 sm:grid-cols-2 lg:grid-cols-1">
                <div className="space-y-3">
                  <p className="text-sm font-semibold text-zinc-700">Avatar photo</p>
                  <div className="h-44 w-full overflow-hidden rounded-xl border border-zinc-200 bg-white">
                    {avatarPreviewUrl ? (
                      <img
                        src={avatarPreviewUrl}
                        alt="Avatar preview"
                        className="h-full w-full object-cover"
                      />
                    ) : (
                      <div className="flex h-full items-center justify-center text-sm text-zinc-500">
                        No avatar selected
                      </div>
                    )}
                  </div>
                    <input
                      id="avatar-file-input"
                      type="file"
                      accept="image/png,image/jpeg"
                      className="hidden"
                      onChange={(event) => {
                        const file = event.target.files?.[0] ?? null;
                        setAvatarFile(file);
                        if (!file) {
                          setAvatarPreviewUrl(profile?.avatarUrl ?? null);
                          return;
                        }

                        const nextPreviewUrl = URL.createObjectURL(file);
                        if (avatarPreviewUrl?.startsWith("blob:")) {
                          URL.revokeObjectURL(avatarPreviewUrl);
                        }
                        setAvatarPreviewUrl(nextPreviewUrl);
                      }}
                    />
                    <div className="grid grid-cols-2 gap-2">
                      {avatarFile ? (
                        <button
                          type="button"
                          className="h-11 rounded-md border-2 border-dashed border-red-300 bg-red-50 px-3 text-sm font-semibold text-red-700 transition hover:border-red-400 hover:bg-red-100"
                          onClick={() => {
                            setAvatarFile(null);
                            setAvatarPreviewUrl(profile?.avatarUrl ?? null);
                          }}
                        >
                          Unselect photo
                        </button>
                      ) : (
                        <label
                          htmlFor="avatar-file-input"
                          className="flex h-11 cursor-pointer items-center justify-center rounded-md border-2 border-dashed border-zinc-300 bg-zinc-50 px-3 text-sm font-semibold text-zinc-700 transition hover:border-emerald-400 hover:bg-emerald-50"
                        >
                          Choose photo
                        </label>
                      )}
                      <button
                        type="button"
                        className="h-11 rounded-md bg-emerald-700 px-4 text-sm font-semibold text-white transition hover:bg-emerald-800 disabled:cursor-not-allowed disabled:bg-zinc-400"
                        onClick={uploadAvatarImage}
                        disabled={!avatarFile || isSavingAvatar}
                      >
                        {isSavingAvatar ? "Uploading..." : "Upload"}
                      </button>
                    </div>
                  </div>

                <div className="space-y-3">
                  <p className="text-sm font-semibold text-zinc-700">Live camera photo</p>
                  <div className="h-44 w-full overflow-hidden rounded-xl border border-zinc-200 bg-black">
                    {cameraCaptureUrl ? (
                      <img
                        src={cameraCaptureUrl}
                        alt="Live capture preview"
                        className="h-full w-full object-cover"
                      />
                    ) : (
                      <video
                        ref={videoRef}
                        autoPlay
                        muted
                        playsInline
                        className="h-full w-full object-cover"
                      />
                    )}
                  </div>
                  <canvas ref={canvasRef} className="hidden" />

                  <div className="grid grid-cols-2 gap-2">
                    {isCameraActive ? (
                      <button
                        type="button"
                        className="h-11 rounded-md border border-red-300 bg-red-50 px-3 text-sm font-semibold text-red-700 transition hover:bg-red-100"
                        onClick={stopCamera}
                      >
                        Close camera
                      </button>
                    ) : (
                      <button
                        type="button"
                        className="h-11 rounded-md border border-zinc-400 bg-white px-3 text-sm font-semibold text-zinc-800 transition hover:bg-zinc-100"
                        onClick={startCamera}
                      >
                        Open camera
                      </button>
                    )}
                    {cameraCaptureUrl ? (
                      <button
                        type="button"
                        className="h-11 rounded-md border border-red-300 bg-red-50 px-3 text-sm font-semibold text-red-700 transition hover:bg-red-100"
                        onClick={clearCameraCapture}
                      >
                        Discard photo
                      </button>
                    ) : (
                      <button
                        type="button"
                        className="h-11 rounded-md border border-zinc-400 bg-white px-3 text-sm font-semibold text-zinc-800 transition hover:bg-zinc-100"
                        onClick={captureFromCamera}
                        disabled={!isCameraActive}
                      >
                        Capture photo
                      </button>
                    )}
                  </div>

                  <button
                    type="button"
                    className="h-11 w-full rounded-md bg-blue-700 px-4 text-sm font-semibold text-white transition hover:bg-blue-800 disabled:cursor-not-allowed disabled:bg-zinc-400"
                    onClick={verifyCapturedFace}
                    disabled={isVerifyingFace}
                  >
                    {isVerifyingFace ? "Verifying face..." : "Verify face"}
                  </button>

                  {cameraError ? (
                    <p className="text-xs font-medium text-red-600">{cameraError}</p>
                  ) : null}
                </div>
              </div>

              {profile ? (
                <div className="mt-6 rounded-xl border border-zinc-200 bg-white p-4">
                  <p className="text-sm font-semibold text-zinc-500">Current status</p>
                  <div className="mt-2 space-y-2 text-sm text-zinc-700">
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
                  <div>
                    <p className="text-sm font-semibold uppercase tracking-[0.18em] text-emerald-700">
                      Profile
                    </p>
                    <h1 className="mt-4 text-2xl font-bold leading-tight text-zinc-950 sm:text-3xl">
                      Edit your information.
                    </h1>
                    <p className="mt-3 text-base leading-7 text-zinc-650">
                      Update your region, suburb, bio, personality, username, and interests from one place.
                    </p>
                  </div>

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
                        Region
                      </span>
                      <select
                        className="mt-2 h-12 w-full rounded-md border border-zinc-300 bg-white px-3 text-base outline-none transition focus:border-emerald-600 focus:ring-4 focus:ring-emerald-100"
                        value={region}
                        onChange={(event) => {
                          const nextRegion = event.target.value;
                          const nextSuburb = nzLocations.find((location) => location.region === nextRegion)?.cities[0] ?? "";
                          setRegion(nextRegion);
                          setSuburb(nextSuburb);
                        }}
                        required
                      >
                        <option value="">Select region</option>
                        {nzLocations.map((location) => (
                          <option key={location.region} value={location.region}>
                            {location.region}
                          </option>
                        ))}
                      </select>
                    </label>

                    <label className="block">
                      <span className="text-sm font-semibold text-zinc-800">
                        Suburb
                      </span>
                      <select
                        className="mt-2 h-12 w-full rounded-md border border-zinc-300 bg-white px-3 text-base outline-none transition focus:border-emerald-600 focus:ring-4 focus:ring-emerald-100"
                        value={suburb}
                        onChange={(event) => setSuburb(event.target.value)}
                        required
                      >
                        <option value="">Select suburb</option>
                        {suburbsForRegion.map((suburbOption) => (
                          <option key={suburbOption} value={suburbOption}>
                            {suburbOption}
                          </option>
                        ))}
                      </select>
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

                    <label className="block">
                      <span className="text-sm font-semibold text-zinc-800">
                        Culture
                      </span>
                      <select
                        className="mt-2 h-12 w-full rounded-md border border-zinc-300 bg-white px-3 text-base outline-none transition focus:border-emerald-600 focus:ring-4 focus:ring-emerald-100"
                        value={culture}
                        onChange={(event) => setCulture(event.target.value as CultureOption | "")}
                      >
                        <option value="">Select (optional)</option>
                        {cultures.map((option) => (
                          <option key={option} value={option}>
                            {option}
                          </option>
                        ))}
                      </select>
                    </label>
                  </div>

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

                  <section className="rounded-md border border-zinc-200 p-4">
                    <div className="mb-4">
                      <h3 className="text-sm font-semibold text-zinc-800">Personality</h3>
                      <p className="mt-1 text-sm text-zinc-600">
                        Fine-tune how you want to show up in matching.
                      </p>
                    </div>

                    <div className="grid gap-5 lg:grid-cols-2">
                      <div className="space-y-4">
                        {personalityTraits.map((trait) => (
                          <label key={trait.key} className="block">
                            <span className="text-sm font-semibold text-zinc-800">{trait.label}</span>
                            <input
                              className="mt-2 w-full accent-emerald-700"
                              type="range"
                              min={0}
                              max={100}
                              value={personality[trait.key]}
                              onChange={(event) => {
                                setPersonality((current) => ({
                                  ...current,
                                  [trait.key]: Number(event.target.value),
                                }));
                              }}
                            />
                            <div className="mt-1 grid grid-cols-[4.5rem_1fr_4.5rem] items-center text-xs text-zinc-500">
                              <span className="truncate">{trait.leftLabel}</span>
                              <span className="text-center tabular-nums font-medium text-zinc-600">
                                {personality[trait.key]}
                              </span>
                              <span className="truncate text-right">{trait.rightLabel}</span>
                            </div>
                          </label>
                        ))}
                      </div>

                      <div className="space-y-4">
                        <label className="block">
                          <span className="text-sm font-semibold text-zinc-800">Preferred days</span>
                          <select
                            className="mt-2 h-12 w-full rounded-md border border-zinc-300 bg-white px-3 text-base outline-none transition focus:border-emerald-600 focus:ring-4 focus:ring-emerald-100"
                            value={personality.preferredDaysOfWeek}
                            onChange={(event) =>
                              setPersonality((current) => ({
                                ...current,
                                preferredDaysOfWeek: event.target.value,
                              }))
                            }
                          >
                            <option value="">Select (optional)</option>
                            {preferredDaysOptions.map((option) => (
                              <option key={option} value={option}>
                                {option}
                              </option>
                            ))}
                          </select>
                        </label>

                        <label className="block">
                          <span className="text-sm font-semibold text-zinc-800">Preferred time</span>
                          <select
                            className="mt-2 h-12 w-full rounded-md border border-zinc-300 bg-white px-3 text-base outline-none transition focus:border-emerald-600 focus:ring-4 focus:ring-emerald-100"
                            value={personality.preferredTimeOfDay}
                            onChange={(event) =>
                              setPersonality((current) => ({
                                ...current,
                                preferredTimeOfDay: event.target.value,
                              }))
                            }
                          >
                            <option value="">Select (optional)</option>
                            {preferredTimeOptions.map((option) => (
                              <option key={option} value={option}>
                                {option}
                              </option>
                            ))}
                          </select>
                        </label>

                        <label className="block">
                          <span className="text-sm font-semibold text-zinc-800">Maximum activity range (km)</span>
                          <input
                            className="mt-2 h-12 w-full rounded-md border border-zinc-300 px-3 text-base outline-none transition focus:border-emerald-600 focus:ring-4 focus:ring-emerald-100"
                            type="number"
                            min={1}
                            max={500}
                            value={personality.preferredDistanceKm}
                            onChange={(event) =>
                              setPersonality((current) => ({
                                ...current,
                                preferredDistanceKm: event.target.value,
                              }))
                            }
                            placeholder="e.g. 20"
                          />
                        </label>
                      </div>
                    </div>
                  </section>

                  <section className="rounded-md border border-zinc-200 p-4">
                    <div className="mb-4">
                      <h3 className="text-sm font-semibold text-zinc-800">Interests</h3>
                      <p className="mt-1 text-sm text-zinc-600">
                        Add the interests you want to bring into matching.
                      </p>
                    </div>

                    <div className="space-y-4">
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

                    <div className="mt-4 flex flex-wrap gap-3">
                      <button
                        type="button"
                        className="flex h-11 items-center justify-center gap-2 rounded-md bg-emerald-700 px-4 text-sm font-semibold text-white shadow-sm shadow-emerald-950/10 transition duration-200 ease-out hover:-translate-y-0.5 hover:bg-emerald-800 hover:shadow-lg hover:shadow-emerald-950/15 active:translate-y-0 disabled:translate-y-0 disabled:cursor-not-allowed disabled:bg-zinc-400"
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
                        className="flex h-11 items-center justify-center gap-2 rounded-md bg-red-600 px-4 text-sm font-semibold text-white shadow-sm shadow-red-950/10 transition duration-200 ease-out hover:-translate-y-0.5 hover:bg-red-700 hover:shadow-lg hover:shadow-red-950/15 active:translate-y-0 disabled:translate-y-0 disabled:cursor-not-allowed disabled:bg-zinc-400"
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
                  </section>

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