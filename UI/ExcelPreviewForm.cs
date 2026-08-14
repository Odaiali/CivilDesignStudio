using CivilDesignBeam.Models;
using CivilDesignBeam.Services;

namespace CivilDesignBeam;

public sealed class ExcelPreviewForm : Form
{
    private readonly List<Beam> _beams;

    private readonly DataGridView _grid = new()
    {
        Dock = DockStyle.Fill,
        ReadOnly = true,
        AutoGenerateColumns = true,
        AllowUserToAddRows = false,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
    };

    private readonly ComboBox _bar = new()
    {
        DropDownStyle = ComboBoxStyle.DropDownList,
        Width = 100
    };

    private readonly ComboBox _stirrup = new()
    {
        DropDownStyle = ComboBoxStyle.DropDownList,
        Width = 100
    };

    private readonly ComboBox _legs = new()
    {
        DropDownStyle = ComboBoxStyle.DropDownList,
        Width = 100
    };

    private readonly Button _start = new()
    {
        Text = "موافق وابدأ التصميم",
        AutoSize = true
    };

    private readonly ProgressBar _progress = new()
    {
        Dock = DockStyle.Fill,
        Minimum = 0,
        Maximum = 100
    };

    private readonly Label _status = new()
    {
        Text = "Ready",
        Dock = DockStyle.Fill,
        TextAlign = ContentAlignment.MiddleLeft
    };

    private List<BeamDesignResult> _lastResults = new();

    public ExcelPreviewForm(List<Beam> beams)
    {
        _beams = beams;

        Text = "Excel Preview & Beam Design";
        StartPosition = FormStartPosition.CenterParent;
        MinimumSize = new Size(1000, 700);
        Size = new Size(1250, 800);

        _grid.DataSource = _beams;

        _bar.Items.AddRange(new object[] { 10, 12, 16, 20, 25, 28, 32 });
        _stirrup.Items.AddRange(new object[] { 8, 10, 12, 16 });
        _legs.Items.AddRange(new object[] { 2, 4 });

        _bar.SelectedItem = 20;
        _stirrup.SelectedItem = 10;
        _legs.SelectedItem = 2;

        BuildLayout();
        _start.Click += async (_, _) => await StartDesignAsync();
    }

    private void BuildLayout()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            Padding = new Padding(10)
        };

        root.RowStyles.Add(new RowStyle(SizeType.Percent, 65));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 55));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));

        var options = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight
        };

        options.Controls.Add(new Label
        {
            Text = "Main Bar Ø",
            AutoSize = true,
            Padding = new Padding(0, 8, 5, 0)
        });
        options.Controls.Add(_bar);

        options.Controls.Add(new Label
        {
            Text = "Stirrup Ø",
            AutoSize = true,
            Padding = new Padding(20, 8, 5, 0)
        });
        options.Controls.Add(_stirrup);

        options.Controls.Add(new Label
        {
            Text = "Legs",
            AutoSize = true,
            Padding = new Padding(20, 8, 5, 0)
        });
        options.Controls.Add(_legs);

        options.Controls.Add(_start);

        root.Controls.Add(_grid, 0, 0);
        root.Controls.Add(options, 0, 1);
        root.Controls.Add(_progress, 0, 2);
        root.Controls.Add(_status, 0, 3);

        Controls.Add(root);
    }

    private async Task StartDesignAsync()
    {
        var options = new DesignOptions
        {
            MainBarDiameter = Convert.ToDouble(_bar.SelectedItem),
            StirrupDiameter = Convert.ToDouble(_stirrup.SelectedItem),
            StirrupLegs = Convert.ToInt32(_legs.SelectedItem)
        };

        SetProcessing(true);

        try
        {
            var progress = new Progress<DesignProgress>(p =>
            {
                _progress.Value = Math.Clamp(p.Percentage, 0, 100);
                _status.Text =
                    $"{p.Stage} | {p.BeamName} | " +
                    $"{p.Current}/{p.Total} | {p.Percentage}%";
            });

            _lastResults =
                await BatchDesignService.DesignAsync(
                    _beams,
                    options,
                    progress);

            while (_lastResults.Any(x => !x.IsPassed))
            {
                using var review =
                    new FailedBeamsForm(
                        _lastResults,
                        options);

                if (review.ShowDialog(this) != DialogResult.OK)
                    break;

                var failed = review.CorrectedBeams;

                if (failed.Count == 0)
                    break;

                _status.Text = "Redesigning failed beams only...";

                var retry =
                    await BatchDesignService.DesignAsync(
                        failed,
                        options,
                        progress);

                // Replace only the corresponding failed results.
                foreach (var rr in retry)
                {
                    int index = _lastResults.FindIndex(
                        x => x.BeamName.Equals(
                            rr.BeamName,
                            StringComparison.OrdinalIgnoreCase));

                    if (index >= 0)
                        _lastResults[index] = rr;
                }

                // Stop this automatic correction loop if the user
                // returned no actual change.
                if (!retry.Any())
                    break;
            }

            bool allPass = _lastResults.All(x => x.IsPassed);

            if (!allPass)
            {
                MessageBox.Show(
                    this,
                    "Some Beams are still FAIL. " +
                    "Review/correct them before final export.",
                    "Incomplete Design",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            var exported =
                await ProjectExporter.ExportAll(
                    _lastResults,
                    progress);

            _progress.Value = 100;
            _status.Text = "Completed - 100%";

            MessageBox.Show(
                this,
                $"Design completed.\r\n\r\n" +
                $"PDF:\r\n{exported.Pdf}\r\n\r\n" +
                $"DXF:\r\n{exported.Dxf}\r\n\r\n" +
                $"Excel:\r\n{exported.Excel}",
                "Design Completed",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            OpenPath(exported.Pdf);
            OpenPath(exported.Dxf);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                this,
                ex.ToString(),
                "Design Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        finally
        {
            SetProcessing(false);
        }
    }

    private void SetProcessing(bool value)
    {
        _start.Enabled = !value;
        _bar.Enabled = !value;
        _stirrup.Enabled = !value;
        _legs.Enabled = !value;
        _grid.Enabled = !value;
        ControlBox = !value;
    }

    private static void OpenPath(string path)
    {
        if (!File.Exists(path))
            return;

        System.Diagnostics.Process.Start(
            new System.Diagnostics.ProcessStartInfo
            {
                FileName = path,
                UseShellExecute = true
            });
    }
}
