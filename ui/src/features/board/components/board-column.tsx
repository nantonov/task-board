import type { TaskCard as TaskCardData } from "../types";
import { TaskCard } from "./task-card";

export function BoardColumn({ title, cards }: { title: string; cards: TaskCardData[] }) {
  return (
    <section className="column" aria-label={title}>
      <h2>
        {title} <span className="count">{cards.length}</span>
      </h2>
      {cards.map((card) => (
        <TaskCard key={card.key} card={card} />
      ))}
    </section>
  );
}
