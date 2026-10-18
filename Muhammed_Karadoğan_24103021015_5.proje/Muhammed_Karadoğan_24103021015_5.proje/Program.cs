using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

public class Program
{
    private static Kullanici _anaKullanici = null!;
    private static BiletlemeSistemi _sistem = null!;

    public static void Main(string[] args)
    {
        AyarlaKultur();
        BaslatSistem();
        AnaMenuDongusu();
    }

    private static void AyarlaKultur()
    {
        CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US");
        CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-US");
    }

    private static void BaslatSistem()
    {
        _anaKullanici = new Kullanici { Ad = "Demo Kullanıcı" };
        _sistem = new BiletlemeSistemi(_anaKullanici);
        _sistem.VerileriHazirla();
    }

    private static void AnaMenuDongusu()
    {
        string anaMenuSecim;
        do
        {
            Console.Clear();
            AnaMenuyuGoster();
            anaMenuSecim = Console.ReadLine() ?? string.Empty;
            Console.Clear();

            try { IslemiGerceklestir(anaMenuSecim); }
            catch (Exception ex) { HataGoster(ex); }

        } while (anaMenuSecim != "0");
    }

    private static void AnaMenuyuGoster()
    {
        BaslikYazdir("ETKİNLİK & BİLETLEME SİSTEMİ ANA MENÜ");
        Console.WriteLine("1: Etkinlikleri Göster ve Bilet Al\n2: Biletlerimi Göster ve İptal Et\n3: Kullanıcı Bilgilerim\n4: Cüzdana Para Ekle\n0: Çıkış");
        CizgiCiz();
        SecimIste();
    }

    private static void IslemiGerceklestir(string secim)
    {
        bool anaMenuyeDon = false;
        switch (secim)
        {
            case "1": while (!anaMenuyeDon) { anaMenuyeDon = BiletAlAnaMenu(); if (!anaMenuyeDon) Console.Clear(); } break;
            case "2": while (!anaMenuyeDon) { anaMenuyeDon = BiletIptalAnaMenu(); if (!anaMenuyeDon) Console.Clear(); } break;
            case "3": KullaniciBilgileriMenu(); GeriDonusIcinBekle(); break;
            case "4": ParaEkleMenu(); GeriDonusIcinBekle(); break;
            case "0": Console.WriteLine("\nSistemden çıkılıyor..."); break;
            default: GecersizSecimUyarisi(); break;
        }
    }

    private static bool BiletAlAnaMenu()
    {
        BaslikYazdir("Etkinlikler");
        if (!_sistem.Etkinlikler.Any()) return GosterVeDon("Gösterilecek etkinlik bulunamadı.");

        ListeleEtkinlikler();
        return AltMenuGoster("Bilet Al", BiletAlmaIslemi);
    }

    private static void BiletAlmaIslemi()
    {
        BaslikYazdir("Bilet Satın Alma");
        ListeleEtkinlikler();
        int etkinlikSecim = ReadInt($"\n>> Bilet almak için etkinlik no (1-{_sistem.Etkinlikler.Count}), Geri Dönmek için 0: ", 0, _sistem.Etkinlikler.Count);
        if (etkinlikSecim == 0) return;

        Etkinlik secilenEtkinlik = _sistem.Etkinlikler[etkinlikSecim - 1];
        try
        {
            Bilet alinanBilet = _sistem.BiletAl(secilenEtkinlik);
            MesajGoster($"'{alinanBilet.Etkinlik.Ad}' için bilet alındı.\nÖdenen Tutar: {alinanBilet.OdenenFiyat:C} (ID: {alinanBilet.Id})", ConsoleColor.Green);
        }
        catch (KategoriLimitException ex)
        {
            MesajGoster(ex.Message, ConsoleColor.Yellow);
            AlternatifleriGoster(ex.Kategori);
        }
        catch (BiletSistemiException ex) { MesajGoster(ex.Message, ConsoleColor.Red); }
        GeriDonusIcinBekle();
    }

    private static bool BiletIptalAnaMenu()
    {
        BaslikYazdir("Biletlerim");
        if (_anaKullanici.ToplamBiletSayisi == 0) return GosterVeDon("Hiç biletiniz bulunmamaktadır.");

        ListeleBiletler();
        return AltMenuGoster("Seçili Bileti İptal Et", BiletIptalIslemi);
    }

    private static void BiletIptalIslemi()
    {
        BaslikYazdir("Bilet İptal Etme");
        ListeleBiletler();
        int biletSecim = ReadInt($"\n>> İptal etmek için bilet no (1-{_anaKullanici.Biletlerim.Count}), Geri Dönmek için 0: ", 0, _anaKullanici.Biletlerim.Count);
        if (biletSecim == 0) return;

        if (biletSecim - 1 >= _anaKullanici.Biletlerim.Count)
        {
            MesajGoster("Geçersiz bilet numarası seçildi.", ConsoleColor.Red); GeriDonusIcinBekle(); return;
        }
        Bilet secilenBilet = _anaKullanici.Biletlerim[biletSecim - 1];

        Console.Write($"'{secilenBilet.Etkinlik.Ad}' biletini iptal etmek istediğinizden emin misiniz? (E/H): ");
        if ((Console.ReadLine()?.ToUpper() ?? "H") == "E")
        {
            try
            {
                double iadeTutari = _sistem.BiletIptal(secilenBilet);
                MesajGoster($"Bilet iptal edildi. Cüzdana eklenen iade tutarı: {iadeTutari:C}", ConsoleColor.Green);
            }
            catch (IadeMümkünDegilException ex) { MesajGoster(ex.Message, ConsoleColor.Yellow); }
            catch (BiletSistemiException ex) { MesajGoster(ex.Message, ConsoleColor.Red); }
        }
        else { Console.WriteLine("\nİptal işlemi durduruldu."); }
        GeriDonusIcinBekle();
    }

