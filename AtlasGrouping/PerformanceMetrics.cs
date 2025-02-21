using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtlasGrouping
{
    // Performance metrics for the entire atlas generation process.
    public class PerformanceMetrics
    {
        public int TotalAtlases { get; set; }              // Total number of atlases generated.
        public double BuildTimeSeconds { get; set; }         // Total time taken to build all atlases.
        public List<AtlasPerformanceMetrics> AtlasMetricsList { get; set; } = new List<AtlasPerformanceMetrics>();
    }

    // Metrics for an individual atlas.
    public class AtlasPerformanceMetrics
    {
        public int AtlasIndex { get; set; }                // Index/ID for the atlas.
        public int AtlasWidth { get; set; }                // Width of the atlas.
        public int AtlasHeight { get; set; }               // Height of the atlas.
        public int NumberOfAssets { get; set; }            // Number of assets packed into this atlas.
                                                           // Average histogram distance among assets in this atlas.
                                                           // Lower values indicate higher color similarity.
        public double AverageHistogramDistance { get; set; }
    }

    // Helper class to calculate metrics.
    public static class MetricsCalculator
    {

        // Compute Euclidean distance between two histograms.
        // Assumes both histograms have the same length.
        public static double ComputeHistogramDistance(double[] hist1, double[] hist2)
        {
            if (hist1.Length != hist2.Length)
                throw new ArgumentException("Histogram lengths must match.");

            double sum = 0.0;
            for (int i = 0; i < hist1.Length; i++)
            {
                double diff = hist1[i] - hist2[i];
                sum += diff * diff;
            }
            return Math.Sqrt(sum);
        }

        // Compute the average histogram distance among all assets in an atlas.
        public static double ComputeAverageHistogramDistance(TextureAtlas atlas)
        {
            var entries = atlas.PlacedEntries;
            int count = entries.Count;
            if (count < 2)
                return 0.0; // Not enough assets to compare.

            double totalDistance = 0.0;
            int comparisons = 0;

            for (int i = 0; i < count; i++)
            {
                for (int j = i + 1; j < count; j++)
                {
                    // Ensure both assets have histograms.
                    if (entries[i].Asset.ColorHistogram != null && entries[j].Asset.ColorHistogram != null)
                    {
                        totalDistance += ComputeHistogramDistance(entries[i].Asset.ColorHistogram,
                                                                    entries[j].Asset.ColorHistogram);
                        comparisons++;
                    }
                }
            }
            return (comparisons > 0) ? totalDistance / comparisons : 0.0;
        }

        // Gather all performance metrics for the generated atlases.
        public static PerformanceMetrics GatherMetrics(List<TextureAtlas> atlases, TimeSpan buildTime)
        {
            PerformanceMetrics metrics = new PerformanceMetrics
            {
                TotalAtlases = atlases.Count,
                BuildTimeSeconds = buildTime.TotalSeconds
            };

            int atlasIndex = 1;
            foreach (var atlas in atlases)
            {
                AtlasPerformanceMetrics atlasMetrics = new AtlasPerformanceMetrics
                {
                    AtlasIndex = atlasIndex,
                    AtlasWidth = atlas.AtlasWidth,
                    AtlasHeight = atlas.AtlasHeight,
                    NumberOfAssets = atlas.PlacedEntries.Count,
                    AverageHistogramDistance = ComputeAverageHistogramDistance(atlas)
                };
                metrics.AtlasMetricsList.Add(atlasMetrics);
                atlasIndex++;
            }
            return metrics;
        }
    }
}
