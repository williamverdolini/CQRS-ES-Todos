CQRS+ES Todo List
=============


[![GitHub license](https://img.shields.io/github/license/mashape/apistatus.svg)](https://github.com/williamverdolini/CQRS-ES-Todos/blob/master/LICENSE)

<a href="http://williamverdolini.github.io/Cqrs-es-todos.html" target="_blank">A training journey into CQRS+ES</a>

## Modern application

The maintained application is the .NET 10 and Angular implementation in [`modern/`](modern/). The former .NET Framework web application and command/query stacks have been retired. The legacy event exporter and its supporting projects remain available for creating import bundles.

### Prerequisites

- Docker Desktop (Windows/macOS) or Docker Engine (Linux), with Docker Compose v2.
- Network access to container, NuGet, and npm registries for the initial image build or package restore.
- Ports `4200`, `8080`, and `1433` available on localhost.
- .NET 10 SDK to build or test the backend outside Docker.
- Node.js 24 and npm 11 to build or test the Angular frontend outside Docker. These are not needed when running the full stack with Docker Compose.

### Build and run the full stack

From the repository root, run:

```powershell
Set-Location modern
docker compose --env-file .env.example up --build
```

Compose builds and starts SQL Server, the .NET API, and the Angular frontend served by Nginx. The example SQL password is for local development only.

- UI: <http://localhost:4200>
- API health: <http://localhost:8080/health/ready>
- OpenAPI: <http://localhost:8080/openapi/v1.json>

Press `Ctrl+C` to stop the foreground stack, or run `docker compose down` from `modern/` to remove its containers. This keeps the SQL data volume; do not add `-v` if you want to retain that data.

### Build and test from the command line

With the .NET 10 SDK installed, from the repository root:

```powershell
dotnet build modern/CqrsEsTodo.Modern.sln
dotnet test modern/CqrsEsTodo.Modern.sln
```

To build and test the Angular frontend with Node.js 24 and npm 11:

```powershell
Set-Location modern/web/cqrs-es-todos-ui
npm install
npm run build
npm test
```

See [`modern/README.md`](modern/README.md) for API routes, architecture notes, and the retained legacy event import procedure.
