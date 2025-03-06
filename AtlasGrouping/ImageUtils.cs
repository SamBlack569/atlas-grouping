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

        public static int Histogram(ImageAsset asset, int hueBins = 36)
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

                    if(pixel.A == 0) continue; // Skip transparent pixels

                    float h, s, v;
                    RgbToHsv(pixel, out h, out s, out v);

                    if (s <= 0.05) // Low saturation, likely a shade of gray, black, or white
                    {
                        if (v <= 0.1) blackPixels++; // Too dark -> black

                        else if (v >= 0.9) whitePixels++; // Too bright -> white

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
            if (blackPixels > pixelCount * 0.7) return -2; // Black 
            if (whitePixels > pixelCount * 0.7) return -3; // White 
            if (grayPixels > pixelCount * 0.7) return -4; // Gray 

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

            if (delta > 0)
            {
                if (max == r) h = 60 * (((g - b) / delta) % 6);
                else if (max == g) h = 60 * (((b - r) / delta) + 2);
                else if (max == b) h = 60 * (((r - g) / delta) + 4);

            }

            if (h < 0) h += 360;

            s = (max == 0) ? 0 : (delta / max);
            v = max;
        }
    }
}
