using System.Text.Json;

namespace AtlasGrouping
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("---- Atlas Builder ----");


            string imageFolder = "./images";

            int atlasWidth = 2048;
            int atlasHeight = 2048;
            int numberOfRanges = 6; // Number of hue ranges to split the assets into

            List<ImageAsset> assets = ImageUtils.LoadImageAssetsFromDirectory(imageFolder);

            // Save to JSON, just because we can
            string json = JsonSerializer.Serialize(assets, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText("image_assets.json", json);
            Console.WriteLine("Saved ImageAsset list to image_assets.json");

            // Create a lookup dictionary for assets
            var assetLookup = assets.ToDictionary(a => a.Id, a => a);

            var hueAssetList = new List<HueAsset>();

            foreach (var asset in assets)
            {
                int hue = ImageUtils.Histogram(asset);
                Console.WriteLine($"Loaded: {asset.Id}, Size: {asset.Width}x{asset.Height}, Hue dominant: {hue}");

                hueAssetList.Add(new HueAsset { AssetId = asset.Id, Hue = hue });
            }

            // Sort the assets by hue
            var sortedHueAssets = ImageUtils.SortHueAssetList(hueAssetList, atlasWidth, atlasHeight, assetLookup);

            Console.WriteLine("\n--- Sorted Hue Assets ---");
            foreach (var ha in sortedHueAssets)
            {
                Console.WriteLine($"Asset: {ha.AssetId}, Hue: {ha.Hue}");
            }

            var separatedLists = ImageUtils.Separate(sortedHueAssets, numberOfRanges);

            Console.WriteLine("\n--- Grouped by Hue Range ---");
            for (int i = 0; i < separatedLists.Count; i++)
            {
                Console.WriteLine($"Range {i}: {string.Join(", ", separatedLists[i])}");
            }

            var builder = new AtlasBuilder();

            builder.RunGenerationAndMetrics(assets, 2048,2048);
        }
    }
}
