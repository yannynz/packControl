namespace PackControl.Domain.Orders;

public enum OrderStatus
{
    Draft = 0,
    AwaitingTechnicalAnalysis = 1,
    AwaitingQuote = 2,
    Approved = 3,
    InProduction = 4
}
