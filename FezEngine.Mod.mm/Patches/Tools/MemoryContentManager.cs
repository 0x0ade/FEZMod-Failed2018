#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using FezEngine.Mod;
using FezEngine.Mod.Core;
using Microsoft.Xna.Framework.Graphics;
using MonoMod;
using MonoMod.BaseLoader;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FezEngine.Tools {
    class patch_MemoryContentManager : MemoryContentManager {

        private static Dictionary<string, byte[]> cachedAssets;

        private static List<string> assetNames;
        private static int assetNamesFromMetadata;
        private static int assetNamesFromCache;

        public static new IEnumerable<string> AssetNames {
            get {
                if (assetNames != null && assetNamesFromMetadata == ModContentManager.Map.Count)
                    return assetNames;

                assetNames = new List<string>();

                assetNamesFromMetadata = ModContentManager.Map.Count;
                assetNames.AddRange(ModContentManager.Map.Keys);

                assetNamesFromCache = cachedAssets.Count;
                assetNames.AddRange(cachedAssets.Keys);

                assetNames = assetNames.Distinct().ToList();

                return assetNames;
            }
        }

        public patch_MemoryContentManager(IServiceProvider serviceProvider, string rootDirectory)
            : base(serviceProvider, rootDirectory) {
            // no-op.
        }

        protected extern Stream orig_OpenStream(string assetName);
        protected override Stream OpenStream(string assetName) {
            byte[] data;
            if (AssetDataCache.Persistent.TryGetValue(assetName, out data))
                return new MemoryStream(data);

            ModAsset modAsset;
            if (ModContentManager.TryGet(assetName.ToLowerInvariant().Replace('\\', '/'), out modAsset))
                return modAsset.Stream;

            return orig_OpenStream(assetName);
        }

        public static extern bool orig_AssetExists(string assetName);
        public static new bool AssetExists(string assetName) {
            if (ModContentManager.Get(assetName.ToLowerInvariant().Replace('\\', '/')) != null)
                return true;

            return orig_AssetExists(assetName);
        }

        public extern void orig_LoadEssentials();
        public new void LoadEssentials() {
            AssetDataCache.PackPrecache(Path.Combine(RootDirectory, "Essentials.pak"));
            AssetDataCache.PackPrecache(Path.Combine(RootDirectory, "Updates.pak"));
        }

        public extern void orig_Preload();
        public new void Preload() {
            AssetDataCache.PackScanMeta(Path.Combine(RootDirectory, "Other.pak"));
        }

    }
}
