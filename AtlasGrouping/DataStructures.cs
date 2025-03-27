using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace AtlasGrouping
{
    // Extended ImageAsset that now includes a color histogram.
    public class ImageAsset
    {
        public required string Id { get; set; }              // Unique identifier for the asset
        public required string FilePath { get; set; }        // File path to the image
        public int Width { get; set; }
        public int Height { get; set; }
    }

    // Represents a single entry in an atlas.
    public class AtlasEntry
    {
        public required ImageAsset Asset { get; set; }
        public int X { get; set; }  // X coordinate in the atlas
        public int Y { get; set; }  // Y coordinate in the atlas
        public int Width { get; set; }  // Typically equal to Asset.Width
        public int Height { get; set; } // Typically equal to Asset.Height
    }

    // Represents a texture atlas that holds multiple image placements.
    public class TextureAtlas
    {
        public int AtlasWidth { get; set; }
        public int AtlasHeight { get; set; }
        public List<AtlasEntry> PlacedEntries { get; set; } = new List<AtlasEntry>();
    }

    // Represents a hue associated with an asset.
    public class HueAsset
    {
        public required double Hue { get; set; } // Hue value
        public required string AssetId { get; set; } // Unique identifier of the asset
    }
    
    public class Gap
    {
        public int Index { get; set; }
        public int Size { get; set; }
    }
}
