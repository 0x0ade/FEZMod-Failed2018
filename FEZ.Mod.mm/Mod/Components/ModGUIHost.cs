using FezEngine.Components;
using FezEngine.Services;
using FezEngine.Services.Scripting;
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

        [ServiceDependency]
        public IMouseStateManager MouseState { get; set; }
        [ServiceDependency]
        public IInputManager InputManager { get; set; }
        [ServiceDependency]
        public IKeyboardStateManager KeyboardState { get; set; }
        [ServiceDependency]
        public IGameService GameService { get; set; }
        [ServiceDependency]
        public IGameStateManager GameState { get; set; }
        [ServiceDependency]
        public IGameCameraManager CameraManager { get; set; }
        [ServiceDependency]
        public IFontManager FontManager { get; set; }
        [ServiceDependency]
        public IPlayerManager PlayerManager { get; set; }
        [ServiceDependency]
        public IGameLevelManager LevelManager { get; set; }
        [ServiceDependency]
        public ILevelMaterializer LevelMaterializer { get; set; }
        [ServiceDependency]
        public IContentManagerProvider CMProvider { get; set; }
        [ServiceDependency]
        public ITargetRenderingManager TargetRenderingManager { get; set; }

        public readonly ImGuiXNAState ImGuiState;

        public ImVec3 ClearColor = new ImVec3(114f / 255f, 144f / 255f, 154f / 255f);

        private bool FinishedLoading = false;

        private RenderTargetHandle GameRT;
        private int GameRTImGui = -1;

        public ModGUIHost(Game game)
            : base(game) {
            Instance = this;

            UpdateOrder = -10000;
            DrawOrder = 10000;

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
                // GraphicsDevice.Clear(new Color(ClearColor.x, ClearColor.y, ClearColor.z, 1f));
                GraphicsDevice.SetBlendingMode(BlendingMode.Opaque);
                TargetRenderingManager.DrawFullscreen(GameRT.Target);
            }

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
        }

        float f = 0.0f;
        bool show_test_window = true;
        bool show_another_window = false;
        protected virtual void ImGuiLayout() {
            // Render to ImGui window. Works, but the render target is alpha-blended, causing minor issues.
            /*
            if (GameRT != null) {
                ImGui.SetNextWindowPos(new ImVec2(-8f, -8f), ImGuiCond.Always);
                ImGui.SetNextWindowSize(new ImVec2(GameRT.Target.Width + 8f, GameRT.Target.Height + 12f), ImGuiCond.Always);
                ImGui.Begin(
                    "Game",
                    ImGuiWindowFlags.NoScrollbar |
                    ImGuiWindowFlags.NoScrollWithMouse |
                    ImGuiWindowFlags.NoCollapse |
                    ImGuiWindowFlags.NoResize |
                    ImGuiWindowFlags.NoFocusOnAppearing |
                    ImGuiWindowFlags.NoMove |
                    ImGuiWindowFlags.NoResize |
                    ImGuiWindowFlags.NoBringToFrontOnFocus |
                    ImGuiWindowFlags.NoSavedSettings |
                    ImGuiWindowFlags.NoTitleBar
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
            }
            */

            // 1. Show a simple window
            // Tip: if we don't call ImGui.Begin()/ImGui.End() the widgets appears in a window automatically called "Debug"
            {
                ImGui.Text("Hello, world!");
                ImGui.SliderFloat("float", ref f, 0.0f, 1.0f, null, 1f);
                ImGui.ColorEdit3("clear color", ref ClearColor, false);
                if (ImGui.Button("Test Window")) show_test_window = !show_test_window;
                if (ImGui.Button("Another Window")) show_another_window = !show_another_window;
                ImGui.Text(string.Format("Application average {0:F3} ms/frame ({1:F1} FPS)", 1000f / ImGui.GetIO().Framerate, ImGui.GetIO().Framerate));
            }

            // 2. Show another simple window, this time using an explicit Begin/End pair
            if (show_another_window) {
                ImGui.SetNextWindowSize(new ImVec2(200, 100), ImGuiCond.FirstUseEver);
                ImGui.Begin("Another Window", ref show_another_window);
                ImGui.Text("Hello");
                ImGui.End();
            }

            // 3. Show the ImGui test window. Most of the sample code is in ImGui.ShowTestWindow()
            if (show_test_window) {
                ImGui.SetNextWindowPos(new ImVec2(650, 20), ImGuiCond.FirstUseEver);
                ImGui.ShowTestWindow(ref show_test_window);
            }
        }

    }
}
