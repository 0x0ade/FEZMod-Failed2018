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
    public class ModGUIHost : DrawableGameComponent {

        public static ModGUIHost Instance;

        [ServiceDependency] public IMouseStateManager MouseState { get; set; }
        [ServiceDependency] public IInputManager InputManager { get; set; }
        [ServiceDependency] public IKeyboardStateManager KeyboardState { get; set; }
        [ServiceDependency] public IGameService GameService { get; set; }
        [ServiceDependency] public IGameStateManager GameState { get; set; }
        [ServiceDependency] public IGameCameraManager CameraManager { get; set; }
        [ServiceDependency] public IFontManager FontManager { get; set; }
        [ServiceDependency] public IPlayerManager PlayerManager { get; set; }
        [ServiceDependency] public IGameLevelManager LevelManager { get; set; }
        [ServiceDependency] public ILevelMaterializer LevelMaterializer { get; set; }
        [ServiceDependency] public IContentManagerProvider CMProvider { get; set; }
        [ServiceDependency] public ITargetRenderingManager TargetRenderingManager { get; set; }

        public readonly ImGuiXNAState ImGuiState;

        public bool FinishedLoading { get; private set; } = false;

        private RenderTarget2D RT;
        private SpriteBatch SpriteBatch;

        public ModGUIHost(Game game)
            : base(game) {
            Instance = this;

            UpdateOrder = -100000;
            DrawOrder = 100000;

            ImGuiState = new ImGuiXNAState(game);

            if (!File.Exists("imgui.ini"))
                File.WriteAllText("imgui.ini", "");

            ServiceHelper.AddComponent(new ModGUIPreHost(game));
        }

        protected override void LoadContent() {
            base.LoadContent();

            DrawActionScheduler.Schedule(() => {
                ImGuiState.BuildTextureAtlas();

                SpriteBatch = new SpriteBatch(GraphicsDevice);

                FinishedLoading = true;
            });
        }

        public override void Draw(GameTime gameTime) {
            base.Draw(gameTime);

            if (!FinishedLoading)
                return;

            GraphicsDevice.PrepareStencilRead(CompareFunction.Always, StencilMask.None);
            GraphicsDevice.PrepareStencilWrite(StencilMask.None);

            if (RT == null ||
                RT.Width != GraphicsDevice.PresentationParameters.BackBufferWidth ||
                RT.Height != GraphicsDevice.PresentationParameters.BackBufferHeight
            ) {
                if (RT != null)
                    RT.Dispose();
                RT = new RenderTarget2D(
                    GraphicsDevice,
                    GraphicsDevice.PresentationParameters.BackBufferWidth,
                    GraphicsDevice.PresentationParameters.BackBufferHeight,
                    false,
                    SurfaceFormat.Color,
                    DepthFormat.None,
                    0,
                    RenderTargetUsage.PreserveContents
                );
            }

            RenderTargetBinding[] prevRT = FNAHooks.PrevRenderTargets;
            GraphicsDevice.SetRenderTarget(RT);
            GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;

            GraphicsDevice.Clear(Color.Transparent);

            // NewFrame in ModGUIPreHost.Draw
            ImGuiLayout();

            bool mouseInImGui = ImGui.IsMouseHoveringAnyWindow() || ImGui.IO.WantCaptureMouse;
            bool keyboardInImGui = ImGui.IO.WantCaptureKeyboard || ImGui.IO.WantTextInput;

            Game.IsMouseVisible &= mouseInImGui;
            if (MouseState is ModMouseStateManager)
                ((ModMouseStateManager) MouseState).ForceDisable = mouseInImGui;
            if (KeyboardState is ModKeyboardStateManager)
                ((ModKeyboardStateManager) KeyboardState).ForceDisable = keyboardInImGui;

            ImGuiState.Render();

            FNAHooks.SkipClear++;
            GraphicsDevice.SetRenderTargets(prevRT);
            TargetRenderingManager.DrawFullscreen(RT);
        }

        protected virtual void ImGuiLayout() {
        }

    }
}
