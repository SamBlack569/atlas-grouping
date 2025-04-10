using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Concurrent;


namespace AtlasGrouping
{
    public class ImageUtils
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
        public static List<ImageAsset> LoadImageAssetsFromDirectory(string directoryPath, int hueBins = 36, int satBins = 10, int valBins = 10)
        {

            var imageAssets = new ConcurrentBag<ImageAsset>(); // Thread-safe collection
            var imageFiles = Directory.GetFiles(directoryPath, "*.png"); // Adjust for other formats

            Parallel.ForEach(imageFiles, filePath =>
            {
                var image = new Bitmap(filePath);

                imageAssets.Add(new ImageAsset
                {
                    Id = Path.GetFileNameWithoutExtension(filePath),
                    FilePath = filePath,
                    Width = image.Width,    
                    Height = image.Height,  
                });

                image.Dispose(); // Free memory
            });

            return [.. imageAssets];
        }

        public static int Histogram(ImageAsset asset, int hueBins)
        {
            var img = new Bitmap(asset.FilePath); // Load image from asset file path

            int[] hueHistogram = new int[hueBins];
            
            int width = asset.Width;
            int height = asset.Height;

            int pixelCount = 0;
            int blackPixels = 0;
            int whitePixels = 0;
            int grayPixels = 0;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++) 
                {
                    Color pixel = img.GetPixel(x, y);

                    if(pixel.A < 50) continue; // Skip transparent pixels and almost transparent pixels

                    float h, s, v;
                    RgbToHsv(pixel, out h, out s, out v);

                    if (s <= 0.15) // Low saturation, likely a shade of gray, black, or white
                    {
                        if (v <= 0.05) blackPixels++; // Too dark -> black

                        else if (v >= 0.95) whitePixels++; // Too bright -> white

                        else grayPixels++; // In between -> gray

                        continue;
                    }

                    // Map hue to bins
                    int hueBin = (int)((h / 360.0) * hueBins);
                    hueBin = Math.Min(hueBin, hueBins - 1); // Ensure it doesn't go out of bounds
                    hueHistogram[hueBin]++;
                    pixelCount++;
                }
            }

            // Check if black, white, or gray is dominant
            if (blackPixels > pixelCount * 0.5) return -2; // Black 
            if (whitePixels > pixelCount * 0.5) return -3; // White 
            if (grayPixels > pixelCount * 0.5) return -4; // Gray 

            if (pixelCount == 0)
            {
                Console.WriteLine($"Warning: Image {asset.Id} has no valid hue.");
                return -1;
            }

            // Find the hue bin with the most pixels (dominant hue)
            int dominantHueBin = Array.IndexOf(hueHistogram, hueHistogram.Max());

            return dominantHueBin;
        }

        public static void RgbToHsv(Color color, out float h, out float s, out float v)
        {
            float r = color.R / 255f;
            float g = color.G / 255f;
            float b = color.B / 255f;

            float max = Math.Max(r, Math.Max(g, b));
            float min = Math.Min(r, Math.Min(g, b));
            float delta = max - min;

            h = 0;

            if (delta == 0)
            {
                h = 0;
            }
            else if (max == r)
            {
                h = 60f * ((g - b) / delta);
                if (h < 0) h += 360;
            }
            else if (max == g)
            {
                h = 60f * ((b - r) / delta + 2);
            }
            else if (max == b)
            {
                h = 60f * ((r - g) / delta + 4);
            }

            if (h < 0) h += 360;

            s = (max == 0) ? 0 : (delta / max);
            v = max;
        }

        public static List<HueAsset> FilterHueAssetList(List<HueAsset> hueAssetList, int atlasWidth, int atlasHeight, Dictionary<string, ImageAsset> assetLookup)
        {
            var filteredList = new List<HueAsset>();

            foreach (var asset in hueAssetList)
            {
                if (!assetLookup.TryGetValue(asset.AssetId, out var original))
                {
                    Console.WriteLine($"Warning: Asset {asset.AssetId} not found in lookup.");
                    continue;
                }

                if (original.Width > atlasWidth || original.Height > atlasHeight)
                {
                    Console.WriteLine($"Warning: Asset {asset.AssetId} is too large for the atlas and will not be added.");
                }
                else
                {
                    filteredList.Add(asset);
                }
            }

            // Separate normal hues from special hues
            var normalHues = filteredList.Where(a => a.Hue >= 0).OrderBy(a => a.Hue).ToList();
            var specialHues = filteredList.Where(a => a.Hue < 0).ToList(); // -2, -3, -4

            // Add special hues to the end of the list
            var sortedList = new List<HueAsset>();
            sortedList.AddRange(normalHues);
            sortedList.AddRange(specialHues);

            return sortedList;
        }

        public static List<List<string>> Separate(List<HueAsset> sortedList, int hueBins, Dictionary<string, ImageAsset> assetLookup)
        {
            var listOfSubLists = new List<List<string>>();

            // Initialize sublists
            for (int i = 0; i < hueBins + 3; i++)  // plus 3 for (black, white and gray)
                listOfSubLists.Add(new List<string>());


            foreach (var asset in sortedList)
            {
                int idx;

                if (asset.Hue < 0)
                {
                    // Special Hue (-2 Black, -3 White, -4 Gray)
                    idx = hueBins + (int)(-asset.Hue) - 2;
                    // Black index = 36, White index = 37, Gray index = 38
                }
                else
                {
                    idx = (int)asset.Hue;
                }

                if (idx >= 0 && idx < listOfSubLists.Count)
                {
                    listOfSubLists[idx].Add(asset.AssetId);
                }
                else
                {
                    Console.WriteLine($"[Warning] Hue index {idx} is out of bounds for asset {asset.AssetId} (Hue: {asset.Hue})");
                }
            }

            Console.WriteLine("Enter asset IDs (separated by commas) thatshould be together in the same sublist, then press Enter when done:");

            List<string> assetsToGroup = new List<string>();

            while (true)
            {
                string input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input)) // If user just presses Enter without typing anything, stop
                    break;

                var group = input.Split(',').Select(id => id.Trim()).ToList();

                if (group.Count < 2)
                {
                    Console.WriteLine("No assets to group.");
                    continue;
                }

                string biggest = null;
                int biggestArea = -1;

                foreach (var id in group)
                {
                    if (assetLookup.TryGetValue(id, out var asset))
                    {
                        int area = asset.Width * asset.Height;
                        if (area > biggestArea)
                        {
                            biggestArea = area;
                            biggest = id;
                        }
                    }
                }

                if (biggest == null)
                {
                    Console.WriteLine("No valid assets from this group found in the dictionary.");
                    continue;
                }

                // Find the sublist that contains the biggest asset
                List<string> targetSublist = listOfSubLists.FirstOrDefault(sublist => sublist.Contains(biggest));
                if (targetSublist == null)
                {
                    Console.WriteLine($"[Error] Could not find sublist for the largest asset ({biggest}).");
                    continue;
                }

                // Remove all assets in the group from their current sublists
                foreach (var id in group)
                {
                    foreach (var sublist in listOfSubLists)
                    {
                        sublist.Remove(id);
                    }
                }

                // Add the group to the target sublist
                foreach (var id in group)
                {
                    if (!targetSublist.Contains(id))
                        targetSublist.Add(id);
                }

                Console.WriteLine($"^Moved group with {group.Count} assets to sublist of largest asset ({biggest}).");

            }
            
            return listOfSubLists;
        }


    }
}
