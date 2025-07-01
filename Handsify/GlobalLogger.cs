using Serilog;
using Serilog.Events;
using Serilog.Configuration;

namespace Badgeify
{
    /// <summary>
    /// Provides a globally accessible logger using Serilog.
    /// </summary>
    public static class GlobalLogger
    {
        // Static instance of the logger
        public static Serilog.ILogger Logger { get; private set; }

        /// <summary>
        /// Initializes the global logger with the specified configuration.
        /// </summary>
        /// <param name="logFilePath">Path for the log file.</param>
        public static void Initialize(string logFilePath)
        {
            Logger = new LoggerConfiguration()
                .MinimumLevel.Debug() // Set the minimum logging level
                .WriteTo.Console() // Log to the console
                .WriteTo.File(logFilePath, rollingInterval: RollingInterval.Day) // Log to a file with daily rolling
                .CreateLogger();
        }



        /// <summary>
        /// Shuts down and disposes of the logger.
        /// </summary>
        public static void CloseAndFlush()
        {
            Log.CloseAndFlush();
        }
    }
}
