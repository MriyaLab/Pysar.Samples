using System.Threading.Tasks;
using Uno.UI.Hosting;

namespace Pysar.Uno.Sample;

internal class Program
{
    public static async Task Main(string[] args)
    {
        App.InitializeLogging();

        // Uno drives FileSavePicker through the File System Access API by default, and hands back
        // "the user cancelled" whenever that API is unavailable - which is every Safari and Firefox
        // visitor, not an edge case. The fallback turns those into an ordinary browser download
        // instead of a save that silently does nothing.
        // Fully qualified: inside Pysar.Uno.Sample a bare "Uno." binds to Pysar.Uno first.
        global::Uno.WinRTFeatureConfiguration.Storage.Pickers.WasmConfiguration =
            global::Uno.WasmPickerConfiguration.FileSystemAccessApiWithFallback;

        // Ctrl (or Command) plus wheel, and the trackpad pinch Chrome delivers as the same event,
        // zoom the page rather than the report unless the browser's own zoom is suppressed over the
        // canvas. ReportView cannot do this for itself - the script has to be imported, and a
        // control cannot await - so the head asks for it.
        await PysarUnoBrowser.UseWheelZoomAsync();

        var host = UnoPlatformHostBuilder.Create()
            .App(() => new App())
            .UseWebAssembly()
            .Build();

        await host.RunAsync();
    }
}
