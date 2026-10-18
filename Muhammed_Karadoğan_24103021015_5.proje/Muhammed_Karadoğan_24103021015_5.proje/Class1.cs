using System;
using System.Collections.Generic;
using System.Linq;

public enum Kategori
{
    Konser,
    Spor,
    Tiyatro,
    Konferans
}

public class Etkinlik
{
    public string Ad { get; set; } = string.Empty;
    public DateTime Tarih { get; set; }
    public string Mekan { get; set; } = string.Empty;
    public double TabanFiyat { get; set; }
    public Kategori Kategori { get; set; }
    public int Kapasite { get; set; }
    public int SatilanBiletSayisi { get; set; }

    public double DolulukOrani => Kapasite > 0 ? (double)SatilanBiletSayisi / Kapasite : 0;

    public static int GetKategoriLimiti(Kategori kategori) => kategori switch
    {
        Kategori.Konser or Kategori.Spor => 3,
        Kategori.Tiyatro or Kategori.Konferans => 2,
        _ => 0
    };

    public double NihaiFiyatiHesapla()
    {
        double guncelFiyat = TabanFiyat;
        double doluluk = DolulukOrani;

        if (doluluk > 0.80)
            guncelFiyat *= 1.10 * 1.15;
        else if (doluluk > 0.50)
            guncelFiyat *= 1.10;

        if ((Tarih - DateTime.Now).TotalDays >= 14)
            guncelFiyat *= 0.90;

        return Math.Round(guncelFiyat, 2);
    }

    public override string ToString()
    {
        double anlikFiyat = NihaiFiyatiHesapla();
        string fiyatBilgisi = anlikFiyat != TabanFiyat ? $" (Anlık Fiyat: {anlikFiyat:C})" : "";

        return $"Etkinlik: {Ad} [{Kategori}]\nMekan: {Mekan}\nTarih: {Tarih:dd MMMM yyyy HH:mm}\nKapasite: {SatilanBiletSayisi}/{Kapasite} ({DolulukOrani:P0})\nFiyat: {TabanFiyat:C}{fiyatBilgisi}";
    }
}

public class Kullanici
{
    public string Ad { get; set; } = "Varsayılan Kullanıcı";
    public List<Bilet> Biletlerim { get; } = new();
    public double CuzdanBakiye { get; private set; } = 500;

    public int ToplamBiletSayisi => Biletlerim.Count;

    public void BiletEkle(Bilet bilet)
    {
        ArgumentNullException.ThrowIfNull(bilet);
        Biletlerim.Add(bilet);
        CuzdanBakiye -= bilet.OdenenFiyat;
    }

    public void BiletIptalEt(Bilet bilet, double iadeTutari)
    {
        ArgumentNullException.ThrowIfNull(bilet);
        if (Biletlerim.Remove(bilet))
        {
            CuzdanBakiye += iadeTutari;
        }
    }

    public void ParaEkle(double miktar)
    {
        if (miktar > 0) CuzdanBakiye += miktar;
    }

    public int KategoridekiBiletSayisi(Kategori kategori) => Biletlerim.Count(b => b.Etkinlik.Kategori == kategori);
}

public class Bilet
{
    public int Id { get; }
    private static int _idSayac = 1;

    public Etkinlik Etkinlik { get; }
    public double OdenenFiyat { get; }
    public DateTime SatinAlmaTarihi { get; } = DateTime.Now;

    public Bilet(Etkinlik etkinlik, double fiyat)
    {
        ArgumentNullException.ThrowIfNull(etkinlik);
        Id = _idSayac++;
        Etkinlik = etkinlik;
        OdenenFiyat = fiyat;
    }

    public override string ToString() => $"Bilet ID: {Id}\nEtkinlik: {Etkinlik.Ad} ({Etkinlik.Kategori})\nTarih: {Etkinlik.Tarih:dd MMMM yyyy HH:mm}\nÖdenen Tutar: {OdenenFiyat:C}\nSatın Alma: {SatinAlmaTarihi:dd.MM.yyyy HH:mm}";
}

public class BiletlemeSistemi
{
    private readonly Kullanici _kullanici;
    public List<Etkinlik> Etkinlikler { get; private set; } = new();

    public BiletlemeSistemi(Kullanici kullanici)
    {
        _kullanici = kullanici ?? throw new ArgumentNullException(nameof(kullanici));
    }

