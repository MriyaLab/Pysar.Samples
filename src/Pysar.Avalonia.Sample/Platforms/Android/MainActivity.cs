using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Avalonia;
using Avalonia.Android;

namespace Pysar.Avalonia.Sample;

/// <summary>
///     The Android entry point. Android has no Main: the application object builds Avalonia and the
///     launcher activity hosts it, and the shared <see cref="App" /> then shows MainView through
///     the single-view lifetime - the same path the browser takes.
/// </summary>
[Application]
public class MainApplication : AvaloniaAndroidApplication<App>
{
    public MainApplication(IntPtr javaReference, JniHandleOwnership transfer)
        : base(javaReference, transfer)
    {
    }

    // The platform-independent half of the bootstrap is shared with every other platform; Android
    // needs no UsePlatformDetect, the Android platform is already installed by this base class.
    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        // The desktop printer shells out to an OS shell that Android does not have, so the viewer
        // is given one that goes through the system print framework instead.
        SampleServices.Printer = new AndroidReportPrinter();

        return base.CustomizeAppBuilder(builder)
            .WithInterFont()
            .ConfigurePysarSample();
    }
}

[Activity(
    Label = "Pysar Samples",
    MainLauncher = true,
    Theme = "@style/Theme.AppCompat.DayNight.NoActionBar",
    ConfigurationChanges = ConfigChanges.Orientation
        | ConfigChanges.ScreenSize
        | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity
{
    /// <summary>
    ///     The activity the printer raises the system print dialog from. Held statically because
    ///     the viewer reaches the printer through <see cref="SampleServices" />, which knows
    ///     nothing about Android.
    /// </summary>
    internal static MainActivity? Current { get; private set; }

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        Current = this;

        base.OnCreate(savedInstanceState);
    }

    protected override void OnDestroy()
    {
        if (ReferenceEquals(Current, this))
            Current = null;

        base.OnDestroy();
    }
}
