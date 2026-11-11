import type { TaskCard as TaskCardData } from "../types";

export function TaskCard({ card }: { card: TaskCardData }) {
  return (
    <article className="card" aria-label={card.key}>
      <header>
        <span className="card-key">{card.key}</span>
        <h3>{card.title}</h3>
      </header>
      {card.description && <p className="card-description">{card.description}</p>}
    </article>
  );
}
