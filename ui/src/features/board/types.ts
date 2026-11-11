export const CARD_STATUSES = ["ToDo", "InProgress", "Done"] as const;

export type CardStatus = (typeof CARD_STATUSES)[number];

export interface TaskCard {
  key: string;
  title: string;
  description: string;
  status: CardStatus;
  createdAt: string;
  updatedAt: string;
}

export type BoardResult =
  | { ok: true; cards: TaskCard[] }
  | { ok: false; error: string };

export function isTaskCard(value: unknown): value is TaskCard {
  if (typeof value !== "object" || value === null) {
    return false;
  }

  const card = value as Record<string, unknown>;
  return (
    typeof card.key === "string" &&
    typeof card.title === "string" &&
    typeof card.description === "string" &&
    typeof card.createdAt === "string" &&
    typeof card.updatedAt === "string" &&
    CARD_STATUSES.some((status) => status === card.status)
  );
}
