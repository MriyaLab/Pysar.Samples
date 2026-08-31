namespace Pysar.Sample.Reports.Data;

public sealed record Organization(string Company, string Name, string Address, string Phone, string Email, string? Website = null, string? Logo = null)
{ 
    public static Organization GetOwnCompany => new Organization("Mriya Lab", "Andrii Kolodiichyk", "V.Velykoho Street, 5B, Drohobych, Lvivska Oblast, 82100, Ukraine", "+38 067 1572417", "support@mriyalab.com", "www.mriyalab.com", "Images/logo.svg");
}
