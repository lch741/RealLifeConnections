import Link from "next/link";
import type { ReactNode } from "react";

type AuthShellProps = {
  title: string;
  subtitle: string;
  switchLabel: string;
  switchHref: string;
  switchText: string;
  children: ReactNode;
};

export default function AuthShell({
  title,
  subtitle,
  switchLabel,
  switchHref,
  switchText,
  children,
}: Readonly<AuthShellProps>) {
  return (
    <main className="min-h-screen bg-[#f8faf7] text-zinc-950">
      <div className="mx-auto flex min-h-screen w-full max-w-6xl flex-col px-5 py-6 sm:px-8 lg:px-10">
        <nav className="flex items-center justify-between">
          <Link className="text-lg font-semibold tracking-wide" href="/">
            RealLifeConnections
          </Link>
          <Link
            className="rounded-md border border-zinc-300 bg-white px-4 py-2 text-sm font-semibold text-zinc-900 shadow-sm transition hover:border-zinc-500"
            href={switchHref}
          >
            {switchText}
          </Link>
        </nav>

        <section className="grid flex-1 items-center gap-10 py-10 lg:grid-cols-[0.9fr_1.1fr]">
          <div className="max-w-xl">
            <p className="text-sm font-semibold uppercase tracking-[0.18em] text-emerald-700">
              {switchLabel}
            </p>
            <h1 className="mt-4 text-4xl font-bold leading-tight text-zinc-950 sm:text-5xl">
              {title}
            </h1>
            <p className="mt-5 text-lg leading-8 text-zinc-650">{subtitle}</p>
          </div>

          <div className="w-full">{children}</div>
        </section>
      </div>
    </main>
  );
}
