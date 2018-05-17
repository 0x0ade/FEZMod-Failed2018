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
    public static class AssetDataCache {

        public static Dictionary<string, byte[]> Persistent = new Dictionary<string, byte[]>();

        public static void PackPrecache(string pathPak) {
            if (!File.Exists(pathPak))
                return;
            using (FileStream packStream = File.OpenRead(pathPak))
            using (BinaryReader packReader = new BinaryReader(packStream)) {
                int count = packReader.ReadInt32();
                for (int i = 0; i < count; i++) {
                    string path = packReader.ReadString();
                    int length = packReader.ReadInt32();
                    Persistent[path] = packReader.ReadBytes(length);
                }
            }
        }

        public static void PackScanMeta(string pathPak) {
            if (!File.Exists(pathPak))
                return;
            using (FileStream stream = File.OpenRead(pathPak))
            using (BinaryReader reader = new BinaryReader(stream)) {
                int count = reader.ReadInt32();
                for (int i = 0; i < count; i++) {
                    string path = reader.ReadString();
                    int length = reader.ReadInt32();
                    // Note: Packs don't contain the file extensions.
                    // This affects SharedContentManager.ReadAsset, but MemoryContentManager.OpenStream keeps working.
                    if (!ModContentManager.Map.ContainsKey(path)) {
                        ModContentManager.Add(path, new PackModAsset(pathPak, stream.Position, length));
                    }
                    stream.Seek(length, SeekOrigin.Current);
                }
            }
        }

    }
}