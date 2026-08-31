using Microsoft.EntityFrameworkCore;
using Wybory.Web.Dane;
using Wybory.Web.Uslugi;

namespace Wybory.Tests;

public class UslugaRejestracjiTests
{
    private static BazaDanych NowaBaza()
    {
        var opcje = new DbContextOptionsBuilder<BazaDanych>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var db = new BazaDanych(opcje);
        db.Okregi.AddRange(
            new Okreg { Id = 1, Nazwa = "Okręg nr 1", LiczbaMandatow = 5 },
            new Okreg { Id = 2, Nazwa = "Okręg nr 2", LiczbaMandatow = 5 });
        db.SaveChanges();
        return db;
    }

    [Fact]
    public async Task RejestrujWyborce_PoprawnyPesel_ZapisujeWJednymOkregu()
    {
        var db = NowaBaza();
        var usluga = new UslugaRejestracji(db);

        var wyborca = await usluga.RejestrujWyborceAsync("11111111116", "Jan", "Kowalski", 1, true, false);

        Assert.Equal(1, wyborca.OkregId);
        Assert.Single(db.Wyborcy);
    }

    [Fact]
    public async Task RejestrujWyborce_NiepoprawnyPesel_RzucaWyjatek()
    {
        var usluga = new UslugaRejestracji(NowaBaza());

        await Assert.ThrowsAsync<BladRegulyBiznesowej>(
            () => usluga.RejestrujWyborceAsync("00000000001", "Jan", "Kowalski", 1, true, false));
    }

    [Fact]
    public async Task RejestrujWyborce_DuplikatPesel_RzucaWyjatek()
    {
        var db = NowaBaza();
        var usluga = new UslugaRejestracji(db);
        await usluga.RejestrujWyborceAsync("11111111116", "Jan", "Kowalski", 1, true, false);

        await Assert.ThrowsAsync<BladRegulyBiznesowej>(
            () => usluga.RejestrujWyborceAsync("11111111116", "Inna", "Osoba", 2, true, false));
    }

    [Fact]
    public async Task RejestrujKandydata_KomitetNiezarejestrowanyWOkregu_RzucaWyjatek()
    {
        var db = NowaBaza();
        var usluga = new UslugaRejestracji(db);
        var wyborca = await usluga.RejestrujWyborceAsync("11111111116", "Jan", "Kowalski", 1, true, true);
        // Komitet zarejestrowany tylko w okręgu 2.
        var komitet = await usluga.RejestrujKomitetAsync("Komitet A", [2]);

        await Assert.ThrowsAsync<BladRegulyBiznesowej>(
            () => usluga.RejestrujKandydataAsync(wyborca.Id, komitet.Id, 1, 1));
    }

    [Fact]
    public async Task RejestrujKandydata_WyborcaBezBiernegoPrawa_RzucaWyjatek()
    {
        var db = NowaBaza();
        var usluga = new UslugaRejestracji(db);
        var wyborca = await usluga.RejestrujWyborceAsync("11111111116", "Jan", "Kowalski", 1, true, false);
        var komitet = await usluga.RejestrujKomitetAsync("Komitet A", [1]);

        await Assert.ThrowsAsync<BladRegulyBiznesowej>(
            () => usluga.RejestrujKandydataAsync(wyborca.Id, komitet.Id, 1, 1));
    }

    [Fact]
    public async Task RejestrujKandydata_PoprawnyKomitetWOkregu_Zapisuje()
    {
        var db = NowaBaza();
        var usluga = new UslugaRejestracji(db);
        var wyborca = await usluga.RejestrujWyborceAsync("11111111116", "Jan", "Kowalski", 1, true, true);
        var komitet = await usluga.RejestrujKomitetAsync("Komitet A", [1, 2]);

        var kandydat = await usluga.RejestrujKandydataAsync(wyborca.Id, komitet.Id, 1, 1);

        Assert.Equal(komitet.Id, kandydat.KomitetId);
        Assert.Equal(1, kandydat.OkregId);
    }

