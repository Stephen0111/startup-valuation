using Newtonsoft.Json;

public class StartupValuationRequest
{
    [JsonProperty("companyName")]
    public string CompanyName { get; set; }

    [JsonProperty("industry")]
    public string Industry { get; set; }

    [JsonProperty("stage")]
    public string Stage { get; set; }

    [JsonProperty("foundingYear")]
    public int? FoundingYear { get; set; }

    [JsonProperty("monthlyRevenue")]
    public double? MonthlyRevenue { get; set; }

    [JsonProperty("revenueGrowthRate")]
    public double? RevenueGrowthRate { get; set; }

    [JsonProperty("monthlyExpenses")]
    public double? MonthlyExpenses { get; set; }

    [JsonProperty("fundingRaised")]
    public double? FundingRaised { get; set; }

    [JsonProperty("burnRate")]
    public double? BurnRate { get; set; }

    [JsonProperty("monthsToBreakeven")]
    public double? MonthsToBreakeven { get; set; }

    [JsonProperty("customersCount")]
    public int? CustomersCount { get; set; }

    [JsonProperty("teamSize")]
    public int? TeamSize { get; set; }

    [JsonProperty("marketSize")]
   public string? MarketSize { get; set; }


    [JsonProperty("intellectualProperty")]
    public string IntellectualProperty { get; set; }

    [JsonProperty("competitorValuation")]
    public double? CompetitorValuation { get; set; }

    [JsonProperty("customerAcquisitionCost")]
    public double? CustomerAcquisitionCost { get; set; }

    [JsonProperty("lifetimeValue")]
    public double? LifetimeValue { get; set; }
}
