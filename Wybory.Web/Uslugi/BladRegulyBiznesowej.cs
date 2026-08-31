namespace Wybory.Web.Uslugi;

// Rzucany, gdy operacja łamie regułę biznesową (np. brak czynnego prawa wyborczego,
// kandydat bez komitetu, podwójne głosowanie) — Razor Pages łapie go i pokazuje
// komunikat w ModelState zamiast strony błędu.
public class BladRegulyBiznesowej(string komunikat) : Exception(komunikat);
