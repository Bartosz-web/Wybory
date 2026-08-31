using Microsoft.EntityFrameworkCore;

namespace Wybory.Web.Dane;

public class BazaDanych : DbContext
{
    public BazaDanych(DbContextOptions<BazaDanych> opcje) : base(opcje) { }

    public DbSet<Okreg> Okregi => Set<Okreg>();
    public DbSet<Wyborca> Wyborcy => Set<Wyborca>();
    public DbSet<Komitet> Komitety => Set<Komitet>();
    public DbSet<KomitetOkreg> KomitetyOkregow => Set<KomitetOkreg>();
    public DbSet<Kandydat> Kandydaci => Set<Kandydat>();
    public DbSet<Glos> Glosy => Set<Glos>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<Wyborca>().HasIndex(w => w.Pesel).IsUnique();
        mb.Entity<Komitet>().HasIndex(k => k.Nazwa).IsUnique();

        // Klucz złożony dla przynależności komitetu do okręgu (pkt 2).
        mb.Entity<KomitetOkreg>().HasKey(ko => new { ko.KomitetId, ko.OkregId });
        mb.Entity<KomitetOkreg>()
            .HasOne(ko => ko.Komitet)
            .WithMany(k => k.Okregi)
            .HasForeignKey(ko => ko.KomitetId)
            .OnDelete(DeleteBehavior.Cascade);
        mb.Entity<KomitetOkreg>()
            .HasOne(ko => ko.Okreg)
            .WithMany(o => o.KomitetyOkregu)
            .HasForeignKey(ko => ko.OkregId)
            .OnDelete(DeleteBehavior.Restrict);

        // Kandydat: wyborca może kandydować co najwyżej raz (pkt 3).
        mb.Entity<Kandydat>().HasIndex(k => k.WyborcaId).IsUnique();
        mb.Entity<Kandydat>()
            .HasOne(k => k.Wyborca)
            .WithOne(w => w.Kandydatura)
            .HasForeignKey<Kandydat>(k => k.WyborcaId)
            .OnDelete(DeleteBehavior.Restrict);
        // KomitetId nie-nullowalny (int, nie int?) => kandydat bez komitetu jest niemożliwy do zapisania.
        mb.Entity<Kandydat>()
            .HasOne(k => k.Komitet)
            .WithMany(k => k.Kandydaci)
            .HasForeignKey(k => k.KomitetId)
            .OnDelete(DeleteBehavior.Restrict);
        mb.Entity<Kandydat>()
            .HasOne(k => k.Okreg)
            .WithMany(o => o.Kandydaci)
            .HasForeignKey(k => k.OkregId)
            .OnDelete(DeleteBehavior.Restrict);
        // Numer na liście musi być unikatowy w obrębie listy komitetu w danym okręgu
        // (dwóch kandydatów tego samego komitetu w tym samym okręgu nie może mieć tego samego numeru).
        mb.Entity<Kandydat>().HasIndex(k => new { k.KomitetId, k.OkregId, k.NumerNaLiscie }).IsUnique();

        // Głos: jeden głos na wyborcę (pkt 4) — indeks unikalny jako siatka bezpieczeństwa
        // pod ewentualną logikę w usłudze (UslugaGlosowania).
        mb.Entity<Glos>().HasIndex(g => g.WyborcaId).IsUnique();
        mb.Entity<Glos>()
            .HasOne(g => g.Wyborca)
            .WithOne(w => w.Glos)
            .HasForeignKey<Glos>(g => g.WyborcaId)
            .OnDelete(DeleteBehavior.Restrict);
        mb.Entity<Glos>()
            .HasOne(g => g.Kandydat)
            .WithMany(k => k.Glosy)
            .HasForeignKey(g => g.KandydatId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
