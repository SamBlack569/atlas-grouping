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
        /*
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
        */


        // Main entry point: generate atlases from a list of image assets
        public List<TextureAtlas> GenerateAtlases(List<List<string>> assetSubLists, Dictionary<string, ImageAsset> assetLookup, int atlasWidth, int atlasHeight, int hueBins)
        {
            // ---- Just an example -----
            List<TextureAtlas> allAtlases = new List<TextureAtlas>();
       
            TextureAtlas currentAtlas = new TextureAtlas { AtlasWidth = atlasWidth, AtlasHeight = atlasHeight };

            // Calculate the total area of all assets
            int totalAssetArea = assetSubLists.Sum(sublist => sublist.Sum(assetId =>
            {
                if (assetLookup.TryGetValue(assetId, out var asset))
                {
                    return asset.Width * asset.Height;
                }
                return 0;
            }));

            int atlasArea = atlasWidth * atlasHeight;
            int minAtlases = (int)Math.Ceiling((double)totalAssetArea / atlasArea); // Minimum number of atlases required

            List<int> subListsSizes = assetSubLists.Select(subList => subList.Count).ToList();

            int zeroscount = 0;

            List<Gap> gaps = new List<Gap>();

            for (int i = 0; i < subListsSizes.Count; i++)
            {
                int size = subListsSizes[i];

                if (size > 0)
                {
                    if (zeroscount > 0)
                    {
                        Gap gap = new Gap { Index = i, Size = zeroscount };
                        zeroscount = 0;
                        gaps.Add(gap);
                        Console.WriteLine($"Gap at index {gap.Index} with size {gap.Size}");
                    }
                    else
                    {
                        continue;
                    }
                }
                else
                {
                    zeroscount++;
                }

            }

            int maxGapSize = gaps.Max(gap => gap.Size);
            int maxGapIndex = gaps.FindIndex(gap => gap.Size == maxGapSize);

            int startIndex = maxGapIndex; // Start after the largest gap
            List<List<string>> reorderedSubLists = new List<List<string>>();

            
            for (int i = 0; i < hueBins; i++)
            {
                int index = (startIndex + i) % hueBins;
                reorderedSubLists.Add(assetSubLists[index]);
            }

            // Add special hues at the end
            reorderedSubLists.Add(assetSubLists[hueBins]); // Black
            reorderedSubLists.Add(assetSubLists[hueBins + 1]); // White
            reorderedSubLists.Add(assetSubLists[hueBins + 2]); // Gray

            int currentX = 0, currentY = 0;
            int currentRowHeight = 0;

            foreach (var sublist in reorderedSubLists)
            {
                foreach (var assetId in sublist)
                {
                    if (!assetLookup.TryGetValue(assetId, out var asset))
                    {
                        Console.WriteLine($"Warning: Asset {assetId} not found in lookup.");
                        continue;
                    }

                    // Move to next row if width overflow
                    if (currentX + asset.Width > atlasWidth)
                    {
                        currentX = 0;
                        currentY += currentRowHeight;
                        currentRowHeight = 0;
                    }

                    // Start a new atlas if height overflows
                    if (currentY + asset.Height > atlasHeight)
                    {
                        // Start a new atlas if height overflows 
                        allAtlases.Add(currentAtlas);
                        currentAtlas = new TextureAtlas { AtlasWidth = atlasWidth, AtlasHeight = atlasHeight };
                        currentX = 0;
                        currentY = 0;
                    }

                    // Add asset in current atlas
                    currentAtlas.PlacedEntries.Add(new AtlasEntry
                    {
                        Asset = asset,
                        X = currentX,
                        Y = currentY,
                        Width = asset.Width,
                        Height = asset.Height
                    });

                    currentX += asset.Width;
                    currentRowHeight = Math.Max(currentRowHeight, asset.Height);
                }
            }

            // Add final atlas if it has any content
            if (currentAtlas.PlacedEntries.Count > 0)
            {
                allAtlases.Add(currentAtlas);
            }
            
            return allAtlases;
        }

        // Method to save generated atlases as PNG images
        public void SaveAtlasesAsImages(List<TextureAtlas> atlases, string outputFolder)
        {
            Directory.CreateDirectory(outputFolder);

            for (int i = 0; i < atlases.Count; i++)
            {
                var atlas = atlases[i];
                using Bitmap bitmap = new Bitmap(atlas.AtlasWidth, atlas.AtlasHeight);
                using Graphics g = Graphics.FromImage(bitmap);

                g.Clear(Color.Transparent); // Optional: fill background with transparency

                foreach (var entry in atlas.PlacedEntries)
                {
                    using var img = Image.FromFile(entry.Asset.FilePath); 
                    g.DrawImage(img, entry.X, entry.Y, entry.Width, entry.Height);
                }

                string outputPath = Path.Combine(outputFolder, $"atlas_{i}.png");
                bitmap.Save(outputPath);
                Console.WriteLine($"Saved {outputPath}");
            }
        }

    }
}
