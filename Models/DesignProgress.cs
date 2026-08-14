namespace CivilDesignBeam.Models;

public sealed class DesignProgress
{
    public int Percentage { get; set; }
    public int Current { get; set; }
    public int Total { get; set; }
    public string BeamName { get; set; } = "";
    public string Stage { get; set; } = "";
}
