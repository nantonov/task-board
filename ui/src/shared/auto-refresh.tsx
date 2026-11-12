"use client";

import { useRouter } from "next/navigation";
import { useEffect } from "react";

// A timer, not a data load: router.refresh() re-runs the server component, which owns the fetch.
// Browsers throttle timers in background tabs, so the tab also refreshes the moment it is shown.
export function AutoRefresh({ intervalMs }: { intervalMs: number }) {
  const router = useRouter();

  useEffect(() => {
    const refreshWhenVisible = () => {
      if (document.visibilityState === "visible") {
        router.refresh();
      }
    };

    const timer = setInterval(() => router.refresh(), intervalMs);
    document.addEventListener("visibilitychange", refreshWhenVisible);
    return () => {
      clearInterval(timer);
      document.removeEventListener("visibilitychange", refreshWhenVisible);
    };
  }, [router, intervalMs]);

  return null;
}
