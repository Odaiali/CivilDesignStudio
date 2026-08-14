namespace CivilDesignBeam.Models;

public enum FailureType
{
    BeamWidth,
    BeamDepth,
    BarDiameter,
    NumberOfBars,
    StirrupDiameter,
    StirrupSpacing,
    Flexure,
    Shear,
    DevelopmentLength,
    Deflection,
    BarSpacing,
    BarFit,
    MinimumReinforcement
}

public sealed class DesignFailure
{
    public FailureType Type { get; set; }
    public string CheckName { get; set; } = "";
    public string Reason { get; set; } = "";
    public string CurrentValue { get; set; } = "";
    public string RequiredValue { get; set; } = "";
    public string SuggestedValue { get; set; } = "";
    public string SuggestedAction { get; set; } = "";
}
