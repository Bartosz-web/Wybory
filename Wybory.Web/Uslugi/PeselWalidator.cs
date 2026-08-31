namespace Wybory.Web.Uslugi;

// Walidacja formatu numeru PESEL: 11 cyfr + standardowa suma kontrolna.
public static class PeselWalidator
{
    private static readonly int[] Wagi = [1, 3, 7, 9, 1, 3, 7, 9, 1, 3];

    public static bool CzyPoprawny(string? pesel)
    {
        if (pesel is null || pesel.Length != 11 || !pesel.All(char.IsDigit))
            return false;

        var suma = 0;
        for (var i = 0; i < 10; i++)
            suma += Wagi[i] * (pesel[i] - '0');

        var cyfraKontrolna = (10 - suma % 10) % 10;
        return cyfraKontrolna == pesel[10] - '0';
    }
}
