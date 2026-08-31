using Pysar.Sample.Reports.Data;

namespace Pysar.Sample.Reports.Reports.Invoice;

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
