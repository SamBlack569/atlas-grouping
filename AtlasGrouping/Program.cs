using System.Text.Json;

namespace AtlasGrouping
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("---- Atlas Builder ----");


            string imageFolder = "./images";
            List<ImageAsset> assets = ImageUtils.LoadImageAssetsFromDirectory(imageFolder);

            // Save to JSON, just because we can
            string json = JsonSerializer.Serialize(assets, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText("image_assets.json", json);
            Console.WriteLine("Saved ImageAsset list to image_assets.json");


            foreach (var asset in assets)
            {
                Console.WriteLine($"Loaded: {asset.Id}, Size: {asset.Width}x{asset.Height}, ColorInfo: {"Nothing yet"}");
            }


            var builder = new AtlasBuilder();

            builder.RunGenerationAndMetrics(assets, 2048,2048);
        }
    }
}
