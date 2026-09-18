using Avalonia.Controls;

namespace Pysar.Avalonia.Sample.Views;

/// <summary>
///     The desktop frame around <see cref="MainView" />. All of the sample's behaviour lives in the
///     view now, so the window itself only needs to load its XAML.
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow() => InitializeComponent();
}
