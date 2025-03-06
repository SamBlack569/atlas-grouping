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
                });

                image.Dispose(); // Free memory
            });

            return [.. imageAssets];
        }

        public static int Histogram(ImageAsset asset, int hueBins = 36)
        {
            var img = new Bitmap(asset.FilePath); // Load image from asset file path

            int[] hueHistogram = new int[hueBins];
            
            int width = img.Width;
            int height = img.Height;
            int pixelCount = 0;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++) 
                {
                    Color pixel = img.GetPixel(x, y);
                    float h, s, v;
                    RgbToHsv(pixel, out h, out s, out v);

                    if (s > 0.05)
                    {
                        int hueBin = (int)(h / 360.0 * hueBins);
                        hueHistogram[hueBin]++;
                        pixelCount++;
                    }
                }
            }

            if (pixelCount == 0)
            {
                Console.WriteLine($"Warning: Image {asset.Id} has no valid hue.");
                return -1;
            }

            return Array.IndexOf(hueHistogram, hueHistogram.Max());
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
