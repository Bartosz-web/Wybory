namespace Wybory.Web.Pages.Shared;

public record DaneFrekwencji(string Etykieta, int Uprawnionych, int Glosow)
{
    public double Frekwencja => Uprawnionych > 0 ? Glosow * 100.0 / Uprawnionych : 0;
}
