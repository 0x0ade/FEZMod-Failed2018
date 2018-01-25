using FezEngine.Mod;
using FezEngine.Tools;
using FezGame.Mod.Components;
using FezGame.Mod.Services;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FezGame.Mod {
    public static class FezMod {

        public static double GameTimeScale = 1f;
        public static GameTime GameTimeUpdate { get; internal set; }
        public static GameTime GameTimeDraw { get; internal set; }

        public static void LoadComponentReplacements(Fez game) {
            ServiceHelperHooks.ReplacementServices["FezEngine.Services.MouseStateManager"] = new ModMouseStateManager();
        }

        public static void LoadComponents(Fez game) {
            ServiceHelper.AddComponent(new ModGUIHost(game));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void InvokeGL(this Action a) {
            DrawActionScheduler.Schedule(a);
        }

        public static T CastDelegate<T>(this Delegate source) where T : class {
            Type type = typeof(T);
            Delegate[] delegates = source.GetInvocationList();
            if (delegates.Length == 1)
                return Delegate.CreateDelegate(type, delegates[0].Target, delegates[0].Method) as T;
            Delegate[] delegatesDest = new Delegate[delegates.Length];
            for (int i = 0; i < delegates.Length; i++)
                delegatesDest[i] = CastDelegate<T>(delegates[i]) as Delegate;
            return Delegate.Combine(delegatesDest) as T;
        }

    }
}
