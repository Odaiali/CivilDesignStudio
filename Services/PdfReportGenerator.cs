using CivilDesignBeam.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CivilDesignBeam.Services;

public static class PdfReportGenerator
{
    public static string Generate(
        IReadOnlyList<BeamDesignResult> results,
        string projectFolder)
    {
        string path = Path.Combine(projectFolder, "Design_Report.pdf");

        Document.Create(doc =>
        {
            doc.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(25);
                page.Header().Text("CIVIL DESIGN SOFTWARE | ACI 318-19")
                    .Bold().FontSize(16);

                page.Content().Column(col =>
                {
                    col.Item().Text("PROJECT SUMMARY").Bold().FontSize(15);
                    col.Item().Text(
                        $"Total Beams: {results.Count} | " +
                        $"PASS: {results.Count(x => x.IsPassed)} | " +
                        $"FAIL: {results.Count(x => !x.IsPassed)}");

                    foreach (var r in results)
                    {
                        col.Item().PageBreak();
                        AddBeam(col, r);
                    }
                });

                page.Footer().AlignCenter().Text(t =>
                {
                    t.Span("Page ");
                    t.CurrentPageNumber();
                    t.Span(" / ");
                    t.TotalPages();
                });
            });
        }).GeneratePdf(path);

        return path;
    }

    private static void AddBeam(ColumnDescriptor col, BeamDesignResult r)
    {
        col.Item().Text($"BEAM {r.BeamName}").Bold().FontSize(18);

        AddTable(col, new[]
        {
            ("Width", $"{r.BeamWidth:0} mm"),
            ("Height", $"{r.BeamHeight:0} mm"),
            ("Length", $"{r.BeamLength:0} mm"),
            ("fc", $"{r.Fc:0} MPa"),
            ("fy", $"{r.Fy:0} MPa"),
            ("Mu", $"{r.Mu:0.00} kN.m"),
            ("Vu", $"{r.Vu:0.00} kN"),
            ("As Required", $"{r.RequiredAs:0} mm²"),
            ("As Minimum", $"{r.MinimumAs:0} mm²"),
            ("As Provided", $"{r.ProvidedAs:0} mm²"),
            ("Bottom", $"{r.NumberOfBars}Ø{r.MainBarDiameter:0}"),
            ("Stirrups", $"{r.StirrupLegs}Ø{r.StirrupDiameter:0} @ {r.StirrupSpacing:0}"),
            ("Development Length", $"{r.DevelopmentLength:0} mm"),
            ("Immediate Deflection", $"{r.ImmediateDeflection:0.00} mm"),
            ("Allowable Deflection", $"{r.AllowableDeflection:0.00} mm")
        });

        col.Item().Text("DESIGN CHECKS").Bold().FontSize(14);

        AddTable(col, new[]
        {
            ("Flexure", r.FlexureOK ? "PASS" : "FAIL"),
            ("Shear", r.ShearOK ? "PASS" : "FAIL"),
            ("Minimum Steel", r.MinimumSteelOK ? "PASS" : "FAIL"),
            ("Development", r.DevelopmentOK ? "PASS" : "FAIL"),
            ("Deflection", r.DeflectionOK ? "PASS" : "FAIL"),
            ("Bar Fit", r.BarFitOK ? "PASS" : "FAIL"),
            ("OVERALL", r.IsPassed ? "PASS" : "FAIL")
        });

        if (r.Failures.Count > 0)
        {
            col.Item().Text("FAILURE REASONS").Bold().FontSize(14);
            foreach (var f in r.Failures)
            {
                col.Item().Text(
                    $"{f.CheckName}: {f.Reason}\n" +
                    $"Current: {f.CurrentValue} | Required: {f.RequiredValue}\n" +
                    $"Suggested: {f.SuggestedAction}");
            }
        }

        col.Item().Text("REINFORCEMENT SCHEDULE").Bold().FontSize(14);

        foreach (var item in r.ReinforcementItems)
        {
            col.Item().Text(
                $"{item.Mark} | {item.Location} | " +
                $"Ø{item.Diameter:0} | Qty {item.Quantity} | " +
                $"Spacing {item.Spacing:0} | " +
                $"Length {item.Length:0.00} m");
        }
    }

    private static void AddTable(
        ColumnDescriptor col,
        IEnumerable<(string A, string B)> rows)
    {
        col.Item().Table(table =>
        {
            table.ColumnsDefinition(c =>
            {
                c.RelativeColumn();
                c.RelativeColumn();
            });

            foreach (var row in rows)
            {
                table.Cell().Border(1).Padding(4).Text(row.A);
                table.Cell().Border(1).Padding(4).Text(row.B);
            }
        });
    }
}