    private static void KullaniciBilgileriMenu()
    {
        BaslikYazdir("Kullanıcı Bilgileri");
        Console.WriteLine($"Kullanıcı: {_anaKullanici.Ad}\nCüzdan Bakiyesi: {_anaKullanici.CuzdanBakiye:C}\nToplam Bilet Limiti: {_anaKullanici.ToplamBiletSayisi} / 8\n\n--- Kategori Limitleri ---");
        foreach (Kategori kat in Enum.GetValues(typeof(Kategori)))
        {
            Console.WriteLine($"{kat,-10}: {_anaKullanici.KategoridekiBiletSayisi(kat)} / {Etkinlik.GetKategoriLimiti(kat)}"); // Hizalama eklendi
        }
    }

    private static void ParaEkleMenu()
    {
        BaslikYazdir("Cüzdana Para Ekle");
        Console.WriteLine($"Mevcut Bakiye: {_anaKullanici.CuzdanBakiye:C}");
        double eklenecekMiktar = ReadDouble(">> Eklemek istediğiniz tutarı girin (Örn: 50.75): ");

        if (eklenecekMiktar > 0)
        {
            _anaKullanici.ParaEkle(eklenecekMiktar);
            MesajGoster($"{eklenecekMiktar:C} cüzdanınıza eklendi.\nYeni Bakiye: {_anaKullanici.CuzdanBakiye:C}", ConsoleColor.Green);
        }
        else { MesajGoster("Geçersiz veya sıfır miktar girdiniz. İşlem yapılmadı.", ConsoleColor.Yellow); }
    }

    private static void ListeleEtkinlikler() => Listele(_sistem.Etkinlikler);
    private static void ListeleBiletler() => Listele(_anaKullanici.Biletlerim);

    private static void Listele<T>(List<T> liste) where T : class // Sınıf kısıtlaması
    {
        for (int i = 0; i < liste.Count; i++)
        {
            Console.WriteLine($"----- {i + 1} -----");
            Console.WriteLine(liste[i]?.ToString() ?? "N/A");
            Console.WriteLine();
        }
    }

    private static bool AltMenuGoster(string islemAdi, Action islemMetodu)
    {
        Console.WriteLine($"\n--- Seçenekler ---\n1: {islemAdi}\n0: Ana Menüye Dön");
        SecimIste();
        string altMenuSecim = Console.ReadLine() ?? "0";
        Console.Clear();

        if (altMenuSecim == "1") { islemMetodu(); return false; }
        if (altMenuSecim == "0") return true;

        GecersizSecimUyarisi(); return false;
    }

    private static void AlternatifleriGoster(Kategori hataliKategori)
    {
        var alternatifler = _sistem.AlternatifOner(hataliKategori).Take(3);
        if (alternatifler.Any())
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n--- Diğer Kategorilerde Bilet Almayı Deneyebilirsiniz ---");
            foreach (var e in alternatifler) { Console.WriteLine($"- {e.Ad} ({e.Kategori}) - Fiyat: {e.NihaiFiyatiHesapla():C}"); }
            Console.ResetColor();
        }
    }

    private static int ReadInt(string prompt, int min, int max)
    {
        while (true)
        {
            Console.Write(prompt);
            if (int.TryParse(Console.ReadLine(), out int value) && value >= min && value <= max) return value;
            UyariGoster($"Lütfen {min} ile {max} arasında geçerli bir sayı girin.");
        }
    }

    private static double ReadDouble(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string? input = Console.ReadLine()?.Replace(',', '.');
            if (double.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out double value) && value >= 0) return value;
            UyariGoster("Lütfen geçerli, pozitif bir ondalıklı sayı girin (örn: 100 veya 50.5).");
        }
    }

    private static void BaslikYazdir(string baslik)
    {
        CizgiCiz();
        int padding = Math.Max(0, (75 - baslik.Length) / 2);
        Console.WriteLine($"{new string(' ', padding)}{baslik}");
        CizgiCiz();
    }

    private static void GeriDonusIcinBekle()
    {
        Console.WriteLine("\nDevam etmek için bir tuşa basın...");
        Console.ReadKey();
    }

    private static void GecersizSecimUyarisi()
    {
        UyariGoster("Geçersiz seçim. Lütfen menüdeki sayılardan birini girin.");
        GeriDonusIcinBekle();
    }

    private static bool GosterVeDon(string mesaj)
    {
        Console.WriteLine(mesaj); GeriDonusIcinBekle(); return true;
    }

    private static void UyariGoster(string mesaj) => MesajGoster($"[UYARI] {mesaj}", ConsoleColor.Yellow);
    private static void HataGoster(Exception ex) => MesajGoster($"[HATA OLUŞTU] {ex.GetType().Name}: {ex.Message}", ConsoleColor.Red); 
    private static void MesajGoster(string mesaj, ConsoleColor renk)
    {
        Console.ForegroundColor = renk; Console.WriteLine($"\n{mesaj}"); Console.ResetColor();
    }

    private static void CizgiCiz() => Console.WriteLine(new string('=', 75));
    private static void SecimIste() => Console.Write(">> Seçiminizi yapın: ");
}