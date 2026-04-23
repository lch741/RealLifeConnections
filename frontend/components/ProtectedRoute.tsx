"use client";

import { useEffect, useState, type ReactNode } from "react";
import { useRouter } from "next/navigation";

type ProtectedRouteProps = {
  children: ReactNode;
  redirectTo?: string;
};

export default function ProtectedRoute({
  children,
  redirectTo = "/login",
}: Readonly<ProtectedRouteProps>) {
  const router = useRouter();
  const [ready, setReady] = useState(false);
  const [allowed, setAllowed] = useState(false);

  useEffect(() => {
    const token = localStorage.getItem("authToken");

    if (!token) {
      router.replace(redirectTo);
      return;
    }

    setAllowed(true);
    setReady(true);
  }, [router, redirectTo]);

  if (!ready || !allowed) {
    return (
      <main className="min-h-screen bg-[#f8faf7]">
        <div className="mx-auto flex min-h-screen w-full max-w-5xl items-center justify-center px-5 py-6 sm:px-8 lg:px-10">
          <p className="text-sm text-zinc-600">Checking session...</p>
        </div>
      </main>
    );
  }

  return <>{children}</>;
}