    public void VerileriHazirla()
    {
        Etkinlikler = new List<Etkinlik>
        {
            new() { Ad = "Rock Festivali", Mekan = "Maçka Parkı", Tarih = DateTime.Now.AddDays(20), TabanFiyat = 200, Kategori = Kategori.Konser, Kapasite = 500, SatilanBiletSayisi = 100 },
            new() { Ad = "Tiyatro Oyunu", Mekan = "Devlet Tiyatrosu", Tarih = DateTime.Now.AddDays(10), TabanFiyat = 120, Kategori = Kategori.Tiyatro, Kapasite = 100, SatilanBiletSayisi = 60 },
            new() { Ad = "Futbol Maçı", Mekan = "Şehir Stadyumu", Tarih = DateTime.Now.AddDays(5), TabanFiyat = 150, Kategori = Kategori.Spor, Kapasite = 1000, SatilanBiletSayisi = 850 },
            new() { Ad = "Teknoloji Zirvesi", Mekan = "Kongre Merkezi", Tarih = DateTime.Now.AddDays(4), TabanFiyat = 180, Kategori = Kategori.Konferans, Kapasite = 200, SatilanBiletSayisi = 50 },
            new() { Ad = "Basketbol Maçı", Mekan = "Spor Salonu", Tarih = DateTime.Now.AddHours(48), TabanFiyat = 130, Kategori = Kategori.Spor, Kapasite = 300, SatilanBiletSayisi = 100 },
            new() { Ad = "Acil Stand-up", Mekan = "BKM Mutfak", Tarih = DateTime.Now.AddHours(12), TabanFiyat = 110, Kategori = Kategori.Tiyatro, Kapasite = 50, SatilanBiletSayisi = 10 },
            new() { Ad = "Pop Konseri (TÜKENDİ)", Mekan = "Arena", Tarih = DateTime.Now.AddDays(7), TabanFiyat = 250, Kategori = Kategori.Konser, Kapasite = 50, SatilanBiletSayisi = 50 }
        };
    }

    public Bilet BiletAl(Etkinlik etkinlik)
    {
        ArgumentNullException.ThrowIfNull(etkinlik);

        if (_kullanici.ToplamBiletSayisi >= 8) throw new BiletSistemiException("Toplamda en fazla 8 bilet alabilirsiniz.");

        int kategoriLimit = Etkinlik.GetKategoriLimiti(etkinlik.Kategori);
        if (_kullanici.KategoridekiBiletSayisi(etkinlik.Kategori) >= kategoriLimit) throw new KategoriLimitException(etkinlik.Kategori, kategoriLimit);

        if (etkinlik.SatilanBiletSayisi >= etkinlik.Kapasite) throw new KapasiteDoluException("Bu etkinlik için biletler tükenmiştir.");

        double fiyat = etkinlik.NihaiFiyatiHesapla();
        if (_kullanici.CuzdanBakiye < fiyat) throw new BiletSistemiException($"Cüzdan bakiyesi yetersiz. Gerekli: {fiyat:C}, Mevcut: {_kullanici.CuzdanBakiye:C}");

        Bilet bilet = new(etkinlik, fiyat);
        _kullanici.BiletEkle(bilet);
        etkinlik.SatilanBiletSayisi++;
        return bilet;
    }

    public double BiletIptal(Bilet bilet)
    {
        ArgumentNullException.ThrowIfNull(bilet);

        double iadeOrani = (bilet.Etkinlik.Tarih - DateTime.Now).TotalHours switch
        {
            > 72 => 1.00,
            >= 24 => 0.50,
            _ => throw new IadeMümkünDegilException("Etkinliğe 24 saatten az kaldığı için iade yapılamaz.")
        };

        double iadeTutari = Math.Round(bilet.OdenenFiyat * iadeOrani, 2);
        _kullanici.BiletIptalEt(bilet, iadeTutari);
        bilet.Etkinlik.SatilanBiletSayisi--;
        return iadeTutari;
    }

    public List<Etkinlik> AlternatifOner(Kategori kategori) => Etkinlikler
        .Where(e => e.Kategori != kategori && e.SatilanBiletSayisi < e.Kapasite)
        .ToList();
}

public class BiletSistemiException : Exception { public BiletSistemiException(string mesaj) : base(mesaj) { } }
public class KategoriLimitException : BiletSistemiException
{
    public Kategori Kategori { get; }
    public KategoriLimitException(Kategori kategori, int limit) : base($"'{kategori}' kategorisinde en fazla {limit} bilet alabilirsiniz.") { Kategori = kategori; }
}
public class IadeMümkünDegilException : BiletSistemiException { public IadeMümkünDegilException(string mesaj) : base(mesaj) { } }
public class KapasiteDoluException : BiletSistemiException { public KapasiteDoluException(string mesaj) : base(mesaj) { } }