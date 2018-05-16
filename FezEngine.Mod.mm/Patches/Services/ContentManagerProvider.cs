#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using FezEngine.Mod;
using FezEngine.Tools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod;
using MonoMod.BaseLoader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FezEngine.Services {
    class patch_ContentManagerProvider : ContentManagerProvider {

        private readonly Dictionary<string, SharedContentManager> levelScope;

        private readonly List<string> levelScopeToRemove = new List<string>();
        private readonly List<string> temporaryToRemove = new List<string>();

        private readonly List<string> precaching = new List<string>();

        [MonoModIgnore]
        public new ILevelManager LevelManager { get; set; }

        public patch_ContentManagerProvider(Game game)
            : base(game) {
            // no-op.
        }

        private extern void orig_CleanAndPrecache();
        private void CleanAndPrecache() {
            if (FezEngineMod.DataCache != DataCacheMode.Smart) {
                orig_CleanAndPrecache();
                return;
            }

            precaching.Clear();

            IEnumerable<string> linked = LevelManager.LinkedLevels();

            foreach (string key in levelScope.Keys) {
                if (key != LevelManager.Name && !linked.Contains(key)) {
                    levelScope[key].Dispose();
                    levelScopeToRemove.Add(key);
                }
            }
            foreach (string key in levelScopeToRemove) {
                levelScope.Remove(key);
            }
            levelScopeToRemove.Clear();

            foreach (KeyValuePair<string, DataCacheItem> pair in AssetDataCache.Temporary) {
                if (pair.Value.References <= 1) {
                    pair.Value.Age++;
                }

                if (pair.Value.Age > 2) {
                    temporaryToRemove.Add(pair.Key);
                }
            }
            foreach (string key in temporaryToRemove) {
                AssetDataCache.Temporary.Remove(key);
            }
            temporaryToRemove.Clear();

            AssetDataCache.Preloaded.Clear();
            // TODO: Schedule preloads for AssetDataCache.Preloaded
        }

    }
}
