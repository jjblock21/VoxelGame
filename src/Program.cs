using Minecraft.Framework;
using VoxelGame.Game;

namespace VoxelGame
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            McWindow minecraft = new McWindow();

#if RELEASE
            // Print the exception through the logger in realease mode.
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                Exception exception = (Exception)e.ExceptionObject;

                // Currently we just print the crash report to the console.
                McWindow.Logger.Error($"Uncaught exception thrown in section '{ErrorHandler.CurrentSection}'");
                McWindow.Logger.Error($"Message: {exception.Message}\n{exception.StackTrace}");
                if (exception.InnerException != null)
                {
                    McWindow.Logger.Error($"Inner Exception: {exception.InnerException!.Message}");
                }
            };
#endif

            minecraft.Run();
            minecraft.Dispose();
        }
    }
}