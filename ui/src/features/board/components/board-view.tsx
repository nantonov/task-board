"use client";

import { useState } from "react";
import { type BoardResult, CARD_STATUSES, type CardStatus, type TaskCard } from "../types";
import { BoardColumn } from "./board-column";

// A Record over CardStatus: adding a status without a column title fails to compile.
const COLUMN_TITLES: Record<CardStatus, string> = {
  ToDo: "To Do",
  InProgress: "In Progress",
  Done: "Done",
};

// Keeps the last good snapshot on screen while the backend is down, so an outage shows up
// as a banner over stale cards instead of an empty board that looks like "nothing to do".
export function BoardView({ board }: { board: BoardResult }) {
  const [lastGood, setLastGood] = useState<TaskCard[]>(board.ok ? board.cards : []);
  if (board.ok && board.cards !== lastGood) {
    setLastGood(board.cards);
  }

  const cards = board.ok ? board.cards : lastGood;

  return (
    <>
      {!board.ok && (
        <p role="alert" className="banner">
          {board.error} Showing the last known state; it will refresh when the backend is back.
        </p>
      )}
      <div className="board">
        {CARD_STATUSES.map((status) => (
          <BoardColumn
            key={status}
            title={COLUMN_TITLES[status]}
            cards={cards.filter((card) => card.status === status)}
          />
        ))}
      </div>
    </>
  );
}
