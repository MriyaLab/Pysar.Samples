using Pysar.Sample.Shared.Data;

namespace Pysar.Sample.Shared.Reports;

public partial class AnnualReport
{
    public AnnualReport(AnnualLedger data)
    {
        InitializeComponent();
        DataContext = data;
    }

    public AnnualReport()
    {
        InitializeComponent();
    }
}