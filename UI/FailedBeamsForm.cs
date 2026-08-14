using CivilDesignBeam.Models;

namespace CivilDesignBeam;

public sealed class FailedBeamsForm : Form
{
    private readonly List<BeamDesignResult> _results;
    private readonly DesignOptions _options;

    private readonly DataGridView _grid = new()
    {
        Dock = DockStyle.Fill,
        ReadOnly = true,
        AllowUserToAddRows = false,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
    };

    private readonly NumericUpDown _width = new()
    {
        Minimum = 100,
        Maximum = 3000,
        Increment = 25,
        Value = 300
    };

    private readonly NumericUpDown _height = new()
    {
        Minimum = 150,
        Maximum = 3000,
        Increment = 25,
        Value = 500
    };

    private readonly ComboBox _bar = new()
    {
        DropDownStyle = ComboBoxStyle.DropDownList
    };

    private readonly Button _apply = new()
    {
        Text = "Apply correction",
        AutoSize = true
    };

    public List<Beam> CorrectedBeams { get; } = new();

    public FailedBeamsForm(
        List<BeamDesignResult> results,
        DesignOptions options)
    {
        _results = results;
        _options = options;

        Text = "Failed Beams - Correction";
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(1200, 750);
        MinimumSize = new Size(1000, 650);

        Build();
        LoadGrid();

        _apply.Click += (_, _) => ApplyCorrection();
    }

    private void Build()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(10)
        };

        root.RowStyles.Add(new RowStyle(SizeType.Percent, 70));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 90));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 55));

        var editor = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill
        };

        editor.Controls.Add(new Label
        {
            Text = "New Width",
            AutoSize = true,
            Padding = new Padding(0, 8, 5, 0)
        });
        editor.Controls.Add(_width);

        editor.Controls.Add(new Label
        {
            Text = "New Depth",
            AutoSize = true,
            Padding = new Padding(15, 8, 5, 0)
        });
        editor.Controls.Add(_height);

        _bar.Items.AddRange(new object[] { 10, 12, 16, 20, 25, 28, 32 });
        _bar.SelectedItem = _options.MainBarDiameter;

        editor.Controls.Add(new Label
        {
            Text = "Main Bar Ø",
            AutoSize = true,
            Padding = new Padding(15, 8, 5, 0)
        });
        editor.Controls.Add(_bar);

        root.Controls.Add(_grid, 0, 0);
        root.Controls.Add(editor, 0, 1);
        root.Controls.Add(_apply, 0, 2);

        Controls.Add(root);
    }

    private void LoadGrid()
    {
        var rows = _results
            .Where(x => !x.IsPassed)
            .Select(x => new
            {
                Beam = x.BeamName,
                Status = "FAIL",
                Checks = string.Join(", ", x.Failures.Select(f => f.CheckName)),
                Reason = string.Join(" | ", x.Failures.Select(f => f.Reason)),
                Current = string.Join(" | ", x.Failures.Select(f => f.CurrentValue)),
                Required = string.Join(" | ", x.Failures.Select(f => f.RequiredValue)),
                Suggested = string.Join(" | ", x.Failures.Select(f => f.SuggestedAction))
            })
            .ToList();

        _grid.DataSource = rows;
    }

    private void ApplyCorrection()
    {
        if (_grid.CurrentRow == null)
            return;

        string beamName =
            Convert.ToString(
                _grid.CurrentRow.Cells["Beam"].Value) ?? "";

        var source = _results
            .FirstOrDefault(x =>
                x.BeamName.Equals(
                    beamName,
                    StringComparison.OrdinalIgnoreCase))
            ?.SourceBeam;

        if (source == null)
            return;

        var corrected = source.Clone();
        corrected.Width = (double)_width.Value;
        corrected.Height = (double)_height.Value;

        if (_bar.SelectedItem != null)
            _options.MainBarDiameter =
                Convert.ToDouble(_bar.SelectedItem);

        CorrectedBeams.Clear();
        CorrectedBeams.Add(corrected);

        DialogResult = DialogResult.OK;
        Close();
    }
}
