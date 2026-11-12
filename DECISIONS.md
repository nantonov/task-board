# Decisions

**Kit versions.** Product: Outstaff Onboarding Tool 2.1.0+ccf2b4a. Kit revision:
ccf2b4a5d82ef7eeb701883c82020c37a32255dd. Crew kit revision: a4b8aea (dotnet-crew 0.3.0).
The install at `D:\Innowise\OutstaffKit` has no `build-info.txt` next to the launcher. The
values come from the launcher banner, `payload/composition.json` (`generator`,
`kitCommit`) and the launcher's crew step, and they agree with each other.

## How the board works with the kit
The human writes "Заведи задачу: <text>". The workspace skill `board-sync` picks the next
`BRD-<n>` and saves the text to `tasks/<KEY>/ticket.md`. It calls `create_task`, so the card
lands in To Do, and starts the kit's `feature` route on that key in the same turn. With no
tracker, pre-flight takes the pasted text as the ticket. After both gates, at Phase 5 entry,
the card moves to In Progress. When the human says "задача сделана", after the route has
handed over (Phase 8), it moves to Done. Each move writes a `Board:` line in
`tasks/<KEY>/STATE.md`, and that file is the truth. A failed MCP call is a loud tool error
("the card was NOT updated"), and the line is then recorded as `PENDING`. The kit keeps
working, and the board catches up from those lines through MCP later.
**Rejected:** registering the board as the kit's tracker (`TRACKER_MCP_PREFIX`): tracker
writes are drafts-only, the installed skills are rendered for `none`, and the board is not
the client's tracker. A hook that writes the board: hooks cannot call MCP, so it would need
curl, which is exactly the channel the agent must not have.

## Workspace configuration changes (personal layer only)
| Change | Why |
|---|---|
| `.mcp.json` at the workspace root registers `task-board` (published dll in `tools/task-board-mcp/`) | a root session sees it; nothing global. Publishing keeps it off `bin/Debug`, which a running server would lock during builds |
| `enabledMcpjsonServers: ["task-board"]` in `.claude/settings.local.json` | no approval dialog before the demo |
| `list_tasks` → ALLOW; `create_task`, `move_task` → ASK | reads skip the prompt. Every board change is one prompt the human answers. The doctor fails write tools in ALLOW, and DENY would stop the agent driving the board |
| DENY: curl/wget/Invoke-WebRequest/Invoke-RestMethod (+aliases) on port 5080, `sqlite3`, Read/Edit/Write `board.db*` | MCP is the agent's only channel. The wildcards are a safety net; the CLAUDE.md rule and the skill are the main control |
| `board-sync` skill with evals, one route line in `CLAUDE.md` | written the kit's writing-skills way: baseline without the skill first |
Nothing the kit or the crew installed was edited. `kit-diff` reports local changes only in
`CLAUDE.md` and `settings.local.json`, the files the task names as the candidate's. The
workspace `.gitignore` is kit-installed, so it stays as installed and the personal git does
not track `.mcp.json`. The guard registration and the onboarder's deny entries are unchanged.

## Board internals
- **Mono repo**, .NET 10 LTS. .NET 8 left support on 2026-11-10, and .NET 11 is a day-old
  STS release.
- **The MCP server is a REST client of the backend.** There is one writer to SQLite.
  Rejected: the MCP server opening the DB directly.
- **SQLite through Dapper**, with the schema created on start. Rejected: EF Core, too much
  machinery for one table.
- **The API is anonymous, declared explicitly, and bound to loopback only.** CORS allows GET
  from the UI origin only. Writes are controlled on the agent side.
- **No pagination** on `GET /api/tasks` (DN-33). A personal board holds tens of cards.
- **The UI is a server component plus `router.refresh()`** every 3 s and on tab focus. The
  backend URL stays on the server (TS-51/52/55). Rejected: a client fetch with a public URL,
  TanStack Query, SSE.
- **Failures are loud.** The tool error says what did not happen. The UI shows a banner over
  the last good snapshot.
- **The MCP server checks the key shape** before putting it in a URL. Malformed keys are never
  echoed back in a `detail`.
- **Next 15 with postcss overridden to 8.5.32**, to clear two high advisories. Next 16 would
  need a newer Node than this machine has.
- **A product `.gitignore`** with build outputs only. Nothing of the personal AI layer is in
  this repo.
- **Tests:** xUnit v3 on Microsoft.Testing.Platform, run with `dotnet test`.
