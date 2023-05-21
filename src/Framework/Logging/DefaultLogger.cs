namespace VoxelGame.Framework.Logging
{
    public class DefaultLogger : Logger
    {
        public DefaultLogger(int minLogLevel, string? consolePrefix = null) : base(minLogLevel, consolePrefix)
        {
            debug = new LogCategory("Debug", 0, ConsoleColor.DarkGray);
            info = new LogCategory("Info", 1, defaultColor); // Default color isn't white but gray for some reason.
            warning = new LogCategory("Warning", 2, ConsoleColor.Yellow);
            error = new LogCategory("Error", 3, ConsoleColor.Red);
        }

        public readonly LogCategory debug;
        public readonly LogCategory info;
        public readonly LogCategory warning;
        public readonly LogCategory error;

        public void Debug(string message) => Log(debug, message);
        public void Debug<T>(T message) => Log(debug, message);
        public void Info(string message) => Log(info, message);
        public void Info<T>(T message) => Log(info, message);
        public void Warn(string message) => Log(warning, message);
        public void Warn<T>(T message) => Log(warning, message);
        public void Error(string message) => Log(error, message);
        public void Error<T>(T message) => Log(error, message);
    }
}
