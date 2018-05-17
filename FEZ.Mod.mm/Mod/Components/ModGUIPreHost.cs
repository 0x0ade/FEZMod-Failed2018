using FezEngine.Components;
using FezEngine.Mod;
using FezEngine.Services;
using FezEngine.Services.Scripting;
using FezEngine.Structure;
using FezEngine.Tools;
using FezGame.Components;
using FezGame.Mod.Services;
using FezGame.Services;
using ImGuiNET;
using ImGuiXNA;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FezGame.Mod.Components {
    public class ModGUIPreHost : DrawableGameComponent {

        public ModGUIPreHost(Game game)
            : base(game) {
            UpdateOrder = -100000;
            DrawOrder = -100000;
        }

        public override void Draw(GameTime gameTime) {
            base.Draw(gameTime);

            if (!ModGUIHost.Instance.FinishedLoading)
                return;

            ModGUIHost.Instance.ImGuiState.NewFrame(gameTime);
        }

    }
}
