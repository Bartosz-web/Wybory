using Microsoft.EntityFrameworkCore;
using Wybory.Web.Dane;
using Wybory.Web.Uslugi;
using Wybory.Web.Uslugi.Formuly;

namespace Wybory.Tests;

public class UslugaZliczaniaTests
{
    private const int OkregDomyslny = 1;

    // Nazwa komitetu i liczba głosów oddanych na kolejnych jego kandydatów.
    private record Lista(string Nazwa, int[] GlosyKandydatow);

    private static BazaDanych NowaBaza() =>
        new(new DbContextOptionsBuilder<BazaDanych>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    // Rozkłada głosy równomiernie na zadaną liczbę kandydatów.
    private static int[] Rozloz(int kandydatow, int glosow)
    {
        var wynik = new int[kandydatow];
        for (var i = 0; i < glosow; i++)
            wynik[i % kandydatow]++;
        return wynik;
    }

    private static BazaDanych ZbudujOkreg(int liczbaMandatow, Lista[] listy, int uprawnionychBezGlosu = 0)
        => ZbudujOkregi([(OkregDomyslny, liczbaMandatow, listy)], uprawnionychBezGlosu);

    // Każdy głos to osobny wyborca z czynnym prawem. Kandydaci mają czynne prawo
    // wyłączone, żeby nie zawyżali liczby uprawnionych w teście frekwencji.
    private static BazaDanych ZbudujOkregi(
        (int OkregId, int LiczbaMandatow, Lista[] Listy)[] okregi,
        int uprawnionychBezGlosu = 0)
    {
        var db = NowaBaza();

        var nastepnaOsoba = 1;
        var nastepnyKandydat = 1;
        var nastepnyGlos = 1;
        var komitetIdPoNazwie = new Dictionary<string, int>();

        foreach (var (okregId, liczbaMandatow, listy) in okregi)
        {
            db.Okregi.Add(new Okreg
            {
                Id = okregId,
                Nazwa = $"Okręg nr {okregId}",
                LiczbaMandatow = liczbaMandatow
            });

            foreach (var lista in listy)
            {
                if (!komitetIdPoNazwie.TryGetValue(lista.Nazwa, out var komitetId))
                {
                    komitetId = komitetIdPoNazwie.Count + 1;
                    komitetIdPoNazwie[lista.Nazwa] = komitetId;
                    db.Komitety.Add(new Komitet { Id = komitetId, Nazwa = lista.Nazwa });
                }

                db.KomitetyOkregow.Add(new KomitetOkreg { KomitetId = komitetId, OkregId = okregId });

                for (var numer = 1; numer <= lista.GlosyKandydatow.Length; numer++)
                {
                    var osobaKandydata = nastepnaOsoba++;
                    db.Wyborcy.Add(NowaOsoba(osobaKandydata, okregId, czynnePrawo: false, biernePrawo: true));

                    var kandydatId = nastepnyKandydat++;
                    db.Kandydaci.Add(new Kandydat
                    {
                        Id = kandydatId,
                        WyborcaId = osobaKandydata,
                        KomitetId = komitetId,
                        OkregId = okregId,
                        NumerNaLiscie = numer
                    });

                    for (var i = 0; i < lista.GlosyKandydatow[numer - 1]; i++)
                    {
                        var glosujacy = nastepnaOsoba++;
                        db.Wyborcy.Add(NowaOsoba(glosujacy, okregId, czynnePrawo: true, biernePrawo: false));
                        db.Glosy.Add(new Glos
                        {
                            Id = nastepnyGlos++,
                            WyborcaId = glosujacy,
                            KandydatId = kandydatId,
                            DataOddania = DateTime.UtcNow
                        });
                    }
                }
            }

            for (var i = 0; i < uprawnionychBezGlosu; i++)
                db.Wyborcy.Add(NowaOsoba(nastepnaOsoba++, okregId, czynnePrawo: true, biernePrawo: false));
        }

        db.SaveChanges();
        return db;
    }

    private static Wyborca NowaOsoba(int id, int okregId, bool czynnePrawo, bool biernePrawo) => new()
    {
        Id = id,
        Pesel = id.ToString("D11"),
        Imie = "Osoba",
        Nazwisko = $"Numer{id}",
        OkregId = okregId,
        CzynnePrawoWyborcze = czynnePrawo,
        BierneProwoWyborcze = biernePrawo
    };

    // A=60, B=35, C=4, D=1 głosów przy 25 mandatach. C ma 4%, więc nie przekracza
    // progu 5%, ale bez progu zdobyłby jeden mandat.
    private static Lista[] ListyZKomitetemPonizejProgu() =>
    [
        new("A", Rozloz(16, 60)),
        new("B", Rozloz(9, 35)),
        new("C", [4]),
        new("D", [1])
    ];

    [Fact]
    public async Task ZliczWyniki_BezProgu_MalyKomitetZdobywaMandat()
    {
        var db = ZbudujOkreg(25, ListyZKomitetemPonizejProgu());

        var wyniki = await new UslugaZliczania(db).ZliczWynikiAsync(OkregDomyslny, new FormulaDHondta());

        Assert.Equal(15, wyniki.Komitety.Single(k => k.NazwaKomitetu == "A").Mandaty);
        Assert.Equal(9, wyniki.Komitety.Single(k => k.NazwaKomitetu == "B").Mandaty);
        Assert.Equal(1, wyniki.Komitety.Single(k => k.NazwaKomitetu == "C").Mandaty);
        Assert.All(wyniki.Komitety, k => Assert.False(k.PonizejProgu));
    }

    [Fact]
    public async Task ZliczWyniki_ZProgiem_MandatPrzechodziDoKomitetuPowyzejProgu()
    {
        var db = ZbudujOkreg(25, ListyZKomitetemPonizejProgu());

        var wyniki = await new UslugaZliczania(db)
            .ZliczWynikiAsync(OkregDomyslny, new FormulaDHondta(), UslugaZliczania.ProgSejmowy);

        var c = wyniki.Komitety.Single(k => k.NazwaKomitetu == "C");
        Assert.True(c.PonizejProgu);
        Assert.Equal(0, c.Mandaty);

        Assert.Equal(16, wyniki.Komitety.Single(k => k.NazwaKomitetu == "A").Mandaty);
        Assert.Equal(9, wyniki.Komitety.Single(k => k.NazwaKomitetu == "B").Mandaty);
    }

    [Fact]
    public async Task ZliczWyniki_ZProgiem_GlosyPonizejProguZostajaWMianownikuProcentow()
    {
        var db = ZbudujOkreg(25, ListyZKomitetemPonizejProgu());

        var wyniki = await new UslugaZliczania(db)
            .ZliczWynikiAsync(OkregDomyslny, new FormulaDHondta(), UslugaZliczania.ProgSejmowy);

        Assert.Equal(60.0, wyniki.Komitety.Single(k => k.NazwaKomitetu == "A").ProcentGlosow, 3);
        Assert.Equal(4.0, wyniki.Komitety.Single(k => k.NazwaKomitetu == "C").ProcentGlosow, 3);
    }

    [Fact]
    public async Task ZliczWyniki_ProgWykluczaWszystkieKomitety_JestPomijany()
    {
        // 21 komitetów po jednym głosie: każdy ma 4,76% i żaden nie przekracza progu.
        var listy = Enumerable.Range(1, 21)
            .Select(i => new Lista($"Komitet {i:D2}", [1]))
            .ToArray();
        var db = ZbudujOkreg(5, listy);

        var wyniki = await new UslugaZliczania(db)
            .ZliczWynikiAsync(OkregDomyslny, new FormulaDHondta(), UslugaZliczania.ProgSejmowy);

        Assert.Equal(5, wyniki.Komitety.Sum(k => k.Mandaty));
        Assert.All(wyniki.Komitety, k => Assert.False(k.PonizejProgu));
    }

    [Fact]
    public async Task ZliczWyniki_LiczyFrekwencjeWzgledemUprawnionych()
    {
        var db = ZbudujOkreg(1, [new Lista("A", [2])], uprawnionychBezGlosu: 3);

        var wyniki = await new UslugaZliczania(db).ZliczWynikiAsync(OkregDomyslny, new FormulaDHondta());

        Assert.Equal(5, wyniki.LiczbaUprawnionych);
        Assert.Equal(2, wyniki.LiczbaGlosow);
        Assert.Equal(40.0, wyniki.Frekwencja, 3);
    }

    [Fact]
    public async Task ZliczWyniki_ListaOtwarta_MandatDlaKandydataZNajwiekszaLiczbaGlosow()
    {
        // Kandydat nr 1 ma 2 głosy, kandydat nr 2 ma 10. Mandat należy się drugiemu.
        var db = ZbudujOkreg(1, [new Lista("A", [2, 10])]);

        var wyniki = await new UslugaZliczania(db).ZliczWynikiAsync(OkregDomyslny, new FormulaDHondta());

        var kandydaci = wyniki.Komitety.Single().Kandydaci;
        Assert.True(kandydaci.Single(k => k.NumerNaLiscie == 2).CzyZdobylMandat);
        Assert.False(kandydaci.Single(k => k.NumerNaLiscie == 1).CzyZdobylMandat);
    }

    [Fact]
    public async Task ZliczWyniki_NieistniejacyOkreg_RzucaWyjatek()
    {
        var db = ZbudujOkreg(5, [new Lista("A", [1])]);

        await Assert.ThrowsAsync<BladRegulyBiznesowej>(
            () => new UslugaZliczania(db).ZliczWynikiAsync(999, new FormulaDHondta()));
    }

    [Fact]
    public async Task PodsumujWszystkieOkregi_SumujeMandatyZOkregow()
    {
        // Okręg 1 przy 3 mandatach: A=30, B=11 daje A=2, B=1.
        // Okręg 2 przy 2 mandatach: A=10, B=30 daje B=2, A=0.
        var db = ZbudujOkregi(
        [
            (1, 3, [new Lista("A", [30]), new Lista("B", [11])]),
            (2, 2, [new Lista("A", [10]), new Lista("B", [30])])
        ]);

        var podsumowanie = await new UslugaZliczania(db).PodsumujWszystkieOkregiAsync(new FormulaDHondta());

        Assert.Equal(5, podsumowanie.LiczbaMandatow);
        Assert.Equal(81, podsumowanie.LiczbaGlosow);
        Assert.Equal(2, podsumowanie.Okregi.Count);

        Assert.Equal(2, podsumowanie.Komitety.Single(k => k.NazwaKomitetu == "A").Mandaty);
        Assert.Equal(40, podsumowanie.Komitety.Single(k => k.NazwaKomitetu == "A").Glosy);
        Assert.Equal(3, podsumowanie.Komitety.Single(k => k.NazwaKomitetu == "B").Mandaty);
        Assert.Equal(41, podsumowanie.Komitety.Single(k => k.NazwaKomitetu == "B").Glosy);
    }

    [Fact]
    public async Task PodsumujWszystkieOkregi_SortujeMalejacoWgMandatow()
    {
        var db = ZbudujOkregi(
        [
            (1, 3, [new Lista("A", [30]), new Lista("B", [11])]),
            (2, 2, [new Lista("A", [10]), new Lista("B", [30])])
        ]);

        var podsumowanie = await new UslugaZliczania(db).PodsumujWszystkieOkregiAsync(new FormulaDHondta());

        Assert.Equal("B", podsumowanie.Komitety[0].NazwaKomitetu);
        Assert.Equal("A", podsumowanie.Komitety[1].NazwaKomitetu);
    }

    [Fact]
    public async Task PodsumujWszystkieOkregi_BrakGlosow_ZwracaZera()
    {
        var db = ZbudujOkregi([(1, 5, Array.Empty<Lista>())], uprawnionychBezGlosu: 4);

        var podsumowanie = await new UslugaZliczania(db).PodsumujWszystkieOkregiAsync(new FormulaDHondta());

        Assert.Equal(0, podsumowanie.LiczbaGlosow);
        Assert.Equal(4, podsumowanie.LiczbaUprawnionych);
        Assert.Equal(0.0, podsumowanie.Frekwencja);
        Assert.Empty(podsumowanie.Komitety);
    }
}
