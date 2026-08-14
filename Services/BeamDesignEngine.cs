using CivilDesignBeam.Models;

namespace CivilDesignBeam.Services;

public static class BeamDesignEngine
{
    // IMPORTANT:
    // This is the software architecture and a preliminary educational
    // calculation engine. Before professional use, every ACI 318-19
    // clause, unit conversion, detailing limit, load combination,
    // development/splice rule, and deflection rule must be independently
    // verified against the licensed code and project criteria.

    public static BeamDesignResult Design(Beam beam, DesignOptions o)
    {
        var r = new BeamDesignResult { SourceBeam = beam.Clone() };

        double Lm = beam.Length / 1000.0;
        double wu = 1.2 * beam.DeadLoad + 1.6 * beam.LiveLoad; // kN/m
        r.Mu = wu * Lm * Lm / 8.0;
        r.Vu = wu * Lm / 2.0;

        double db = o.MainBarDiameter;
        double stirrup = o.StirrupDiameter;
        double d = beam.Height - beam.Cover - stirrup - db / 2.0;
        r.EffectiveDepth = d;
        r.MainBarDiameter = db;
        r.StirrupDiameter = stirrup;
        r.StirrupLegs = o.StirrupLegs;

        double MuNmm = r.Mu * 1_000_000.0;
        double phi = o.PhiFlexure;
        double As = 0;

        // Iterative singly reinforced approximation.
        double Rn = MuNmm / (phi * beam.Width * d * d);
        double m = beam.Fy / (0.85 * beam.Fc);
        double ratioTerm = Math.Max(0, 1 - 2 * Rn / beam.Fy);
        As = (0.85 * beam.Fc / beam.Fy) *
             beam.Width * d *
             (1 - Math.Sqrt(ratioTerm));

        double fyForMin = Math.Min(beam.Fy, 550.0);
        double as1 = 0.25 * Math.Sqrt(beam.Fc) / fyForMin * beam.Width * d;
        double as2 = 1.4 / fyForMin * beam.Width * d;

        r.RequiredAs = Math.Max(0, As);
        r.MinimumAs = Math.Max(as1, as2);

        double barArea = Math.PI * db * db / 4.0;
        r.NumberOfBars = Math.Max(2,
            (int)Math.Ceiling(Math.Max(r.RequiredAs, r.MinimumAs) / barArea));
        r.ProvidedAs = r.NumberOfBars * barArea;

        // Approximate capacity check.
        double a = r.ProvidedAs * beam.Fy / (0.85 * beam.Fc * beam.Width);
        double Mn = r.ProvidedAs * beam.Fy * (d - a / 2.0) / 1_000_000.0;
        r.FlexureOK = phi * Mn >= r.Mu;
        r.MinimumSteelOK = r.ProvidedAs >= r.MinimumAs;

        // Approximate shear check.
        double VcN = 0.17 * Math.Sqrt(beam.Fc) * beam.Width * d;
        r.Vc = VcN / 1000.0;

        double phiVc = o.PhiShear * r.Vc;
        double VsNeed = Math.Max(0, r.Vu / o.PhiShear - r.Vc);
        r.VsRequired = VsNeed;

        double av = o.StirrupLegs * Math.PI * stirrup * stirrup / 4.0;
        double s = VsNeed <= 0
            ? 300
            : av * beam.Fy * d / (VsNeed * 1000.0);

        s = Math.Min(300, Math.Max(50, Math.Floor(s / 5.0) * 5.0));
        r.StirrupSpacing = s;

        double VsProvided =
            av * beam.Fy * d / (s * 1000.0);

        r.PhiVn =
            o.PhiShear * (r.Vc + VsProvided);

        r.ShearOK = r.PhiVn >= r.Vu;

        // Simplified development check for the software prototype.
        r.DevelopmentLength =
            Math.Max(300,
                beam.Fy * db /
                (1.1 * Math.Sqrt(beam.Fc)));

        r.DevelopmentOK =
            r.DevelopmentLength <= beam.Length;

        // Elastic immediate deflection approximation for simply supported UDL.
        double E = 4700 * Math.Sqrt(beam.Fc); // MPa approximation
        double Ig = beam.Width * Math.Pow(beam.Height, 3) / 12.0;
        double wNmm = (beam.DeadLoad + beam.LiveLoad); // N/mm
        r.ImmediateDeflection =
            5 * wNmm * Math.Pow(beam.Length, 4) /
            (384 * E * Ig);

        r.AllowableDeflection = beam.Length / 360.0;
        r.DeflectionOK =
            r.ImmediateDeflection <= r.AllowableDeflection;

        // Bar fit: clear width after cover and stirrup.
        double available = beam.Width - 2 * (beam.Cover + stirrup);
        double required = r.NumberOfBars * db +
                          Math.Max(0, r.NumberOfBars - 1) * 25.0;
        r.BarFitOK = required <= available;

        BuildReinforcement(r, o);
        BuildFailures(r, beam, o);

        return r;
    }

