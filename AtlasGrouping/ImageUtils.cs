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

        //Just an example
        public static List<List<ImageAsset>> ClusterImages(List<ImageAsset> assets, int maxClusters = 8)
        {
            var clusters = new ConcurrentBag<List<ImageAsset>>();

            Parallel.ForEach(Enumerable.Range(0, maxClusters), _ =>
            {
                var cluster = new List<ImageAsset>();
                foreach (var asset in assets)
                {
                    if (cluster.Count == 0 || IsSimilar(asset, cluster[0]))
                    {
                        cluster.Add(asset);
                    }
                }
                clusters.Add(cluster);
            });

            return clusters.ToList();
        }

        private static bool IsSimilar(ImageAsset a, ImageAsset b)
        {
            return true;
        }

    }

}
