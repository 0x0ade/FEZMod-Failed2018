using FezEngine.Components;
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

        public ImVec3 ClearColor = new ImVec3(114f / 255f, 144f / 255f, 154f / 255f);

        public bool IsGameWindowOpen = false;

        private bool FinishedLoading = false;

        private RenderTargetHandle GameRT;
        private int GameRTImGui = -1;

        // private RenderTarget2D RT;

        public ModGUIHost(Game game)
            : base(game) {
            Instance = this;

            UpdateOrder = -100000;
            DrawOrder = 100000;

            ImGuiState = new ImGuiXNAState(game);

            if (!File.Exists("imgui.ini"))
                File.WriteAllText("imgui.ini", "");
        }

        public override void Initialize() {
            base.Initialize();

        }

        protected override void LoadContent() {
            base.LoadContent();

            DrawActionScheduler.Schedule(() => {
                ImGuiState.BuildTextureAtlas();

                FinishedLoading = true;
            });
        }

        public override void Update(GameTime gameTime) {
            base.Update(gameTime);

            if (!FinishedLoading)
                return;

            if (GameRT == null) {
                // Generate and schedule the render target in update - before anything renders.
                GameRT = TargetRenderingManager.TakeTarget();
                TargetRenderingManager.ScheduleHook(DrawOrder, GameRT.Target);
            }
            var prevGameRTImGui = GameRTImGui;
            GameRTImGui = ImGuiState.Register(GameRT.Target);
            if (prevGameRTImGui != -1 && prevGameRTImGui != GameRTImGui) {
                // If the ImGui texture got replaced, dispose the old texture.
                // This can happen if GameRT.Target got replaced when the window resizes.
                ImGuiState.GetTexture(prevGameRTImGui).Dispose();
                ImGuiState.Unregister(prevGameRTImGui);
            }
        }

        public override void Draw(GameTime gameTime) {
            base.Draw(gameTime);

            if (!FinishedLoading)
                return;

            if (GameRT != null) {
                TargetRenderingManager.Resolve(GameRT.Target, true);
                if (IsGameWindowOpen) {
                    GraphicsDevice.Clear(new Color(ClearColor.x, ClearColor.y, ClearColor.z, 1f));
                } else {
                    GraphicsDevice.SetBlendingMode(BlendingMode.Opaque);
                    TargetRenderingManager.DrawFullscreen(GameRT.Target);
                }
            }

            GraphicsDevice.PrepareStencilRead(CompareFunction.Always, StencilMask.None);
            GraphicsDevice.PrepareStencilWrite(StencilMask.None);

            /*
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
                    DepthFormat.None
                );
            }
            */

            // GraphicsDevice.SetRenderTarget(RT);

            GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;

            ImGuiState.NewFrame(gameTime);

            bool mouseInImGui = ImGui.IsMouseHoveringAnyWindow() || ImGui.IO.WantCaptureMouse;
            bool keyboardInImGui = ImGui.IO.WantCaptureKeyboard || ImGui.IO.WantTextInput;

            Game.IsMouseVisible &= mouseInImGui;
            if (MouseState is ModMouseStateManager)
                ((ModMouseStateManager) MouseState).ForceDisable = mouseInImGui;
            if (KeyboardState is ModKeyboardStateManager)
                ((ModKeyboardStateManager) KeyboardState).ForceDisable = keyboardInImGui;

            ImGuiLayout();
            ImGuiState.Render();

            // GraphicsDevice.SetRenderTarget(null);
            // TargetRenderingManager.DrawFullscreen(RT);
        }

        protected virtual void ImGuiLayout() {
            // Render to ImGui window. Works, but the render target is alpha-blended, causing minor issues.
            if (GameRT != null && IsGameWindowOpen) {
                // ImGui.SetNextWindowPos(new ImVec2(-8f, -8f), ImGuiCond.Always);
                // ImGui.SetNextWindowSize(new ImVec2(GameRT.Target.Width, GameRT.Target.Height), ImGuiCond.Always);
                ImGui.SetNextWindowSize(new ImVec2(GameRT.Target.Width / 2f, GameRT.Target.Height / 2f), ImGuiCond.Once);
                ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new ImVec2(0f, 0f));
                ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 0f);
                ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new ImVec2(0f, 0f));
                ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0f);
                ImGui.Begin(
                    "Game",
                    ref IsGameWindowOpen,
                    ImGuiWindowFlags.NoFocusOnAppearing |
                    ImGuiWindowFlags.NoScrollbar |
                    ImGuiWindowFlags.NoScrollWithMouse |
                    ImGuiWindowFlags.NoCollapse |
                    ImGuiWindowFlags.NoSavedSettings |
                    ImGuiWindowFlags.NoTitleBar |
                    ImGuiWindowFlags.NoMove |
                    ImGuiWindowFlags.NoResize
                );

                ImVec2 gameWindowSize = ImGui.GetWindowSize();

                ImGui.Image(
                    GameRTImGui,
                    new ImVec2(gameWindowSize.x, gameWindowSize.y),
                    new ImVec2(0f, 0f),
                    new ImVec2(1f, 1f),
                    new ImVec4(1f, 1f, 1f, 1f),
                    new ImVec4(0f, 0f, 0f, 0f)
                );

                ImGui.End();

                ImGui.PopStyleVar(4);
            }
        }

    }
}
