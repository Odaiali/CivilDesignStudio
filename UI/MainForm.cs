using CivilDesignBeam.Models;
using CivilDesignBeam.Services;

namespace CivilDesignBeam;

public sealed class MainForm : Form
{
    private readonly Button _import = new()
    {
        Text = "Import Excel",
        AutoSize = true
    };

    private readonly Button _template = new()
    {
        Text = "Create Excel Template",
        AutoSize = true
    };

    private readonly Label _title = new()
    {
        Text = "CIVIL STRUCTURAL DESIGN SOFTWARE",
        Dock = DockStyle.Top,
        Height = 55,
        TextAlign = ContentAlignment.MiddleCenter,
        Font = new Font("Segoe UI", 18, FontStyle.Bold)
    };

    public MainForm()
    {
        Text = "Civil Design Software";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(1000, 650);
        Size = new Size(1250, 800);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(20)
        };

        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 55));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true
        };

        buttons.Controls.Add(_import);
        buttons.Controls.Add(_template);

        layout.Controls.Add(_title, 0, 0);
        layout.Controls.Add(buttons, 0, 1);

        var info = new Label
        {
            Dock = DockStyle.Fill,
            Text =
                "Beam module ready.\r\n\r\n" +
                "Workflow:\r\n" +
                "Excel → Preview → Design All → Failure Review → " +
                "Design Failed Only → PDF + DXF + Excel Results.\r\n\r\n" +
                "After stabilizing the Beam module, the next module is Column Designer.",
            Font = new Font("Segoe UI", 12),
            Padding = new Padding(20)
        };

        layout.Controls.Add(info, 0, 2);
        Controls.Add(layout);

        _import.Click += Import_Click;
        _template.Click += Template_Click;
    }

    private void Template_Click(object? sender, EventArgs e)
    {
        using var dialog = new SaveFileDialog
        {
            Filter = "Excel Files (*.xlsx)|*.xlsx",
            FileName = "Beam_Input_Template.xlsx"
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
            return;

        ExcelImporter.CreateTemplate(dialog.FileName);
        MessageBox.Show(
            this,
            $"Template saved:\r\n{dialog.FileName}",
            "Done",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    private void Import_Click(object? sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "Excel Files (*.xlsx)|*.xlsx",
            Title = "Select Beam Excel Input"
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
            return;

        try
        {
            var beams = ExcelImporter.Import(dialog.FileName);

            if (beams.Count == 0)
            {
                MessageBox.Show(
                    this,
                    "No Beam rows were found.",
                    "Excel",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            using var preview =
                new ExcelPreviewForm(beams);

            preview.ShowDialog(this);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                this,
                ex.Message,
                "Excel Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}
