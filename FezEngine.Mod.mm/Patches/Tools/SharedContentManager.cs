#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using Microsoft.Xna.Framework.Graphics;
using MonoMod.BaseLoader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FezEngine.Tools {
    class patch_SharedContentManager : SharedContentManager {

        public patch_SharedContentManager(string name)
            : base(name) {
            // no-op.
        }

        public class patch_CommonContentManager {

            private extern T orig_ReadAsset<T>(string assetName) where T : class;
            private T ReadAsset<T>(string assetName) where T : class {
                string modName = assetName.ToLowerInvariant().Replace('\\', '/');
                ModAsset mod;
                if (ModContentManager.TryGet<T>(modName, out mod)) {
                    if (typeof(T) == typeof(Texture2D)) {
                        using (Stream s = mod.Stream)
                            return Texture2D.FromStream(ServiceHelper.Game.GraphicsDevice, s) as T;
                    }
                }

                try {
                    return orig_ReadAsset<T>(assetName);
                } catch (Exception e) {
                    Logger.Log("FEZMod.Content", "orig_ReadAsset failed on " + assetName);
                    Logger.Log("FEZMod.Content", e.ToString());
                    throw;
                }
            }

        }

    }
}
