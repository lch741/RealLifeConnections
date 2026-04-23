type ErrorPayload = {
  message?: unknown;
  title?: unknown;
  errors?: unknown;
};

type HandleApiErrorOptions = {
  onUnauthorized?: () => void;
  redirectOnUnauthorized?: boolean;
  loginPath?: string;
};

function appendIfNonEmpty(value: unknown, messages: string[]) {
  if (typeof value === "string" && value.trim()) {
    messages.push(value);
  }
}

function extractArrayErrors(items: unknown[], messages: string[]) {
  for (const item of items) {
    if (typeof item === "string") {
      appendIfNonEmpty(item, messages);
      continue;
    }

    if (
      item &&
      typeof item === "object" &&
      "description" in item &&
      typeof (item as { description?: unknown }).description === "string"
    ) {
      appendIfNonEmpty((item as { description: string }).description, messages);
    }
  }
}

function extractObjectErrors(errors: Record<string, unknown>, messages: string[]) {
  for (const value of Object.values(errors)) {
    if (Array.isArray(value)) {
      extractArrayErrors(value, messages);
      continue;
    }

    appendIfNonEmpty(value, messages);
  }
}

function extractMessages(data: unknown): string[] {
  const messages: string[] = [];

  if (typeof data === "string") {
    appendIfNonEmpty(data, messages);
    return messages;
  }

  if (!data || typeof data !== "object") {
    return [];
  }

  const payload = data as ErrorPayload;
  appendIfNonEmpty(payload.message, messages);
  appendIfNonEmpty(payload.title, messages);

  if (Array.isArray(payload.errors)) {
    extractArrayErrors(payload.errors, messages);
  } else if (payload.errors && typeof payload.errors === "object") {
    extractObjectErrors(payload.errors as Record<string, unknown>, messages);
  }

  return [...new Set(messages)];
}

export function handleApiError(
  status: number,
  data: unknown,
  options?: HandleApiErrorOptions,
): never {
  if (status === 401) {
    options?.onUnauthorized?.();

    if (options?.redirectOnUnauthorized && globalThis.location.pathname !== "/login") {
      globalThis.location.replace(options.loginPath ?? "/login");
    }

    throw new Error("Please login.");
  }

  const messages = extractMessages(data);
  throw new Error(messages[0] ?? "Request failed.");
}
