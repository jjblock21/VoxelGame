using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace VoxelGame.Framework
{
    /// <summary>
    /// Class to make input events accessible from everywhere.
    /// </summary>
    public static class Input
    {
        public static event KeyboardEvent? OnKeyPressed;
        public static event KeyboardEvent? OnKeyReleased;
        public static event MouseEvent? OnMouseClicked;
        public static event MouseEvent? OnMouseReleased;

        public static Vector2 MouseDelta => _mouseState == null ? Vector2.Zero : _mouseState.Delta;
        public static float ScrollDelta => _mouseState == null ? 0 : _mouseState.ScrollDelta.Y;

        private static MouseState? _mouseState;
        private static KeyboardState? _keyboardState;

        private static unsafe Window* _windowPtr;

        public static unsafe void Init(Window* window, MouseState mouseState, KeyboardState keyboardState)
        {
            _windowPtr = window;
            _mouseState = mouseState;
            _keyboardState = keyboardState;
        }

        public static unsafe void SetCursorLocked(bool locked)
        {
            if (_windowPtr is null) return;
            CursorModeValue m = locked ? CursorModeValue.CursorDisabled : CursorModeValue.CursorNormal;
            GLFW.SetInputMode(_windowPtr, CursorStateAttribute.Cursor, m);
        }

        // Probably not the best idea to allow everything to call these.
        public static void CallMouseDown(MouseButtonEventArgs e) { OnMouseClicked?.Invoke(e.Button, e.Modifiers); }
        public static void CallMouseUp(MouseButtonEventArgs e) { OnMouseReleased?.Invoke(e.Button, e.Modifiers); }
        public static void CallKeyDown(KeyboardKeyEventArgs e) { OnKeyPressed?.Invoke(e.Key, e.ScanCode, e.Modifiers); }
        public static void CallKeyUp(KeyboardKeyEventArgs e) { OnKeyReleased?.Invoke(e.Key, e.ScanCode, e.Modifiers); }

        public static bool IsKeyPressed(Keys key) => _keyboardState == null ? false : _keyboardState.IsKeyDown(key);
        public static bool IsMouseClicked(MouseButton button) => _mouseState == null ? false : _mouseState.IsButtonDown(button);
        public static bool IsAnyKeyPressed => _keyboardState == null ? false : _keyboardState.IsAnyKeyDown;
    }

    public delegate void MouseEvent(MouseButton button, KeyModifiers modifiers);
    public delegate void KeyboardEvent(Keys key, int code, KeyModifiers modifiers);
}
