using System;

namespace VoxelGame.Framework.Logging
{
    /// <summary>
    /// Quickly thrown together logging solution. I was to lazy to learn and implement a proper logger.
    /// </summary>
    public abstract class Logger
    {
        private string _prefix;
        protected readonly ConsoleColor defaultColor;

        /// <summary>
        /// Smallest log level that will be logged.
        /// </summary>
        public int MinLogLevel;

        public Logger(int minLogLevel, string? consolePrefix)
        {
            _prefix = consolePrefix == null ? string.Empty : "[" + consolePrefix + "] ";
            MinLogLevel = minLogLevel;
            defaultColor = Console.ForegroundColor;
        }

        /// <summary>
        /// Logs a string to the console.
        /// </summary>
        public void Log(LogCategory category, string message)
        {
            if (category.Level < MinLogLevel) return;
            Print(category.Color, category.Prefix + message);
        }

        /// <summary>
        /// Logs the string representation of an object to the console.<br/>
        /// (Does not require boxing for types which override the <see cref="object.ToString"/> method)
        /// </summary>
        public void Log<T>(LogCategory category, T value)
        {
            if (category.Level < MinLogLevel) return;
            string message = value == null ? "null" : value.ToString()!;
            Print(category.Color, category.Prefix + message);
        }

        private void Print(ConsoleColor color, string message)
        {
            Console.Write(_prefix);
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ForegroundColor = defaultColor;
        }

        public struct LogCategory
        {
            public readonly string Prefix;
            public readonly ConsoleColor Color;
            public readonly int Level;

            public LogCategory(string prefix, int level, ConsoleColor color = ConsoleColor.White)
            {
                Prefix = $"({prefix}) ";
                Level = level;
                Color = color;
            }
        }
    }

}
