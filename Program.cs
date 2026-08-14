using QuestPDF.Infrastructure;

namespace CivilDesignBeam;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        // QuestPDF is used only for report generation.
        QuestPDF.Settings.License = LicenseType.Community;

        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }
}
