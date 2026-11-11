import { BoardView, fetchBoard } from "@/features/board";
import { AutoRefresh } from "@/shared/auto-refresh";

export const dynamic = "force-dynamic";

const REFRESH_INTERVAL_MS = 3000;

export default async function Page() {
  const board = await fetchBoard();

  return (
    <main>
      <h1>Task board</h1>
      <BoardView board={board} />
      <AutoRefresh intervalMs={REFRESH_INTERVAL_MS} />
    </main>
  );
}
