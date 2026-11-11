# task-board

A personal task board (To Do / In Progress / Done) that mirrors what the Outstaff kit is
doing with a ticket. Only the agent changes it, and only through the MCP server in this
repo; the web UI is read-only.

```
backend/TaskBoard.Api     ASP.NET Core minimal API, SQLite (Dapper)   http://127.0.0.1:5080
mcp-server/TaskBoard.Mcp  MCP server (stdio) -> backend REST           started by Claude Code
ui/                       Next.js 15 + TypeScript, polls every 3 s     http://localhost:3000
tests/                    xUnit v3: API (in-process host) + MCP tools
```

## Prerequisites

- .NET SDK 10.0.404 or a later 10.0 feature band (pinned in `global.json`)
- Node.js 20.3 or later, npm

## Run on a clean clone

Three terminals, from the repo root:

```bash
dotnet run --project backend/TaskBoard.Api --launch-profile http
```

```bash
cd ui && npm ci && npm run build && npm run start
```

Then open http://localhost:3000. The SQLite file is created on first start at
`backend/TaskBoard.Api/data/board.db` (git-ignored). `npm run dev` works too for UI work.

The MCP server is not started by hand: the agent's host launches it over stdio. Build it
once so the host can start the compiled binary quickly:

```bash
dotnet build mcp-server/TaskBoard.Mcp
```

Register it with Claude Code from the folder the sessions start in (for the kit, the
workspace root, never inside this repo), e.g. in that folder's `.mcp.json`:

```json
{
  "mcpServers": {
    "task-board": {
      "command": "dotnet",
      "args": ["<path-to-this-repo>/mcp-server/TaskBoard.Mcp/bin/Debug/net10.0/TaskBoard.Mcp.dll"]
    }
  }
}
```

Tools: `create_task(key, title, description)`, `move_task(key, status)` with status
`ToDo | InProgress | Done`, `list_tasks()`. A failed write (backend down, unknown key,
duplicate key) comes back as an MCP tool error that says what did not happen.

## Tests

```bash
dotnet test
```

Runs both test projects (Microsoft.Testing.Platform, opted in via `global.json`).

## Configuration

| Setting | Where | Default |
|---|---|---|
| API listen URL | `backend/TaskBoard.Api/appsettings.json` `Urls` | `http://127.0.0.1:5080` |
| SQLite path | `Board:DatabasePath` (relative to the API project) | `data/board.db` |
| UI origin allowed by CORS (GET only) | `Board:UiOrigin` | `http://localhost:3000` |
| Backend URL seen by the MCP server | env `BOARD_API_URL` | `http://127.0.0.1:5080/` |
| Backend URL seen by the UI server | env `BOARD_API_URL` | `http://127.0.0.1:5080` |

## API

See `DECISIONS.md` for why it looks the way it does.

| Method | Route | Body | Success | Errors (ProblemDetails) |
|---|---|---|---|---|
| GET | /api/tasks | | 200 `TaskCard[]` oldest first | |
| GET | /api/tasks/{key} | | 200 `TaskCard` | 404 |
| POST | /api/tasks | `{key, title, description?}` | 201 `TaskCard` (status `ToDo`) | 400, 409 |
| PATCH | /api/tasks/{key} | `{status}` | 200 `TaskCard` | 400, 404 |
