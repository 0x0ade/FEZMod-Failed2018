#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoMod;
using Microsoft.Xna.Framework;
using FezGame.Mod;
using System.Reflection;
using Common;
using FezEngine.Tools;
using FezGame.Components;
using FezEngine.Components;
using MonoMod.BaseLoader;

namespace FezGame {
    class patch_Fez : Fez {

        private PropertyInfo property_GameTime_ElapsedGameTime;
        private PropertyInfo property_GameTime_TotalGameTime;

        private GameTime _MulGameTime(ref GameTime gameTime) {
            double scale = FezMod.GameTimeScale;
            if (scale == 1d) {
                return gameTime;
            }

            if (property_GameTime_ElapsedGameTime == null) {
                property_GameTime_ElapsedGameTime = gameTime.GetType().GetProperty("ElapsedGameTime", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            }
            if (property_GameTime_TotalGameTime == null) {
                property_GameTime_TotalGameTime = gameTime.GetType().GetProperty("TotalGameTime", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            }

            TimeSpan egt = gameTime.ElapsedGameTime;
            TimeSpan tgt = gameTime.TotalGameTime;
            tgt -= egt;
            egt = TimeSpan.FromTicks((long) (egt.Ticks * scale));
            tgt += egt;

            ReflectionHelper.SetValue(property_GameTime_ElapsedGameTime, gameTime, egt);
            ReflectionHelper.SetValue(property_GameTime_TotalGameTime, gameTime, tgt);

            return gameTime;
        }

        public extern void orig_Update(GameTime gameTime);
        protected override void Update(GameTime gameTime) {
            _MulGameTime(ref gameTime);
            FezMod.GameTimeUpdate = gameTime;
            orig_Update(gameTime);
        }

        public extern void orig_Draw(GameTime gameTime);
        protected override void Draw(GameTime gameTime) {
            _MulGameTime(ref gameTime);
            FezMod.GameTimeDraw = gameTime;
            orig_Draw(gameTime);
        }

        public extern void orig_Initialize();
        protected override void Initialize() {
            FezMod.LoadComponentReplacements(this);
            ModManager.Initialize();
            orig_Initialize();
        }

        public static extern void orig_LoadComponents(Fez game);
        public static void LoadComponents(Fez game) {
            if (ServiceHelper.FirstLoadDone)
                return;
            orig_LoadComponents(game);
            FezMod.LoadComponents(game);
            ServiceHelper.FirstLoadDone = true;
        }

    }
}
