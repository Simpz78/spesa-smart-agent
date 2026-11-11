using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.Writer;
using UglyToad.PdfPig.Fonts.Standard14Fonts;

namespace SpesaSmartAgent.Tools;

public static class TestPdfGenerator
{
    public static void CreateSampleFlyer(string outputPath)
    {
        var builder = new PdfDocumentBuilder();
        var page = builder.AddPage(PageSize.A4);
        var font = builder.AddStandard14Font(Standard14Font.Helvetica);
        var boldFont = builder.AddStandard14Font(Standard14Font.HelveticaBold);

        // Intestazione
        page.AddText("ESSELUNGA - OFFERTE DELLA SETTIMANA CASALECCHIO", 16, new PdfPoint(50, 800), boldFont);
        page.AddText("Valide fino al 18 Novembre 2026", 12, new PdfPoint(50, 780), font);

        // Offerte
        page.AddText("REPARTO MACELLERIA E SURGELATI", 14, new PdfPoint(50, 740), boldFont);
        page.AddText("AIA PETTO DI POLLO Aequilibrium al forno 140g - SCONTO FIDATY 30% - EUR 1,78", 11, new PdfPoint(50, 715), font);
        page.AddText("AIA PETTO TACCHINO Aequilibrium 140g - SCONTO FIDATY 30% - EUR 1,75", 11, new PdfPoint(50, 695), font);
        page.AddText("SMART WURSTEL 1 KG pacco scorta - EUR 2,52", 11, new PdfPoint(50, 675), font);

        // Dispensa
        page.AddText("DISPENSA E COLAZIONE", 14, new PdfPoint(50, 635), boldFont);
        page.AddText("ESSELUNGA TONNO 70x12 lattine formato scorta - SCONTO 30% - EUR 8,74", 11, new PdfPoint(50, 610), font);
        page.AddText("SLUNGA FROLLINO PANNA 700g - EUR 1,52", 11, new PdfPoint(50, 590), font);
        page.AddText("PASTA INTEGRALE MOLISANA 500g - SCONTO 20% - EUR 1,15", 11, new PdfPoint(50, 570), font);
        page.AddText("MUTTI PASSATA DI POMODORO 700g - SCONTO 25% - EUR 1,19", 11, new PdfPoint(50, 550), font);

        // Latticini
        page.AddText("LATTICINI", 14, new PdfPoint(50, 530), boldFont);
        page.AddText("MILK KEFIR BIANCO 480G - EUR 1,29", 11, new PdfPoint(50, 505), font);
        page.AddText("YOGURT MAGRO VIPITENO 0,1% - EUR 1,15", 11, new PdfPoint(50, 485), font);
        page.AddText("UOVA FRESCHE ALLEVATE A TERRA X6 - EUR 1,49", 11, new PdfPoint(50, 465), font);

        // Cura casa
        page.AddText("CURA CASA E PERSONA", 14, new PdfPoint(50, 425), boldFont);
        page.AddText("SCOTTEX CARTA CASA QUANTOBASTA - PROMOZIONE 2x1 - SCONTO 50% - EUR 5,79", 11, new PdfPoint(50, 400), font);
        page.AddText("CARTA IGIENICA 16 ROTOLI ESS. - EUR 5,99", 11, new PdfPoint(50, 380), font);
        page.AddText("SMART AMMONIACA 2LT - EUR 0,89", 11, new PdfPoint(50, 360), font);
        page.AddText("AIR WICK DEODORANTE RICARICA - SCONTO 40% - EUR 2,99", 11, new PdfPoint(50, 340), font);

        var fileBytes = builder.Build();
        File.WriteAllBytes(outputPath, fileBytes);
    }
}
