import { type BoardResult, isTaskCard } from "../types";

const REQUEST_TIMEOUT_MS = 2500;

// Runs on the Next.js server only, so the backend address never reaches the browser bundle.
export async function fetchBoard(): Promise<BoardResult> {
  const baseUrl = process.env.BOARD_API_URL ?? "http://127.0.0.1:5080";

  let response: Response;
  try {
    response = await fetch(`${baseUrl}/api/tasks`, {
      cache: "no-store",
      signal: AbortSignal.timeout(REQUEST_TIMEOUT_MS),
    });
  } catch (error: unknown) {
    const reason = error instanceof Error && error.name === "TimeoutError" ? "timed out" : "unreachable";
    return { ok: false, error: `Board backend ${reason}.` };
  }

  if (!response.ok) {
    return { ok: false, error: `Board backend answered ${response.status}.` };
  }

  const body: unknown = await response.json().catch(() => null);
  if (!Array.isArray(body) || !body.every(isTaskCard)) {
    return { ok: false, error: "Board backend returned an unexpected payload." };
  }

  return { ok: true, cards: body };
}
