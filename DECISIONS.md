# Decisions

## Kit versions

| Value | Recorded | Read from |
|---|---|---|
| Product version | Outstaff Onboarding Tool 2.1.0+ccf2b4a | launcher banner; `payload/composition.json` `generator` |
| Kit revision | ccf2b4a5d82ef7eeb701883c82020c37a32255dd | `payload/composition.json` `kitCommit` |
| Crew kit revision | a4b8aea (dotnet-crew 0.3.0) | launcher step "Choose crews" |

The install at `D:\Innowise\OutstaffKit` has no `build-info.txt` next to the launcher, so the
three values come from the sources above; they match each other.

## Board

- **Mono repo, three parts** (backend, mcp-server, ui), as recorded at onboarding
  (STACK_LAYOUT=mono).
- **.NET 10 (LTS)**: .NET 8 support ended 2026-11-10; .NET 11 is a day-old STS release.
- **The MCP server is a REST client of the backend**, not a second writer to SQLite. One
  process owns the file; agent and UI see the same state. Rejected: MCP server opening the
  DB directly (two writers, duplicated validation).
- **SQLite through Dapper, schema created on start.** One table; EF Core and a migrations
  framework would be most of the code. Rejected: EF Core.
- **No auth on the API, loopback binding only, anonymous declared explicitly.** A
  single-user local tool; who may write is enforced at the agent's side (MCP is its only
  channel). CORS is a named, GET-only policy for the UI origin.
- **No pagination on `GET /api/tasks`** (rulebook DN-33 deviation): a personal board holds
  tens of cards and the UI needs all of them for three columns.
- **UI polling via `router.refresh()` every 3 s from a server component.** The browser never
  calls the backend and the backend URL stays server-side (TS-51/52/55). Rejected: client
  fetch with a `NEXT_PUBLIC_` URL (blocked by those rules); TanStack Query (a dependency for
  one GET); SSE/WebSocket (polling is explicitly allowed and simpler).
- **Loud failures.** Backend down: the MCP tool returns an error that says the card was not
  updated; the UI shows a banner over the last good snapshot instead of an empty board.
- **postcss pinned to 8.5.32 via `overrides`**: Next 15.5.29 bundles a postcss with two
  high advisories. Rejected: Next 16, which needs Node >= 20.9 and would mean replacing the
  machine's Node.
- **Next 15, not 16**, for the same Node reason.
- **Tests: xUnit v3 on Microsoft.Testing.Platform**, one command (`dotnet test`).
- **The MCP server checks the key shape itself** before the key goes into a URL path, on
  top of the backend's validation: the key comes from the agent and is part of an internal
  URL. Title and description rules stay in the backend only.
- **Malformed keys are never echoed back** in a 404/409 `detail` ("No card with that
  key."); well-formed keys are.
- **A product `.gitignore`** (build outputs, `node_modules`, `*.db`) lives in this repo. It
  has no entries for any personal AI tooling, and no such files live here.
- **UI layout**: `features/board/` (api, types, components, public `index.ts`),
  `shared/` for the refresh timer, thin `app/` routes with `loading` and `error` states.