    [Fact]
    public async Task RejestrujKandydata_ZajetyNumerNaLiscieWTymSamymKomitecie_RzucaWyjatek()
    {
        var db = NowaBaza();
        var usluga = new UslugaRejestracji(db);
        var wyborca1 = await usluga.RejestrujWyborceAsync("11111111116", "Jan", "Kowalski", 1, true, true);
        var wyborca2 = await usluga.RejestrujWyborceAsync("22222222222", "Anna", "Nowak", 1, true, true);
        var komitet = await usluga.RejestrujKomitetAsync("Komitet A", [1]);
        await usluga.RejestrujKandydataAsync(wyborca1.Id, komitet.Id, 1, 1);

        await Assert.ThrowsAsync<BladRegulyBiznesowej>(
            () => usluga.RejestrujKandydataAsync(wyborca2.Id, komitet.Id, 1, 1));
    }

    [Fact]
    public async Task RejestrujKandydata_TenSamNumerWInnymKomitecie_JestDozwolony()
    {
        var db = NowaBaza();
        var usluga = new UslugaRejestracji(db);
        var wyborca1 = await usluga.RejestrujWyborceAsync("11111111116", "Jan", "Kowalski", 1, true, true);
        var wyborca2 = await usluga.RejestrujWyborceAsync("22222222222", "Anna", "Nowak", 1, true, true);
        var komitetA = await usluga.RejestrujKomitetAsync("Komitet A", [1]);
        var komitetB = await usluga.RejestrujKomitetAsync("Komitet B", [1]);
        await usluga.RejestrujKandydataAsync(wyborca1.Id, komitetA.Id, 1, 1);

        var kandydat2 = await usluga.RejestrujKandydataAsync(wyborca2.Id, komitetB.Id, 1, 1);

        Assert.Equal(1, kandydat2.NumerNaLiscie);
    }

    [Fact]
    public async Task RejestrujKandydata_NumerPozaZakresem_RzucaWyjatek()
    {
        var db = NowaBaza();
        var usluga = new UslugaRejestracji(db);
        var wyborca = await usluga.RejestrujWyborceAsync("11111111116", "Jan", "Kowalski", 1, true, true);
        var komitet = await usluga.RejestrujKomitetAsync("Komitet A", [1]);

        await Assert.ThrowsAsync<BladRegulyBiznesowej>(
            () => usluga.RejestrujKandydataAsync(wyborca.Id, komitet.Id, 1, 11));
    }

    [Fact]
    public async Task RejestrujKandydata_ListaPelna_JedenastyKandydatOdrzucony()
    {
        // Ważne numery PESEL (poprawna suma kontrolna) dla 11 różnych osób.
        string[] pesele =
        [
            "11111111116", "22222222222", "33333333338", "44444444444", "55555555550",
            "66666666666", "77777777772", "88888888888", "99999999994", "00000000000",
            "12345678903"
        ];

        var db = NowaBaza();
        var usluga = new UslugaRejestracji(db);
        var komitet = await usluga.RejestrujKomitetAsync("Komitet A", [1]);

        for (var i = 0; i < UslugaRejestracji.MaksymalnaLiczbaKandydatowNaLiscie; i++)
        {
            var wyborca = await usluga.RejestrujWyborceAsync(pesele[i], "Kandydat", $"Numer{i + 1}", 1, true, true);
            await usluga.RejestrujKandydataAsync(wyborca.Id, komitet.Id, 1, i + 1);
        }

        var jedenasty = await usluga.RejestrujWyborceAsync(pesele[10], "Kandydat", "Jedenasty", 1, true, true);

        // Lista jest pełna (10/10) — każdy numer 1-10 jest już zajęty.
        await Assert.ThrowsAsync<BladRegulyBiznesowej>(
            () => usluga.RejestrujKandydataAsync(jedenasty.Id, komitet.Id, 1, 1));
    }
}
