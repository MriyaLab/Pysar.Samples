using Pysar.Core.Abstractions;

namespace Pysar.Sample.Reports.Data;

public sealed record InvoiceData(
    string Number,
    string Date,
    Organization Company,
    Organization Customer,
    IReadOnlyList<InvoiceItem> Items,
    Payment Payment) : IDesignTimeCreatable<InvoiceData>
{
    public decimal SubTotal => Items.Sum(i => i.UnitPrice * i.Quantity);
    public decimal DiscountTotal => Items.Sum(i => i.Discount);
    public decimal GrandTotal => Items.Sum(i => i.Total);

    /// <summary>The sample invoice used by both the console application and design-time preview.</summary>
    public static InvoiceData CreateDesignInstance() => new(
        "10643", "03/29/26",
        Organization.GetOwnCompany,
        new Organization("Alfreds Futterkiste", "Maria Anders",
            "Obere Str. 57, Berlin, Germany, 12209", "030-0074321", "alfredsfutterkiste@mail.com"),
        InvoiceItems.ToArray(),
        new Payment("123-45-6789", "1st Enterprise Bank", "SWFTKUS6LXXX", "Visa, MasterCard, American Express"));
    
    private static IEnumerable<InvoiceItem> InvoiceItems { get; } = new[]
    {
        new InvoiceItem(1, "Rössle Sauerkraut", 45.60m, 15, 3.75m),
        new InvoiceItem(2, "Chartreuse verte", 18.00m, 21, 5.25m),
        new InvoiceItem(3, "Spegesild", 12.00m, 2, 0.50m),
        new InvoiceItem(4, "Formagator Ltd.", 2.00m, 12, 5.0m),
        new InvoiceItem(5, "Cho Ho Wang co.", 15.00m, 1, 5.0m),
        new InvoiceItem(6, "ChartWang verte", 23.00m, 6, 2.0m),
        new InvoiceItem(7, "SpegHo Sauerkraut", 4.00m, 7, 5.0m)
    };
}

public sealed record InvoiceItem(int Index, string Product, decimal UnitPrice, int Quantity, decimal Discount)
{
    public decimal Total => UnitPrice * Quantity - Discount;
}

public sealed record Payment(string Account, string Bank, string Swift, string Options);
