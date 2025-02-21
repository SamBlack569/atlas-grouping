using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtlasGrouping
{
    public class AtlasBuilder
    {

        public void RunGenerationAndMetrics(List<ImageAsset> assets, int atlasWidth, int atlasHeight)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            List<TextureAtlas> atlases = GenerateAtlases(assets, atlasWidth, atlasHeight);
            stopwatch.Stop();

            PerformanceMetrics metrics = MetricsCalculator.GatherMetrics(atlases, stopwatch.Elapsed);
            // Output or log the metrics.
            Console.WriteLine($"Total Atlases: {metrics.TotalAtlases}");
            Console.WriteLine($"Build Time (s): {metrics.BuildTimeSeconds:F2}");
            foreach (var atlasMetric in metrics.AtlasMetricsList)
            {
                Console.WriteLine($"Atlas {atlasMetric.AtlasIndex}: Size {atlasMetric.AtlasWidth}x{atlasMetric.AtlasHeight}, " +
                                  $"Assets: {atlasMetric.NumberOfAssets}, " +
                                  $"Avg. Histogram Distance: {atlasMetric.AverageHistogramDistance:F2}");
            }
        }


        // Main entry point: generate atlases from a list of image assets
        public List<TextureAtlas> GenerateAtlases(List<ImageAsset> assets, int atlasWidth, int atlasHeight)
        {
            // ---- Just an example -----
            List<TextureAtlas> allAtlases = new List<TextureAtlas>();
       
            TextureAtlas currentAtlas = new TextureAtlas { AtlasWidth = atlasWidth, AtlasHeight = atlasHeight };

            foreach (var asset in assets)
            {
                currentAtlas.PlacedEntries.Add(new AtlasEntry
                {
                    Asset = asset,
                    X = 0,
                    Y =0,
                    Width = asset.Width,
                    Height = asset.Height
                });
              
            }
            // Finalize the last atlas in the group.
            allAtlases.Add(currentAtlas);            
            return allAtlases;
        }

    }
}
