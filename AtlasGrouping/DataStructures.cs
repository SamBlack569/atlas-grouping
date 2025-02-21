using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtlasGrouping
{
    // Extended ImageAsset that now includes a color histogram.
    public class ImageAsset
    {
        public required string Id { get; set; }              // Unique identifier for the asset
        public required string FilePath { get; set; }        // File path to the image
        public int Width { get; set; }
        public int Height { get; set; }
        // Precomputed, normalized histogram for the image.
        // For example, a 256-bin histogram (or any other scheme) for the image’s color distribution.
        public required double[] ColorHistogram { get; set; }
        public List<string> GroupTags { get; set; } = new List<string>();
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

    // Represents a grouping of assets that should be packed together.
    public class AtlasGroup
    {
        public required string GroupId { get; set; }  // Unique identifier for the group
        public List<ImageAsset> Assets { get; set; } = new List<ImageAsset>();

        // Optionally, include any custom rules that apply specifically to this group.
        public List<CustomGroupingRule> GroupingRules { get; set; } = new List<CustomGroupingRule>();
    }

    // Represents a custom rule that forces certain assets to be grouped together.
    public class CustomGroupingRule
    {
        public string RuleId { get; set; }  // Unique identifier for the rule

        // List of asset IDs that must be placed in the same atlas.
        public List<string> RequiredAssetIds { get; set; } = new List<string>();

        // Additional properties (like priority or conditions) can be added as needed.
    }
}