    private static void BuildReinforcement(
        BeamDesignResult r, DesignOptions o)
    {
        double mainLength = r.BeamLength / 1000.0;
        r.ReinforcementItems.Add(new ReinforcementItem
        {
            Mark = r.BeamName + "-B",
            Location = "Bottom",
            Type = "Main",
            Diameter = o.MainBarDiameter,
            Quantity = r.NumberOfBars,
            Length = mainLength
        });

        r.ReinforcementItems.Add(new ReinforcementItem
        {
            Mark = r.BeamName + "-T",
            Location = "Top",
            Type = "Main",
            Diameter = o.MainBarDiameter,
            Quantity = 2,
            Length = mainLength
        });

        int count = Math.Max(2,
            (int)Math.Ceiling(r.BeamLength / r.StirrupSpacing) + 1);

        double stirrupLength =
            2 * (r.BeamWidth - 2 * r.Cover) +
            2 * (r.BeamHeight - 2 * r.Cover);

        r.ReinforcementItems.Add(new ReinforcementItem
        {
            Mark = r.BeamName + "-S",
            Location = "Shear",
            Type = "Stirrup",
            Diameter = o.StirrupDiameter,
            Quantity = count,
            Spacing = r.StirrupSpacing,
            Length = stirrupLength / 1000.0
        });
    }

    private static void BuildFailures(
        BeamDesignResult r,
        Beam beam,
        DesignOptions o)
    {
        if (!r.MinimumSteelOK)
            r.Failures.Add(new DesignFailure
            {
                Type = FailureType.MinimumReinforcement,
                CheckName = "Minimum Reinforcement",
                Reason = "Provided steel is below the calculated minimum.",
                CurrentValue = $"{r.ProvidedAs:0} mm²",
                RequiredValue = $"{r.MinimumAs:0} mm²",
                SuggestedValue = "Increase bar area.",
                SuggestedAction = "Increase bar diameter or number of bars."
            });

        if (!r.FlexureOK)
            r.Failures.Add(new DesignFailure
            {
                Type = FailureType.Flexure,
                CheckName = "Flexure",
                Reason = "Design flexural capacity is less than Mu.",
                CurrentValue = $"{r.NumberOfBars}Ø{o.MainBarDiameter:0}",
                RequiredValue = $"As ≥ {r.RequiredAs:0} mm²",
                SuggestedValue = "Increase main reinforcement.",
                SuggestedAction = "Increase bar diameter/number or revise section."
            });

        if (!r.ShearOK)
            r.Failures.Add(new DesignFailure
            {
                Type = FailureType.Shear,
                CheckName = "Shear",
                Reason = "Design shear capacity is less than Vu.",
                CurrentValue = $"{o.StirrupLegs}Ø{o.StirrupDiameter:0} @ {r.StirrupSpacing:0}",
                RequiredValue = $"φVn ≥ {r.Vu:0.00} kN",
                SuggestedValue = "Reduce stirrup spacing or increase section.",
                SuggestedAction = "Increase stirrup area/reduce spacing."
            });

        if (!r.DevelopmentOK)
            r.Failures.Add(new DesignFailure
            {
                Type = FailureType.DevelopmentLength,
                CheckName = "Development Length",
                Reason = "Available development length is insufficient.",
                CurrentValue = $"{beam.Length:0} mm",
                RequiredValue = $"{r.DevelopmentLength:0} mm",
                SuggestedValue = "Increase available length or revise detailing.",
                SuggestedAction = "Review anchorage/detailing."
            });

        if (!r.DeflectionOK)
            r.Failures.Add(new DesignFailure
            {
                Type = FailureType.Deflection,
                CheckName = "Deflection",
                Reason = "Calculated immediate deflection exceeds the selected limit.",
                CurrentValue = $"{r.ImmediateDeflection:0.00} mm",
                RequiredValue = $"{r.AllowableDeflection:0.00} mm",
                SuggestedValue = "Increase beam stiffness.",
                SuggestedAction = "Increase depth or revise section."
            });

        if (!r.BarFitOK)
            r.Failures.Add(new DesignFailure
            {
                Type = FailureType.BarFit,
                CheckName = "Bar Fit",
                Reason = "Selected reinforcement does not fit within available width.",
                CurrentValue = $"{beam.Width:0} mm",
                RequiredValue = "Increase clear reinforcement space.",
                SuggestedValue = "Increase width or change bar arrangement.",
                SuggestedAction = "Increase beam width or revise bar arrangement."
            });
    }
}
