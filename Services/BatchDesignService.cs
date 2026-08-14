using CivilDesignBeam.Models;

namespace CivilDesignBeam.Services;

public static class BatchDesignService
{
    public static async Task<List<BeamDesignResult>> DesignAsync(
        IEnumerable<Beam> beams,
        DesignOptions options,
        IProgress<DesignProgress>? progress = null)
    {
        var list = beams.ToList();
        var results = new List<BeamDesignResult>();
        int total = list.Count;

        for (int i = 0; i < total; i++)
        {
            Beam beam = list[i];

            BeamDesignResult result =
                await Task.Run(() => BeamDesignEngine.Design(beam, options));

            results.Add(result);

            progress?.Report(new DesignProgress
            {
                Percentage = (int)Math.Round((i + 1) * 100.0 / total),
                Current = i + 1,
                Total = total,
                BeamName = beam.Name,
                Stage = "Designing"
            });

            await Task.Yield();
        }

        return results;
    }
}
