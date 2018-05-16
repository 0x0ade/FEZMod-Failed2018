using FezEngine.Mod;
using FezEngine.Tools;
using FezGame.Mod.Components;
using FezGame.Mod.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.BaseLoader;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FezGame.Mod {
    public static class FezMod {

        public readonly static string VersionString = "0.0.0-dev";

        public static double GameTimeScale = 1f;
        public static GameTime GameTimeUpdate { get; internal set; }
        public static GameTime GameTimeDraw { get; internal set; }

        internal static void Boot(params string[] args) {
            ModContentManager.OnGuessType += ContentGuessType;

            ModManager.Boot("FEZMod", VersionString);

            string modContent = Path.Combine(ModManager.PathGame, "ModContent");
            if (!Directory.Exists(modContent))
                Directory.CreateDirectory(modContent);
            ModContentManager.Crawl(new DirectoryModContent(modContent));
            ModContentManager.Crawl(new AssemblyModContent(Assembly.GetEntryAssembly()));

            Queue<string> queue = new Queue<string>(args);
            while (queue.Count > 0) {
                string arg = queue.Dequeue();
                
                if (arg == "--dump-all") {
                    DumpAllPacks();
                }
            }

        }

        private static string ContentGuessType(string file, out Type type, out string format) {
            type = typeof(object);
            format = Path.GetExtension(file) ?? "";
            if (format.Length >= 1)
                format = format.Substring(1);

            if (file.EndsWith(".png")) {
                type = typeof(Texture2D);
                file = file.Substring(0, file.Length - 4);
            }

            return null;
        }

        internal static void LoadComponentReplacements(Fez game) {
            ServiceHelperHooks.ReplacementServices["FezEngine.Services.MouseStateManager"] = new ModMouseStateManager();
            ServiceHelperHooks.ReplacementServices["FezEngine.Services.KeyboardStateManager"] = new ModKeyboardStateManager();
        }

        internal static void LoadComponents(Fez game) {
            ServiceHelper.AddComponent(new ModGUIHost(game));
        }

        internal static void DumpAllPacks() {
            string pathPakDir = Path.Combine(ModManager.PathGame, "Content");
            if (!Directory.Exists(pathPakDir))
                return;
            foreach (string pathPak in Directory.GetFiles(pathPakDir)) {
                if (!pathPak.EndsWith(".pak"))
                    continue;
                DumpPack(pathPak);
            }
        }

        internal static void DumpPack(string pathPak) {
            if (!File.Exists(pathPak))
                return;

            string pathOutRoot = Path.Combine(ModManager.PathGame, "ModDUMP", Path.GetFileNameWithoutExtension(pathPak));

            using (FileStream packStream = File.OpenRead(pathPak))
            using (BinaryReader packReader = new BinaryReader(packStream)) {
                int count = packReader.ReadInt32();
                for (int i = 0; i < count; i++) {
                    string path = packReader.ReadString();
                    int length = packReader.ReadInt32();

                    // The FEZ 1.12 .pak files store the raw fxb files, which should be dumped with the fxb extensions.
                    if (path.StartsWith("effects")) {
                        path += ".fxb";
                    } else {
                        path += ".xnb";
                    }
                    string pathOut = Path.Combine(pathOutRoot, path.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar));
                    string pathOutDir = Path.GetDirectoryName(pathOut);

                    if (!Directory.Exists(pathOutDir))
                        Directory.CreateDirectory(pathOutDir);

                    Console.WriteLine($"Dumping {pathOut}");
                    using (FileStream dumpStream = File.OpenWrite(pathOut))
                        dumpStream.Write(packReader.ReadBytes(length), 0, length);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void InvokeGL(this Action a) {
            DrawActionScheduler.Schedule(a);
        }

    }
}
