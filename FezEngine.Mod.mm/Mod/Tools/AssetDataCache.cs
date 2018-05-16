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
        public static Dictionary<string, DataCacheItem> Temporary = new Dictionary<string, DataCacheItem>();
        public static Dictionary<string, byte[]> Preloaded = new Dictionary<string, byte[]>();

        public static void CachePackPersistent(string pathPak) {
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

        public static void ScanPackMetadata(string pathPak) {
            if (!File.Exists(pathPak))
                return;
            using (FileStream stream = File.OpenRead(pathPak))
            using (BinaryReader reader = new BinaryReader(stream)) {
                int count = reader.ReadInt32();
                for (int i = 0; i < count; i++) {
                    string path = reader.ReadString();
                    int length = reader.ReadInt32();
                    // Note: The original packs don't contain the file extensions.
                    // This affects SharedContentManager.ReadAsset, but MemoryContentManager.OpenStream keeps working.
                    ModContentManager.Add(path, new PackModAsset(pathPak, stream.Position, length));
                }
            }
        }

    }

    public class DataCacheItem {
        public byte[] Data;
        public int References;
        public int Age;
    }

    public enum DataCacheMode {
        Default,
        Disabled,
        Smart
    }

    public enum MusicCacheMode {
        Default,
        Disabled,
        Enabled
    }
}