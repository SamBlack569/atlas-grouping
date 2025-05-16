//#define NO_SORT


using System.Diagnostics;
using System.Text.Json;



namespace AtlasGrouping
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("---- Atlas Builder ----");


            Stopwatch stopwatch = Stopwatch.StartNew();

            // Ask user to input the image folder path
            Console.Write("Enter the path to the image folder: ");
            string imageFolder = Console.ReadLine()?.Trim();

            // Check if the folder exists
            if (string.IsNullOrEmpty(imageFolder) || !Directory.Exists(imageFolder))
            {
                Console.WriteLine("Error: The specified folder does not exist.");
                return;
            }

            int atlasWidth = 2048;
            int atlasHeight = 2048;
            int hueBins = 36; // 

            List<ImageAsset> assets = ImageUtils.LoadImageAssetsFromDirectory(imageFolder);

            // Save to JSON, just because we can
            string json = JsonSerializer.Serialize(assets, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText("image_assets.json", json);
            Console.WriteLine("Saved ImageAsset list to image_assets.json");

            // Create a lookup dictionary for assets
            var assetLookup = assets.ToDictionary(a => a.Id, a => a);
#if !NO_SORT
            var hueAssetList = new List<HueAsset>();

            foreach (var asset in assets)
            {
                int hue = ImageUtils.Histogram(asset, hueBins);
                Console.WriteLine($"Loaded: {asset.Id}, Size: {asset.Width}x{asset.Height}, Hue dominant: {hue}");

                hueAssetList.Add(new HueAsset { AssetId = asset.Id, Hue = hue });
            }

            // Sort the assets by hue
            var sortedHueAssets = ImageUtils.FilterHueAssetList(hueAssetList, atlasWidth, atlasHeight, assetLookup);

            /*
            Console.WriteLine("\n--- Sorted Hue Assets ---");
            foreach (var ha in sortedHueAssets)
            {
                Console.WriteLine($"Asset: {ha.AssetId}, Hue: {ha.Hue}");
            }
            */

            var separatedLists = ImageUtils.Separate(sortedHueAssets, hueBins, assetLookup);

            Console.WriteLine("\n--- Grouped by Hue ---");
            for (int i = 0; i < separatedLists.Count; i++)
            {
                Console.WriteLine($"Hue Group {i}: {string.Join(", ", separatedLists[i])}");
            }

            var builder = new AtlasBuilder();


            // Now generate the atlases based on the assets grouped by hue
            var atlases = builder.GenerateAtlases(separatedLists, assetLookup, atlasWidth, atlasHeight, hueBins);
#else
            var allAssetIds = assets.Select(a => a.Id).ToList();
            var singleList = new List<List<string>> { allAssetIds };

            var builder = new AtlasBuilder();

            var atlases = builder.NoSortGenerateAtlases(singleList, assetLookup, atlasWidth, atlasHeight, hueBins);

#endif

            // Save the atlas images as .png files
            builder.SaveAtlasesAsImages(atlases, "./output_atlases");

            stopwatch.Stop();

            Console.WriteLine($"Total time taken: {stopwatch.Elapsed.TotalSeconds} seconds");
        }
    }
}
