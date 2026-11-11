"use client";

export default function BoardError({ reset }: { error: Error; reset: () => void }) {
  return (
    <main>
      <h1>Task board</h1>
      <p role="alert" className="banner">
        The board page failed to render.
      </p>
      <button type="button" onClick={reset}>
        Try again
      </button>
    </main>
  );
}
