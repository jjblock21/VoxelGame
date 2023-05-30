using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using VoxelGame.Framework;
using VoxelGame.Framework.Logging;

namespace VoxelGame.Game
{
    public class McWindow : GameWindow
    {
        private const int STARTUP_WIDTH = 1280;
        private const int STARTUP_HEIGHT = 720;
        private const int VERSION_MAJOR = 4;
        private const int VERSION_MINOR = 5;

        private Minecraft? _minecraft;
        private FrameCounter _frameCounter;

        public static readonly DefaultLogger Logger;

        static McWindow()
        {
#if DEBUG
            Logger = new DefaultLogger(0);
#else
            // Print only info, waring and error logs.
            Logger = new DefaultLogger(1);
#endif
        }

        // Dont run code in here as exceptions will not be handled correctly.
        public McWindow() : base(
            GameWindowSettings.Default,
            new NativeWindowSettings()
            {
                Size = new Vector2i(STARTUP_WIDTH, STARTUP_HEIGHT),
                APIVersion = new Version(VERSION_MAJOR, VERSION_MINOR),
                StartVisible = false,
                Vsync = VSyncMode.On
            })
        {
            _frameCounter = new FrameCounter(1f);
        }

        #region Public Methods

        /// <summary>
        /// Turns Fullscreen mode on/off.
        /// </summary>
        public void SetFullscreen(bool fullscreen)
        {
            if (fullscreen)
            {
                _prevWindowState = WindowState;
                WindowState = WindowState.Fullscreen;
            }
            else WindowState = _prevWindowState;
        }

        /// <summary>
        /// Turns wireframe mode on/off.
        /// </summary>
        public void SetWireframe(bool wireframe)
        {
            GL.PolygonMode(MaterialFace.FrontAndBack, wireframe ? PolygonMode.Line : PolygonMode.Fill);
            if (wireframe) GL.Disable(EnableCap.Blend);
            else GL.Enable(EnableCap.Blend);
        }

        /// <summary>
        /// Framerate the game is currently running at.
        /// </summary>
        public int FrameRate => _frameCounter.FrameRate;

        #endregion

        protected override void OnLoad()
        {
            unsafe { Input.Init(WindowPtr, MouseState, KeyboardState); }
            Input.SetCursorLocked(true);

            CenterWindow();

            Logger.Info($"Version: {API} {APIVersion}");

            _minecraft = new Minecraft(this);
            _minecraft!.Init();
            IsVisible = true;
            _minecraft.Load();

            base.OnLoad();
        }

        protected override void OnUnload()
        {
            _minecraft!.Unload();
            base.OnUnload();
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            _frameCounter.Sample(args.Time);

            _minecraft!.RenderScene(args.Time);
            GL.Disable(EnableCap.DepthTest);
            _minecraft!.RenderOverlay(args.Time);
            GL.Enable(EnableCap.DepthTest);

            Context.SwapBuffers();
            ErrorHandler.CheckGLErrors();

            base.OnRenderFrame(args);
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            _minecraft!.UpdateFrame(args.Time);
            base.OnUpdateFrame(args);
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            _minecraft!.OnResize(e);
            base.OnResize(e);
        }

        private WindowState _prevWindowState;

        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            if (e.Key == Keys.F11)
            {
                SetFullscreen(!IsFullscreen);
                return;
            }
            Input.CallKeyDown(e);

            base.OnKeyDown(e);
        }

        protected override void OnKeyUp(KeyboardKeyEventArgs e)
        {
            Input.CallKeyUp(e);
            base.OnKeyUp(e);
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            Input.CallMouseDown(e);
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            Input.CallMouseUp(e);
            base.OnMouseUp(e);
        }
    }
}
