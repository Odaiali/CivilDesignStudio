using System.Text;
using CivilDesignBeam.Models;

namespace CivilDesignBeam.Services;

public sealed class DxfWriter
{
    private readonly StringBuilder _sb = new();

    public void Begin()
    {
        Pair(0, "SECTION");
        Pair(2, "HEADER");
        Pair(0, "ENDSEC");

        Pair(0, "SECTION");
        Pair(2, "ENTITIES");
    }

    public void End()
    {
        Pair(0, "ENDSEC");
        Pair(0, "EOF");
    }

    public void Text(double x, double y, double height, string text, string layer = "TEXT")
    {
        Pair(0, "TEXT");
        Pair(8, layer);
        Pair(10, x.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture));
        Pair(20, y.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture));
        Pair(40, height.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture));
        Pair(1, text);
    }

    public void Line(double x1, double y1, double x2, double y2, string layer = "BEAM")
    {
        Pair(0, "LINE");
        Pair(8, layer);
        Pair(10, x1.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture));
        Pair(20, y1.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture));
        Pair(11, x2.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture));
        Pair(21, y2.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture));
    }

    public void Rectangle(double x, double y, double w, double h, string layer)
    {
        Line(x, y, x + w, y, layer);
        Line(x + w, y, x + w, y + h, layer);
        Line(x + w, y + h, x, y + h, layer);
        Line(x, y + h, x, y, layer);
    }

    public string GetText() => _sb.ToString();

    private void Pair(int code, string value)
    {
        _sb.Append(code).Append('\n');
        _sb.Append(value).Append('\n');
    }
}

public static class DxfProjectExporter
{
    public static string Export(
        IReadOnlyList<BeamDesignResult> results,
        string projectFolder)
    {
        string path = Path.Combine(projectFolder, "Structural_Design.dxf");
        var dxf = new DxfWriter();
        dxf.Begin();

        double xOffset = 0;
        double yOffset = 0;

        foreach (var r in results)
        {
            DrawBeamSheet(dxf, r, xOffset, yOffset);
            xOffset += 9000;
            if (xOffset > 27000)
            {
                xOffset = 0;
                yOffset -= 7000;
            }
        }

        dxf.End();
        File.WriteAllText(path, dxf.GetText(), Encoding.ASCII);

        string beamFolder = Path.Combine(projectFolder, "Beams");
        Directory.CreateDirectory(beamFolder);

        foreach (var r in results)
        {
            var one = new DxfWriter();
            one.Begin();
            DrawBeamSheet(one, r, 0, 0);
            one.End();
            File.WriteAllText(
                Path.Combine(beamFolder, $"{r.BeamName}.dxf"),
                one.GetText(),
                Encoding.ASCII);
        }

        return path;
    }

    private static void DrawBeamSheet(
        DxfWriter dxf,
        BeamDesignResult r,
        double ox,
        double oy)
    {
        dxf.Text(ox, oy, 250, $"BEAM {r.BeamName}", "TITLE");
        dxf.Text(ox, oy - 400, 120,
            $"ACI 318-19 | b={r.BeamWidth:0} h={r.BeamHeight:0} L={r.BeamLength:0} mm",
            "DESIGN_INFO");

        double x = ox;
        double y = oy - 1200;
        double scale = Math.Min(1.0, 6500.0 / Math.Max(r.BeamLength, 1));
        double w = r.BeamLength * scale;
        double h = r.BeamHeight * scale;

        dxf.Rectangle(x, y, w, h, "BEAM");
        dxf.Line(x + 150, y + 100, x + w - 150, y + 100, "REBAR_BOTTOM");
        dxf.Line(x + 150, y + h - 100, x + w - 150, y + h - 100, "REBAR_TOP");

        for (double pos = 0; pos <= r.BeamLength; pos += r.StirrupSpacing)
        {
            double sx = x + pos * scale;
            dxf.Line(sx, y, sx, y + h, "STIRRUP");
        }

        double sx = ox;
        double sy = oy - 2600;

        dxf.Text(sx, sy, 160, "SECTION", "TITLE");
        dxf.Rectangle(sx, sy - 800, r.BeamWidth, r.BeamHeight, "BEAM");

        dxf.Text(sx, sy - 1100, 110,
            $"BOTTOM: {r.NumberOfBars}Ø{r.MainBarDiameter:0}",
            "DESIGN_INFO");
        dxf.Text(sx, sy - 1300, 110,
            $"STIRRUP: {r.StirrupLegs}Ø{r.StirrupDiameter:0} @ {r.StirrupSpacing:0}",
            "DESIGN_INFO");

        double tx = ox + 3500;
        dxf.Text(tx, sy, 160, "DESIGN CHECKS", "TITLE");
        dxf.Text(tx, sy - 300, 110, $"Mu = {r.Mu:0.00} kN.m", "DESIGN_INFO");
        dxf.Text(tx, sy - 500, 110, $"Vu = {r.Vu:0.00} kN", "DESIGN_INFO");
        dxf.Text(tx, sy - 700, 110, $"As req = {r.RequiredAs:0} mm2", "DESIGN_INFO");
        dxf.Text(tx, sy - 900, 110, $"As prov = {r.ProvidedAs:0} mm2", "DESIGN_INFO");
        dxf.Text(tx, sy - 1100, 110, $"Flexure = {(r.FlexureOK ? "PASS" : "FAIL")}", "DESIGN_INFO");
        dxf.Text(tx, sy - 1300, 110, $"Shear = {(r.ShearOK ? "PASS" : "FAIL")}", "DESIGN_INFO");
        dxf.Text(tx, sy - 1500, 110, $"Deflection = {(r.DeflectionOK ? "PASS" : "FAIL")}", "DESIGN_INFO");
        dxf.Text(tx, sy - 1700, 110, $"Overall = {(r.IsPassed ? "PASS" : "FAIL")}", "DESIGN_INFO");
    }
}
