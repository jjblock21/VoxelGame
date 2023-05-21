using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
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

        #region Static Logger
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
        #endregion

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
        { }

        private void Init()
        {
            unsafe { Input.Init(WindowPtr, MouseState, KeyboardState); }
            Input.SetCursorLocked(true);

            CenterWindow();

            Logger.Info($"Version: {API} {APIVersion}");
            _minecraft = new Minecraft(this);
        }

        public void SetFullscreen(bool fullscreen)
        {
            if (fullscreen)
            {
                _prevWindowState = WindowState;
                WindowState = WindowState.Fullscreen;
            }
            else WindowState = _prevWindowState;
        }

        protected override void OnLoad()
        {
            Init();
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
            _minecraft!.RenderFrame(args.Time);
            Context.SwapBuffers();
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
            switch (e.Key)
            {
                case Keys.F11:
                    SetFullscreen(!IsFullscreen);
                    break;
                default:
                    Input.CallKeyDown(e);
                    break;
            }
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
