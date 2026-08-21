using PackControl.Domain.Orders;

namespace PackControl.Domain.Tests;

public sealed class OrderTests
{
    [Fact]
    public void Create_AddScopeItem_AndRegisterPendingAnalysis_ShouldKeepAggregateConsistent()
    {
        var order = Order.Create(
            "PED-TEST-001",
            Guid.NewGuid(),
            ServiceType.New,
            UrgencyLevel.Normal,
            "Pedido de teste para facaria",
            null,
            null,
            DateTime.UtcNow,
            "tester");

        order.AddScopeItem(
            "Faca principal",
            "produto_principal",
            1,
            null,
            null,
            "Por unidade",
            1250m,
            null,
            DateTime.UtcNow,
            "tester");
        var attachment = order.AddAttachment(
            "desenho.pdf",
            "abc123.pdf",
            "2026/03/abc123.pdf",
            "application/pdf",
            1024,
            "hash",
            DateTime.UtcNow,
            "tester");
        order.RegisterPendingAnalysis(
            attachment.Id,
            ".pdf",
            "Aguardando parser real.",
            DateTime.UtcNow,
            "tester");

        Assert.Equal(OrderStatus.AwaitingTechnicalAnalysis, order.Status);
        Assert.Single(order.ScopeItems);
        Assert.Single(order.Attachments);
        Assert.Single(order.Analyses);
        Assert.Equal(".pdf", order.Analyses[0].SourceFileExtension);
    }
}
