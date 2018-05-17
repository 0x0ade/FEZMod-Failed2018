using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FezEngine.Mod {
    public static class FNAHooks {

        public static RenderTargetBinding[] PrevRenderTargets;

        public static int SkipClear;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Initialize() {
            // Just let the class constructor run for now.
            // Maybe automate this using custom attributes once this becomes too large.
        }

        private static Hook h_SetRenderTargets = new Hook(
            typeof(GraphicsDevice).GetMethod("SetRenderTargets", BindingFlags.Public | BindingFlags.Instance, null, new Type[] {
                typeof(RenderTargetBinding[])
            }, null),
            typeof(FNAHooks).GetMethod("SetRenderTargets")
        );
        public static void SetRenderTargets(Action<GraphicsDevice, RenderTargetBinding[]> orig, GraphicsDevice self, params RenderTargetBinding[] renderTargets) {
            PrevRenderTargets = (RenderTargetBinding[]) renderTargets?.Clone();
            orig(self, renderTargets);
        }

        private static Hook h_Clear = new Hook(
            typeof(GraphicsDevice).GetMethod("Clear", BindingFlags.Public | BindingFlags.Instance, null, new Type[] {
                typeof(ClearOptions), typeof(Vector4), typeof(float), typeof(int)
            }, null),
            typeof(FNAHooks).GetMethod("Clear")
        );
        public static void Clear(Action<GraphicsDevice, ClearOptions, Vector4, float, int> orig, GraphicsDevice self, ClearOptions options, Vector4 color, float depth, int stencil) {
            if (SkipClear > 0) {
                SkipClear--;
                return;
            }
            orig(self, options, color, depth, stencil);
        }

    }
}
