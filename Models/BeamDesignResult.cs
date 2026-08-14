namespace CivilDesignBeam.Models;

public sealed class BeamDesignResult
{
    public Beam SourceBeam { get; set; } = new();

    public string BeamName => SourceBeam.Name;
    public double BeamWidth => SourceBeam.Width;
    public double BeamHeight => SourceBeam.Height;
    public double BeamLength => SourceBeam.Length;
    public double Fc => SourceBeam.Fc;
    public double Fy => SourceBeam.Fy;
    public double Cover => SourceBeam.Cover;

    public double Mu { get; set; }                 // kN.m
    public double Vu { get; set; }                 // kN
    public double EffectiveDepth { get; set; }     // mm

    public double RequiredAs { get; set; }         // mm2
    public double MinimumAs { get; set; }          // mm2
    public double ProvidedAs { get; set; }         // mm2
    public int NumberOfBars { get; set; }
    public double MainBarDiameter { get; set; }

    public double Vc { get; set; }                 // kN
    public double VsRequired { get; set; }         // kN
    public double PhiVn { get; set; }              // kN
    public double StirrupDiameter { get; set; }
    public int StirrupLegs { get; set; }
    public double StirrupSpacing { get; set; }     // mm

    public double DevelopmentLength { get; set; }  // mm
    public double ImmediateDeflection { get; set; }// mm
    public double AllowableDeflection { get; set; }// mm

    public bool FlexureOK { get; set; }
    public bool ShearOK { get; set; }
    public bool MinimumSteelOK { get; set; }
    public bool DevelopmentOK { get; set; }
    public bool DeflectionOK { get; set; }
    public bool BarFitOK { get; set; }

    public bool IsPassed =>
        FlexureOK && ShearOK && MinimumSteelOK &&
        DevelopmentOK && DeflectionOK && BarFitOK;

    public List<DesignFailure> Failures { get; } = new();
    public List<ReinforcementItem> ReinforcementItems { get; } = new();
    public List<RevisionRecord> RevisionHistory { get; } = new();
}
