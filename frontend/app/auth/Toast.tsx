"use client";

export type ToastState = {
  message: string;
  tone: "success" | "error" | "info";
};

type ToastProps = {
  toast: ToastState | null;
};

const toneStyles = {
  success: "border-emerald-200 bg-emerald-50 text-emerald-950",
  error: "border-rose-200 bg-rose-50 text-rose-950",
  info: "border-sky-200 bg-sky-50 text-sky-950",
};

const toneDots = {
  success: "bg-emerald-500",
  error: "bg-rose-500",
  info: "bg-sky-500",
};

export default function Toast({ toast }: ToastProps) {
  if (!toast) {
    return null;
  }

  return (
    <div className="fixed right-4 top-4 z-50 w-[calc(100%-2rem)] max-w-sm">
      <div
        className={`flex min-h-14 items-start gap-3 rounded-lg border px-4 py-3 shadow-lg shadow-black/10 ${toneStyles[toast.tone]}`}
        role="status"
        aria-live="polite"
      >
        <span
          className={`mt-1 h-2.5 w-2.5 shrink-0 rounded-full ${toneDots[toast.tone]}`}
          aria-hidden="true"
        />
        <p className="text-sm font-medium leading-6">{toast.message}</p>
      </div>
    </div>
  );
}
