using Pysar.Sample.Shared.Data;

namespace Pysar.Sample.Shared.Reports.Invoice;

public partial class InvoiceReport
{
    public InvoiceReport(InvoiceData data)
    {
        InitializeComponent();
        DataContext = data;
    }

    public InvoiceReport()
    {
        InitializeComponent();
    }
}
