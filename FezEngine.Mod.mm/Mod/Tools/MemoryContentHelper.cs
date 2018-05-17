using System;
using System.Collections.Generic;
using System.IO;
using FezEngine.Structure;
using FezEngine.Services;
using FezEngine.Mod;
using Microsoft.Xna.Framework.Content;
using MonoMod;
using System.Reflection;
using FezEngine.Tools;
using MonoMod.BaseLoader;

namespace FezEngine.Tools {
    public static class MemoryContentHelper {

        public static Dictionary<string, ModAsset> Map = new Dictionary<string, ModAsset>();

        public static void IngestPack(string pathPak, bool precache = false, bool force = false) {
            if (!File.Exists(pathPak))
                return;
            using (FileStream stream = File.OpenRead(pathPak))
            using (BinaryReader reader = new BinaryReader(stream)) {
                int count = reader.ReadInt32();
                for (int i = 0; i < count; i++) {
                    string path = reader.ReadString().ToLowerInvariant().Replace('\\', '/');
                    int length = reader.ReadInt32();
                    // Note: Packs don't contain the file extensions.
                    // This affects SharedContentManager.ReadAsset, but MemoryContentManager.OpenStream keeps working.
                    if (!Map.ContainsKey(path) || force) {
                        if (precache) {
                            Map[path] = new MemoryAsset(reader.ReadBytes(length));
                        } else {
                            Map[path] = new PackedAsset(pathPak, stream.Position, length);
                            stream.Seek(length, SeekOrigin.Current);
                        }
                    } else {
                        stream.Seek(length, SeekOrigin.Current);
                    }
                }
            }
        }

    }
}