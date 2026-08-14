namespace CivilDesignBeam.Models;

public sealed class DesignOptions
{
    public double MainBarDiameter { get; set; } = 20;
    public double StirrupDiameter { get; set; } = 10;
    public int StirrupLegs { get; set; } = 2;
    public double PhiFlexure { get; set; } = 0.90;
    public double PhiShear { get; set; } = 0.75;
}
