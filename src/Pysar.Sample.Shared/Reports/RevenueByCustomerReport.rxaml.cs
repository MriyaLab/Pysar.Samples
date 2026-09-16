using Pysar.Core.Abstractions;
using Pysar.Sample.Shared.Data;

namespace Pysar.Sample.Shared.Reports;

public partial class RevenueByCustomerReport
{
    public RevenueByCustomerReport(RevenueReportData data)
    {
        InitializeComponent();
        DataContext = data;
    }

    public RevenueByCustomerReport()
    {
        InitializeComponent();
    }
}
