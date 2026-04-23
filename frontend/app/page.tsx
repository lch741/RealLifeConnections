"use client";

import { useEffect } from "react";
import { useRouter } from "next/navigation";

export default function Home() {
  const router = useRouter();

  useEffect(() => {
    const token = localStorage.getItem("authToken");
    router.replace(token ? "/profile" : "/login");
  }, [router]);

  return (
    <main className="min-h-screen bg-[#f8faf7]">
      <div className="mx-auto flex min-h-screen w-full max-w-5xl items-center justify-center px-5 py-6 sm:px-8 lg:px-10">
        <p className="text-sm text-zinc-600">Redirecting...</p>
      </div>
    </main>
  );
}
