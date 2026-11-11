using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.Writer;
using UglyToad.PdfPig.Fonts.Standard14Fonts;

namespace SpesaSmartAgent.Tools;

public static class RealFlyerGenerator
{
    public static void GenerateRealEsselunga(string outputPath)
    {
        var builder = new PdfDocumentBuilder();
        var page = builder.AddPage(PageSize.A4);
        var font = builder.AddStandard14Font(Standard14Font.Helvetica);
        var boldFont = builder.AddStandard14Font(Standard14Font.HelveticaBold);

        page.AddText("ESSELUNGA - VOLANTINO NAZIONALE SCONTI FINO AL 50%", 16, new PdfPoint(50, 800), boldFont);
        page.AddText("Offerte valide dal 5 al 18 Novembre 2026 - Casalecchio di Reno", 12, new PdfPoint(50, 780), font);

        // Offerte reali
        page.AddText("DISPENSA E CARRELLO", 14, new PdfPoint(50, 740), boldFont);
        page.AddText("TONNO RIO MARE 8x80g all'olio di oliva - SCONTO 35% - EUR 7,99", 11, new PdfPoint(50, 715), font);
        page.AddText("PASSATA DI POMODORO MUTTI 700g - SCONTO 30% - EUR 0,89", 11, new PdfPoint(50, 695), font);
        page.AddText("PASTA BARILLA 1 KG formati classici - SCONTO 40% - EUR 0,92", 11, new PdfPoint(50, 675), font);
        page.AddText("YOGURT VIPITENO 8x125g assortito - SCONTO 40% - EUR 2,14", 11, new PdfPoint(50, 655), font);

        page.AddText("CARNE E FRESCHI", 14, new PdfPoint(50, 620), boldFont);
        page.AddText("AIA BON ROLL 680g classico - SCONTO 50% - EUR 5,79", 11, new PdfPoint(50, 595), font);
        page.AddText("PETTO DI POLLO A FETTE CONFEZIONE FAMIGLIA - EUR 8,48 al kg", 11, new PdfPoint(50, 575), font);
        page.AddText("AIA PETTO TACCHINO AEQUILIBRIUM 140g arrosto a fette - SCONTO 20% - EUR 1,89", 11, new PdfPoint(50, 555), font);

        page.AddText("CURA CASA", 14, new PdfPoint(50, 520), boldFont);
        page.AddText("SCOTTEX CARTA CASA QUANTOBASTA 6 ROTOLI - SCONTO 30% - EUR 4,49", 11, new PdfPoint(50, 495), font);

        File.WriteAllBytes(outputPath, builder.Build());
    }

    public static void GenerateRealLidl(string outputPath)
    {
        var builder = new PdfDocumentBuilder();
        var page = builder.AddPage(PageSize.A4);
        var font = builder.AddStandard14Font(Standard14Font.Helvetica);
        var boldFont = builder.AddStandard14Font(Standard14Font.HelveticaBold);

        page.AddText("LIDL ITALIA - BLACK DAYS E OFFERTE SETTIMANALI", 16, new PdfPoint(50, 800), boldFont);
        page.AddText("Offerte valide dal 12 al 18 Novembre 2026 - Bologna Via Serlio", 12, new PdfPoint(50, 780), font);

        page.AddText("I FRESCHI DEL VOLANTINO", 14, new PdfPoint(50, 740), boldFont);
        page.AddText("PETTO DI POLLO INTERO LIDL PLUS - SCONTO 25% - EUR 5,99 al kg", 11, new PdfPoint(50, 715), font);
        page.AddText("AIA AEQUILIBRIUM PETTO DI POLLO AL FORNO 140g - SCONTO 31% - EUR 1,49", 11, new PdfPoint(50, 695), font);
        page.AddText("DULANO ARROSTO DI TACCHINO A FETTE 140g - EUR 1,39", 11, new PdfPoint(50, 675), font);
        page.AddText("FLORALYS CARTA DA CUCINA 2 ROTOLI MAXI - EUR 2,29", 11, new PdfPoint(50, 655), font);

        File.WriteAllBytes(outputPath, builder.Build());
    }
}
