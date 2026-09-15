using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Views;
using AndroidX.Core.View;

namespace Pysar.Uno.Sample;

[Application(
    LargeHeap = true,
    HardwareAccelerated = true,
    Theme = "@style/Theme.AppCompat.DayNight.NoActionBar")]
public class MainApplication : Microsoft.UI.Xaml.NativeApplication
{
    public MainApplication(IntPtr javaReference, JniHandleOwnership transfer)
        : base(() => new App(), javaReference, transfer)
    {
    }
}

[Activity(
    MainLauncher = true,
    Theme = "@style/Theme.AppCompat.DayNight.NoActionBar",
    ConfigurationChanges = global::Uno.UI.ActivityHelper.AllConfigChanges,
    WindowSoftInputMode = SoftInput.AdjustNothing | SoftInput.StateHidden)]
public class MainActivity : Microsoft.UI.Xaml.ApplicationActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        WindowCompat.SetDecorFitsSystemWindows(Window, true);
    }
}
