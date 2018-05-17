using FezEngine.Tools;
using MonoMod.BaseLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FezEngine.Mod.Core {
    public class CoreModuleSettings : ModSettings {

        public DataCacheMode DataCache { get; set; } = DataCacheMode.Default;
        public MusicCacheMode MusicCache { get; set; } = MusicCacheMode.Default;

    }
}
