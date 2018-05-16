using FezEngine.Tools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.BaseLoader;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FezEngine.Mod {
    public class PackModAsset : ModAsset {

        public string PathPak;
        public long Position;
        public int Length;

        public PackModAsset(string pathPak, long position, int length) {
            PathPak = pathPak;
            Position = position;
            Length = length;
        }

        protected override void Open(out Stream stream, out bool isSection) {
            stream = new LimitedStream(File.OpenRead(PathPak), Position, Length);
            isSection = true;
        }

    }
}
