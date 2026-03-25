# TornCompany

## Overview
Windows Forms (.NET 9.0) application that displays TORN companies with job openings across all company types.

## Build & Run
```bash
cd TornCompany
dotnet build
dotnet run
```

## Project Conventions
- **API Version**: Only use Torn API v2 (never v1)
- **Authentication**: `?key=YOUR_API_KEY` query parameter
- **Base URL**: `https://api.torn.com/v2/torn/companies`
- **JSON**: Use `System.Text.Json` with `[JsonPropertyName]` attributes for snake_case mapping
- **Classes**: Use `sealed` on classes not intended for inheritance
- **Async**: Use async/await with CancellationToken for all API calls
- **UI**: Keep responsive — disable controls during loading, show progress bar and status messages
- **Sorting**: Click column headers to toggle ascending/descending sort
- **No external NuGet packages** — use only .NET framework libraries

## Architecture
- `Models/CompanyModels.cs` — DTOs for API responses + company type lookup
- `Services/TornApiService.cs` — HTTP service for TORN API calls
- `Form1.cs` — Main form logic (fetch, sort, display)
- `Form1.Designer.cs` — UI layout and control initialization
