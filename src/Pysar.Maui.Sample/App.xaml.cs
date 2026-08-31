using Microsoft.Extensions.DependencyInjection;

namespace Pysar.Maui.Sample;

public partial class App : Application
{
    private readonly IServiceProvider _services;

    /*
      The shell is resolved in CreateWindow rather than injected here. Taking an AppShell parameter
      makes the container build the shell before this constructor is entered, so the shell's
      InitializeComponent runs ahead of the one below - and every StaticResource in AppShell.xaml
      is looked up against an Application.Resources that the line below has not filled in yet.
    */
    public App(IServiceProvider services)
    {
        InitializeComponent();

        _services = services;
    }

    protected override Window CreateWindow(IActivationState? activationState)
        => new(_services.GetRequiredService<AppShell>());
}
