namespace CivilDesignBeam.Models;

public sealed class Beam
{
    public string Name { get; set; } = "";
    public double Width { get; set; }       // mm
    public double Height { get; set; }      // mm
    public double Length { get; set; }      // mm
    public double Fc { get; set; }          // MPa
    public double Fy { get; set; }          // MPa
    public double Cover { get; set; }       // mm
    public double DeadLoad { get; set; }    // kN/m
    public double LiveLoad { get; set; }    // kN/m

    public Beam Clone() => (Beam)MemberwiseClone();
}
