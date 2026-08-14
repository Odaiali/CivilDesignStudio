using CivilDesignBeam.Models;

namespace CivilDesignBeam.Services;

public static class ProjectExporter
{
    public static async Task<(string Folder, string Pdf, string Dxf, string Excel)>
        ExportAll(
            IReadOnlyList<BeamDesignResult> results,
            IProgress<DesignProgress>? progress = null)
    {
        string folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "CivilDesign",
            "Projects",
            DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));

        Directory.CreateDirectory(folder);

        progress?.Report(new DesignProgress
        {
            Percentage = 88,
            Current = results.Count,
            Total = results.Count,
            Stage = "Generating PDF"
        });

        string pdf = await Task.Run(() =>
            PdfReportGenerator.Generate(results, folder));

        progress?.Report(new DesignProgress
        {
            Percentage = 94,
            Current = results.Count,
            Total = results.Count,
            Stage = "Generating DXF"
        });

        string dxf = await Task.Run(() =>
            DxfProjectExporter.Export(results, folder));

        progress?.Report(new DesignProgress
        {
            Percentage = 98,
            Current = results.Count,
            Total = results.Count,
            Stage = "Generating Excel Results"
        });

        string excel = await Task.Run(() =>
            ExcelResultsExporter.Export(results, folder));

        return (folder, pdf, dxf, excel);
    }
}
