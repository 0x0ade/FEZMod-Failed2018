#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using Common;
using FezGame.Mod;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.BaseLoader;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FezGame {
    class patch_Program {

        private static extern void orig_Main(string[] args);
        private static void Main(string[] args) {
            Fez.Version = $"{Fez.Version} | FEZMod.neo {FezMod.VersionString}";

            FezMod.Boot(args);

            orig_Main(args);
        }

        private static extern void orig_MainInternal();
        private static void MainInternal() {
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            try {
                orig_MainInternal();
            } catch (Exception e) {
                Common.Logger.Log("FEZMod", "Fatal error!");
                e.LogDetailed();
                throw;
            }
        }

    }
}
