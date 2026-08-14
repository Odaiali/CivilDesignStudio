using CivilDesignBeam.Models;

namespace CivilDesignBeam.Services;

public static class FailureCorrectionService
{
    public static Beam ApplySuggestedCorrection(
        Beam source,
        DesignFailure failure,
        DesignOptions options)
    {
        var beam = source.Clone();

        switch (failure.Type)
        {
            case FailureType.BeamWidth:
            case FailureType.BarFit:
                beam.Width += 50;
                break;

            case FailureType.BeamDepth:
            case FailureType.Deflection:
                beam.Height += 50;
                break;

            case FailureType.Shear:
            case FailureType.StirrupSpacing:
                // Handled by the design engine's spacing selection.
                break;

            default:
                // For reinforcement-related failures, keep geometry unchanged.
                break;
        }

        return beam;
    }
}
