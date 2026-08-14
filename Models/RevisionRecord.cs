namespace CivilDesignBeam.Models;

public sealed class RevisionRecord
{
    public int Revision { get; set; }
    public string BeamName { get; set; } = "";
    public string Change { get; set; } = "";
    public string Status { get; set; } = "";
    public DateTime Time { get; set; } = DateTime.Now;
}
