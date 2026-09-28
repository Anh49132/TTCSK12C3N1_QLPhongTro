namespace QL_PhongTro.ViewModels;

public sealed class StagingOverviewViewModel
{
    public int BuildingCount { get; init; }
    public int RoomCount { get; init; }
    public int ContractCount { get; init; }
    public int InvoiceCount { get; init; }
    public int InvoicePeriodCount { get; init; }
    public int PaidInFullCount { get; init; }
    public int PaidPartiallyCount { get; init; }
    public int OverdueCount { get; init; }
}