# RobRequest

A lightweight, modern REST API development client built with .NET 10 Blazor Server, SQLite, and MudBlazor. A streamlined alternative to Postman that runs entirely in the browser.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

## Getting Started

```bash
cd RobRequest
dotnet restore
dotnet run
```

Then open `https://localhost:7025` in your browser.

## Project Structure

- **RobRequest.Server/** — Blazor Server project
  - `Components/` - Routable pages and reusable UI components
  - `wwwroot/` - Static files (favicon, images, etc.)
- **RobRequest.Shared/** — shared library models and services
  - `Models/` — Data models (HttpRequestModel, HttpResponseModel, etc.)
  - `Services/` — Business logic (ApiService, HistoryService, EnvironmentService, SettingsService)
  - `Migrations/` — Entity Framework Core migrations
  - `Data/` - Application database context
- **RobRequest.Tests/** — unit tests

## Features

- **Request Builder** — All HTTP methods, query params, custom headers, JSON/XML/text body, auth (Bearer, Basic, API Key)
- **Response Viewer** — Color-coded status, formatted JSON, sortable headers, copy/search
- **History** — Automatic request logging with search/filter
- **Environments** — Variable substitution with `{{variable}}` syntax
- **Dark/Light Theme** — Toggle with system preference support
- **Settings** — Configurable timeout, history limits, environment management

## Tech Stack

- .NET 10.0 Blazor Server
- MudBlazor 9.x (Material Design UI)
- SQLite (server-side database)

&nbsp;
