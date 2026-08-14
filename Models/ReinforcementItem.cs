namespace CivilDesignBeam.Models;

public sealed class ReinforcementItem
{
    public string Mark { get; set; } = "";
    public string Location { get; set; } = "";
    public string Type { get; set; } = "";
    public double Diameter { get; set; }
    public int Quantity { get; set; }
    public double Spacing { get; set; }
    public double Length { get; set; }

    public double UnitWeight => Diameter * Diameter / 162.0;
    public double TotalLength => Quantity * Length;
    public double TotalWeight => TotalLength * UnitWeight;
}
