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

        var host = UnoPlatformHostBuilder.Create()
            .App(() => new App())
            .UseWebAssembly()
            .Build();

        await host.RunAsync();
    }
}
