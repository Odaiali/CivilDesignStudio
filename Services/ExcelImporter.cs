using ClosedXML.Excel;
using CivilDesignBeam.Models;

namespace CivilDesignBeam.Services;

public static class ExcelImporter
{
    public static List<Beam> Import(string path)
    {
        using var workbook = new XLWorkbook(path);
        var ws = workbook.Worksheets.First();

        int lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;
        var result = new List<Beam>();

        // Expected columns:
        // A Beam, B b, C h, D L, E fc, F fy, G Cover, H DL, I LL
        for (int row = 2; row <= lastRow; row++)
        {
            string name = ws.Cell(row, 1).GetString().Trim();
            if (string.IsNullOrWhiteSpace(name))
                continue;

            result.Add(new Beam
            {
                Name = name,
                Width = ws.Cell(row, 2).GetDouble(),
                Height = ws.Cell(row, 3).GetDouble(),
                Length = ws.Cell(row, 4).GetDouble() * 1000.0, // Excel L in m
                Fc = ws.Cell(row, 5).GetDouble(),
                Fy = ws.Cell(row, 6).GetDouble(),
                Cover = ws.Cell(row, 7).GetDouble(),
                DeadLoad = ws.Cell(row, 8).GetDouble(),
                LiveLoad = ws.Cell(row, 9).GetDouble()
            });
        }

        return result;
    }

    public static void CreateTemplate(string path)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Beams");

        string[] headers =
        {
            "Beam","b (mm)","h (mm)","L (m)",
            "fc (MPa)","fy (MPa)","Cover (mm)",
            "Dead Load (kN/m)","Live Load (kN/m)"
        };

        for (int i = 0; i < headers.Length; i++)
            ws.Cell(1, i + 1).Value = headers[i];

        ws.Cell(2, 1).Value = "B01";
        ws.Cell(2, 2).Value = 300;
        ws.Cell(2, 3).Value = 500;
        ws.Cell(2, 4).Value = 5;
        ws.Cell(2, 5).Value = 25;
        ws.Cell(2, 6).Value = 420;
        ws.Cell(2, 7).Value = 40;
        ws.Cell(2, 8).Value = 20;
        ws.Cell(2, 9).Value = 10;

        ws.Columns().AdjustToContents();
        wb.SaveAs(path);
    }
}
