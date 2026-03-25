# Torn Company Viewer

A Windows Forms (.NET 9.0) desktop application for the online game [TORN](https://www.torn.com) that helps players find companies with job openings.

## Features

- Browse companies across all 39 company types or filter by a specific type
- Filter by company rating (3-10)
- Displays only companies with available job openings, sorted by number of openings (descending)
- Shows director name and last online status for each company
- Automatically filters out inactive companies (director offline 3+ days or zero employees)
- Click column headers to sort by Openings, Rating, Daily Income, Daily Customers, Days Old, Hired, or Capacity
- Double-click a row to open the company page on torn.com in your browser
- API key saved locally in settings.json for convenience

## Requirements

- Windows 10 or later
- .NET 9.0 SDK

## Build & Run

```bash
cd TornCompany
dotnet build
dotnet run
```

## Usage

1. Enter your TORN API key (saved automatically for future sessions)
2. Select a company type from the dropdown (or leave as the first option to fetch all types)
3. Select a rating level (3-10)
4. Click **Fetch Companies**
5. Double-click any row to view the company on torn.com

## API

Uses the TORN API with the `company/{typeId}?selections=companies` endpoint and `user/{userId}?selections=profile` for director information. Requires a valid TORN API key.
