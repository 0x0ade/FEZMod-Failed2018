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
    public class MemoryAsset : ModAsset {

        public byte[] _Data;
        public override byte[] Data => _Data;

        public MemoryAsset(byte[] data) {
            _Data = data;
        }

        protected override void Open(out Stream stream, out bool isSection) {
            stream = new MemoryStream(_Data, 0, _Data.Length, false, true);
            isSection = true;
        }

    }
}
