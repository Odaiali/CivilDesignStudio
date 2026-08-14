using ClosedXML.Excel;
using CivilDesignBeam.Models;

namespace CivilDesignBeam.Services;

public static class ExcelResultsExporter
{
    public static string Export(
        IReadOnlyList<BeamDesignResult> results,
        string projectFolder)
    {
        string path = Path.Combine(projectFolder, "Design_Results.xlsx");

        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Beam Results");

        string[] headers =
        {
            "Beam","b","h","L","fc","fy","Mu","Vu",
            "As Req","As Min","As Prov","Bottom",
            "Stirrups","Spacing","Ld","Deflection",
            "Flexure","Shear","Development","Deflection Check",
            "Bar Fit","Overall"
        };

        for (int i = 0; i < headers.Length; i++)
            ws.Cell(1, i + 1).Value = headers[i];

        int row = 2;
        foreach (var r in results)
        {
            ws.Cell(row, 1).Value = r.BeamName;
            ws.Cell(row, 2).Value = r.BeamWidth;
            ws.Cell(row, 3).Value = r.BeamHeight;
            ws.Cell(row, 4).Value = r.BeamLength;
            ws.Cell(row, 5).Value = r.Fc;
            ws.Cell(row, 6).Value = r.Fy;
            ws.Cell(row, 7).Value = r.Mu;
            ws.Cell(row, 8).Value = r.Vu;
            ws.Cell(row, 9).Value = r.RequiredAs;
            ws.Cell(row, 10).Value = r.MinimumAs;
            ws.Cell(row, 11).Value = r.ProvidedAs;
            ws.Cell(row, 12).Value = $"{r.NumberOfBars}Ø{r.MainBarDiameter:0}";
            ws.Cell(row, 13).Value = $"{r.StirrupLegs}Ø{r.StirrupDiameter:0}";
            ws.Cell(row, 14).Value = r.StirrupSpacing;
            ws.Cell(row, 15).Value = r.DevelopmentLength;
            ws.Cell(row, 16).Value = r.ImmediateDeflection;
            ws.Cell(row, 17).Value = r.FlexureOK ? "PASS" : "FAIL";
            ws.Cell(row, 18).Value = r.ShearOK ? "PASS" : "FAIL";
            ws.Cell(row, 19).Value = r.DevelopmentOK ? "PASS" : "FAIL";
            ws.Cell(row, 20).Value = r.DeflectionOK ? "PASS" : "FAIL";
            ws.Cell(row, 21).Value = r.BarFitOK ? "PASS" : "FAIL";
            ws.Cell(row, 22).Value = r.IsPassed ? "PASS" : "FAIL";
            row++;
        }

        ws.Row(1).Style.Font.Bold = true;
        ws.Columns().AdjustToContents();
        wb.SaveAs(path);
        return path;
    }
}
