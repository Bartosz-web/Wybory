using Microsoft.EntityFrameworkCore;
using Wybory.Web.Dane;
using Wybory.Web.Uslugi;

namespace Wybory.Tests;

public class UslugaGlosowaniaTests
{
    private static async Task<(BazaDanych Db, Wyborca Wyborca, Kandydat KandydatWOkregu, Kandydat KandydatSpozaOkregu)> PrzygotujDaneAsync(bool czynnePrawo)
    {
        var opcje = new DbContextOptionsBuilder<BazaDanych>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var db = new BazaDanych(opcje);
        db.Okregi.AddRange(
            new Okreg { Id = 1, Nazwa = "Okręg nr 1", LiczbaMandatow = 5 },
            new Okreg { Id = 2, Nazwa = "Okręg nr 2", LiczbaMandatow = 5 });
        await db.SaveChangesAsync();

        var usluga = new UslugaRejestracji(db);
        var wyborca = await usluga.RejestrujWyborceAsync("11111111116", "Jan", "Kowalski", 1, czynnePrawo, false);

        var kandydatOsoba1 = await usluga.RejestrujWyborceAsync("22222222222", "Kandydat", "Jeden", 1, false, true);
        var kandydatOsoba2 = await usluga.RejestrujWyborceAsync("33333333338", "Kandydat", "Dwa", 2, false, true);

        var komitet = await usluga.RejestrujKomitetAsync("Komitet A", [1, 2]);
        var kandydatWOkregu = await usluga.RejestrujKandydataAsync(kandydatOsoba1.Id, komitet.Id, 1, 1);
        var kandydatSpozaOkregu = await usluga.RejestrujKandydataAsync(kandydatOsoba2.Id, komitet.Id, 2, 1);

        return (db, wyborca, kandydatWOkregu, kandydatSpozaOkregu);
    }

    [Fact]
    public async Task OddajGlos_BezCzynnegoPrawa_RzucaWyjatek()
    {
        var (db, wyborca, kandydat, _) = await PrzygotujDaneAsync(czynnePrawo: false);
        var usluga = new UslugaGlosowania(db);

        await Assert.ThrowsAsync<BladRegulyBiznesowej>(() => usluga.OddajGlosAsync(wyborca.Pesel, kandydat.Id));
    }

    [Fact]
    public async Task OddajGlos_KandydatSpozaOkreguWyborcy_RzucaWyjatek()
    {
        var (db, wyborca, _, kandydatSpozaOkregu) = await PrzygotujDaneAsync(czynnePrawo: true);
        var usluga = new UslugaGlosowania(db);

        await Assert.ThrowsAsync<BladRegulyBiznesowej>(() => usluga.OddajGlosAsync(wyborca.Pesel, kandydatSpozaOkregu.Id));
    }

    [Fact]
    public async Task OddajGlos_Poprawny_ZapisujeGlos()
    {
        var (db, wyborca, kandydat, _) = await PrzygotujDaneAsync(czynnePrawo: true);
        var usluga = new UslugaGlosowania(db);

        await usluga.OddajGlosAsync(wyborca.Pesel, kandydat.Id);

        Assert.Single(db.Glosy);
        Assert.Equal(kandydat.Id, db.Glosy.Single().KandydatId);
    }

    [Fact]
    public async Task OddajGlos_PodwojneGlosowanie_RzucaWyjatek()
    {
        var (db, wyborca, kandydat, _) = await PrzygotujDaneAsync(czynnePrawo: true);
        var usluga = new UslugaGlosowania(db);
        await usluga.OddajGlosAsync(wyborca.Pesel, kandydat.Id);

        await Assert.ThrowsAsync<BladRegulyBiznesowej>(() => usluga.OddajGlosAsync(wyborca.Pesel, kandydat.Id));
    }
}
