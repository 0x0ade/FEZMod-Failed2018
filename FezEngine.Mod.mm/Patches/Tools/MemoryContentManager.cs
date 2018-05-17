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

        private static List<string> assetNames;
        private static int assetNamesFromMetadata;
        private static int assetNamesFromCache;

        public static new IEnumerable<string> AssetNames {
            [MonoModReplace]
            get {
                if (assetNames != null &&
                    assetNamesFromMetadata == ModContentManager.Map.Count &&
                    assetNamesFromCache == MemoryContentHelper.Map.Count)
                    return assetNames;

                assetNames = new List<string>();

                assetNamesFromMetadata = ModContentManager.Map.Count;
                assetNames.AddRange(ModContentManager.Map.Keys);

                assetNamesFromCache = MemoryContentHelper.Map.Count;
                assetNames.AddRange(MemoryContentHelper.Map.Keys);

                assetNames = assetNames.Distinct().ToList();

                return assetNames;
            }
        }

        public patch_MemoryContentManager(IServiceProvider serviceProvider, string rootDirectory)
            : base(serviceProvider, rootDirectory) {
            // no-op.
        }

        [MonoModReplace]
        protected override Stream OpenStream(string assetName) {
            assetName = assetName.ToLowerInvariant().Replace('\\', '/');
            ModAsset modAsset;

            if (ModContentManager.TryGet(assetName, out modAsset))
                return modAsset.Stream;

            if (MemoryContentHelper.Map.TryGetValue(assetName, out modAsset))
                return modAsset.Stream;

            throw new FileNotFoundException($"Asset not found: {assetName}");
        }

        [MonoModReplace]
        public static new bool AssetExists(string assetName) {
            assetName = assetName.ToLowerInvariant().Replace('\\', '/');

            if (ModContentManager.Get(assetName) != null)
                return true;

            if (MemoryContentHelper.Map.ContainsKey(assetName))
                return true;

            return false;
        }

        [MonoModReplace]
        public new void LoadEssentials() {
            MemoryContentHelper.IngestPack(Path.Combine(RootDirectory, "Essentials.pak"), precache: true);
        }

        [MonoModReplace]
        public new void Preload() {
            MemoryContentHelper.IngestPack(Path.Combine(RootDirectory, "Updates.pak"), precache: true);
            // Game originally precaches Other.pak - let's just scan it instead.
            MemoryContentHelper.IngestPack(Path.Combine(RootDirectory, "Other.pak"), precache: false);
        }

    }
}
