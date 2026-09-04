using Pysar.Binding;
using Pysar.Elements;
using Pysar.Sample.Reports.Data;

namespace Pysar.Sample.Reports.Views;

/// <summary>
/// The document banner: company identity (logo, name, contacts) on the left and the
/// document title, number and date on the right. Every variable part is a bindable
/// parameter, so the same component serves any document that carries a number and a date.
/// </summary>
public partial class CompanyHeader
{
    
    public static BindableProperty CompanyProperty { get; } =
        BindableProperty.Create(nameof(Company), typeof(Organization), typeof(CompanyHeader), null, propertyChanged: OnCompanyPropertyChanged);

    public static BindableProperty LogoSourceProperty { get; } =
        BindableProperty.Create(nameof(LogoSource), typeof(ImageSource), typeof(CompanyHeader), null);

    public static BindableProperty CompanyNameProperty { get; } =
        BindableProperty.Create(nameof(CompanyName), typeof(string), typeof(CompanyHeader), string.Empty);

    public static BindableProperty CompanyAddressProperty { get; } =
        BindableProperty.Create(nameof(CompanyAddress), typeof(string), typeof(CompanyHeader), string.Empty);

    public static BindableProperty CompanyPhoneProperty { get; } =
        BindableProperty.Create(nameof(CompanyPhone), typeof(string), typeof(CompanyHeader), string.Empty);

    public static BindableProperty CompanyEmailProperty { get; } =
        BindableProperty.Create(nameof(CompanyEmail), typeof(string), typeof(CompanyHeader), string.Empty);

    public static BindableProperty CompanyWebsiteProperty { get; } =
        BindableProperty.Create(nameof(CompanyWebsite), typeof(string), typeof(CompanyHeader), string.Empty);

    public static BindableProperty DocumentTitleProperty { get; } =
        BindableProperty.Create(nameof(DocumentTitle), typeof(string), typeof(CompanyHeader), string.Empty);

    public static BindableProperty DocumentNumberProperty { get; } =
        BindableProperty.Create(nameof(DocumentNumber), typeof(string), typeof(CompanyHeader), string.Empty);

    public static BindableProperty DocumentDateProperty { get; } =
        BindableProperty.Create(nameof(DocumentDate), typeof(string), typeof(CompanyHeader), string.Empty);

    public static BindableProperty DocumentNumberLabelProperty { get; } =
        BindableProperty.Create(nameof(DocumentNumberLabel), typeof(string), typeof(CompanyHeader), "Number:");

    public static BindableProperty DocumentDateLabelProperty { get; } =
        BindableProperty.Create(nameof(DocumentDateLabel), typeof(string), typeof(CompanyHeader), "Date:");

    public CompanyHeader() => InitializeComponent();

    private static void OnCompanyPropertyChanged(BindableObject control, object? oldValue, object? newValue)
    {
        if (control is CompanyHeader companyHeader && newValue is Organization customer)
        {
            companyHeader.CompanyName = customer.Company;
            companyHeader.CompanyAddress = customer.Address;
            companyHeader.CompanyWebsite = customer.Website;
            companyHeader.CompanyEmail = customer.Email;
            companyHeader.CompanyPhone = customer.Phone;
            companyHeader.LogoSource = new FileImageSource(customer.Logo);
        }
    }
    
    public Organization? Company
    {
        get => (Organization?)GetValue(CompanyProperty);
        set => SetValue(CompanyProperty, value);
    }
    
    /// <summary>The company logo shown at the left edge of the banner.</summary>
    public ImageSource? LogoSource
    {
        get => (ImageSource?)GetValue(LogoSourceProperty);
        set => SetValue(LogoSourceProperty, value);
    }

    public string CompanyName
    {
        get => (string)GetValue(CompanyNameProperty)!;
        set => SetValue(CompanyNameProperty, value);
    }

    public string CompanyAddress
    {
        get => (string)GetValue(CompanyAddressProperty)!;
        set => SetValue(CompanyAddressProperty, value);
    }

    public string CompanyPhone
    {
        get => (string)GetValue(CompanyPhoneProperty)!;
        set => SetValue(CompanyPhoneProperty, value);
    }

    public string CompanyEmail
    {
        get => (string)GetValue(CompanyEmailProperty)!;
        set => SetValue(CompanyEmailProperty, value);
    }

    public string? CompanyWebsite
    {
        get => (string)GetValue(CompanyWebsiteProperty)!;
        set => SetValue(CompanyWebsiteProperty, value);
    }

    /// <summary>The large caption on the right (for example "INVOICE").</summary>
    public string DocumentTitle
    {
        get => (string)GetValue(DocumentTitleProperty)!;
        set => SetValue(DocumentTitleProperty, value);
    }

    public string DocumentNumber
    {
        get => (string)GetValue(DocumentNumberProperty)!;
        set => SetValue(DocumentNumberProperty, value);
    }

    public string DocumentDate
    {
        get => (string)GetValue(DocumentDateProperty)!;
        set => SetValue(DocumentDateProperty, value);
    }

    /// <summary>The caption in front of <see cref="DocumentNumber"/>, e.g. "Invoice №:" or "Year:".</summary>
    public string DocumentNumberLabel
    {
        get => (string)GetValue(DocumentNumberLabelProperty)!;
        set => SetValue(DocumentNumberLabelProperty, value);
    }

    /// <summary>The caption in front of <see cref="DocumentDate"/>.</summary>
    public string DocumentDateLabel
    {
        get => (string)GetValue(DocumentDateLabelProperty)!;
        set => SetValue(DocumentDateLabelProperty, value);
    }
}
