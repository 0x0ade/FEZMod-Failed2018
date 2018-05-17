using MonoMod.BaseLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FezEngine.Mod.Core {
    public class CoreModule : ModBase {

        public static CoreModule Instance;

        public CoreModule() {
            Instance = this;
        }

        public override Type SettingsType => typeof(CoreModuleSettings);
        public static CoreModuleSettings Settings => (CoreModuleSettings) Instance._Settings;

        public override void Load() {
        }

        public override void Unload() {
        }

    }
}
