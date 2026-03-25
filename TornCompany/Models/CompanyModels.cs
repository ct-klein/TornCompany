using System.Text.Json.Serialization;

namespace TornCompany.Models;

public sealed class CompanyListResponse
{
    [JsonPropertyName("company")]
    public Dictionary<string, Company>? Company { get; set; }

    [JsonPropertyName("company_timestamp")]
    public long CompanyTimestamp { get; set; }
}

public sealed class Company
{
    [JsonPropertyName("ID")]
    public int Id { get; set; }

    [JsonPropertyName("company_type")]
    public int CompanyType { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("director")]
    public int Director { get; set; }

    [JsonPropertyName("rating")]
    public int Rating { get; set; }

    [JsonPropertyName("days_old")]
    public int DaysOld { get; set; }

    [JsonPropertyName("daily_income")]
    public long DailyIncome { get; set; }

    [JsonPropertyName("weekly_income")]
    public long WeeklyIncome { get; set; }

    [JsonPropertyName("daily_customers")]
    public int DailyCustomers { get; set; }

    [JsonPropertyName("weekly_customers")]
    public int WeeklyCustomers { get; set; }

    [JsonPropertyName("employees_hired")]
    public int EmployeesHired { get; set; }

    [JsonPropertyName("employees_capacity")]
    public int EmployeesCapacity { get; set; }

    public int Openings => EmployeesCapacity - EmployeesHired;

    // Populated after fetching director profile
    public string DirectorName { get; set; } = string.Empty;
    public string DirectorLastAction { get; set; } = string.Empty;
    public long DirectorLastActionTimestamp { get; set; }
}

public sealed class UserProfileResponse
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("last_action")]
    public LastAction? LastAction { get; set; }
}

public sealed class LastAction
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("timestamp")]
    public long Timestamp { get; set; }

    [JsonPropertyName("relative")]
    public string Relative { get; set; } = string.Empty;
}

public static class CompanyTypes
{
    public static readonly Dictionary<int, string> Names = new()
    {
        [1] = "Hair Salon",
        [2] = "Law Firm",
        [3] = "Flower Shop",
        [4] = "Car Dealership",
        [5] = "Clothing Store",
        [6] = "Gun Shop",
        [7] = "Game Shop",
        [8] = "Candle Shop",
        [9] = "Toy Shop",
        [10] = "Adult Novelties",
        [11] = "Cyber Cafe",
        [12] = "Grocery Store",
        [13] = "Theater",
        [14] = "Sweet Shop",
        [15] = "Cruise Line",
        [16] = "Television Network",
        [18] = "Zoo",
        [19] = "Firework Stand",
        [20] = "Property Broker",
        [21] = "Furniture Store",
        [22] = "Gas Station",
        [23] = "Music Store",
        [24] = "Nightclub",
        [25] = "Pub",
        [26] = "Gents Strip Club",
        [27] = "Restaurant",
        [28] = "Oil Rig",
        [29] = "Fitness Center",
        [30] = "Mechanic Shop",
        [31] = "Amusement Park",
        [32] = "Lingerie Store",
        [33] = "Meat Warehouse",
        [34] = "Farm",
        [35] = "Software Corp",
        [36] = "Ladies Strip Club",
        [37] = "Private Security Firm",
        [38] = "Mining Corporation",
        [39] = "Detective Agency",
        [40] = "Logistics Management"
    };

    public static string GetName(int typeId) =>
        Names.TryGetValue(typeId, out var name) ? name : $"Unknown ({typeId})";
}
