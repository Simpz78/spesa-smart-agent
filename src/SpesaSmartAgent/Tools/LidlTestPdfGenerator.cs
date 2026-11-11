using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.Writer;
using UglyToad.PdfPig.Fonts.Standard14Fonts;

namespace SpesaSmartAgent.Tools;

public static class LidlTestPdfGenerator
{
    public static void CreateSampleFlyer(string outputPath)
    {
        var builder = new PdfDocumentBuilder();
        var page = builder.AddPage(PageSize.A4);
        var font = builder.AddStandard14Font(Standard14Font.Helvetica);
        var boldFont = builder.AddStandard14Font(Standard14Font.HelveticaBold);

        page.AddText("LIDL BOLOGNA - OFFERTE SETTIMANALI SUPER WEEKEND", 16, new PdfPoint(50, 800), boldFont);
        page.AddText("Valide da Lunedi a Domenica", 12, new PdfPoint(50, 780), font);

        page.AddText("OFFERTE IN EVIDENZA", 14, new PdfPoint(50, 740), boldFont);
        // Tonno Nixe a prezzo più basso di Esselunga
        page.AddText("NIXE TONNO ALL OLIO DI OLIVA 4x80g - EUR 2,69", 11, new PdfPoint(50, 715), font);
        // Passata Freshona
        page.AddText("FRESHONA PASSATA DI POMODORO 700g - SCONTO 20% - EUR 0,69", 11, new PdfPoint(50, 695), font);
        // Petto di pollo
        page.AddText("DULANO PETTO DI POLLO AL FORNO 140g - SCONTO 20% - EUR 1,59", 11, new PdfPoint(50, 675), font);
        // Arrosto di tacchino a fette
        page.AddText("DULANO ARROSTO DI TACCHINO A FETTE 140g - SCONTO 25% - EUR 1,49", 11, new PdfPoint(50, 660), font);
        // Carta da cucina
        page.AddText("FLORALYS CARTA DA CUCINA ASCIUGATUTTO 2 MAXI ROTOLI - EUR 2,19", 11, new PdfPoint(50, 640), font);

        var fileBytes = builder.Build();
        File.WriteAllBytes(outputPath, fileBytes);
    }
}
