#region

using System;
using System.IO;
using System.Reflection;
using System.Text;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using log4net.Layout;

#endregion

namespace FileBackupper {
internal class Log {
    private const string DatePattern = "yyyy-MM-dd HH:mm:ss";
    private static readonly string AppenderPattern = $"%date{{{DatePattern}}} [%p] %m%n";

    private static readonly ILog Logger;
    private static readonly StringBuilder LogEvents;

    static Log() {
        Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);
        LogEvents = new StringBuilder();

        var consoleAppender = new ConsoleAppender() {
            Layout = new PatternLayout(AppenderPattern),
            Threshold = Level.Info
        };
        var textWriterAppender = new TextWriterAppender() {
            Layout = new PatternLayout(AppenderPattern),
            Threshold = Level.All,
            Writer = new StringWriter(LogEvents)
        };
        BasicConfigurator.Configure(textWriterAppender /*, consoleAppender*/);
    }

    internal static void Info(string message, bool printToConsole = true) {
        Logger.Info(message);
        if (printToConsole){
            PrintToConsole(message, Level.Info);
        }
    }

    internal static void Error(string message, Exception exception = null, bool printToConsole = true) {
        Logger.Error(message);
        if (exception != null){
            Logger.Debug(exception);
        }

        if (printToConsole){
            PrintToConsole(message, Level.Error);
        }
    }

    internal static void Warning(string message, Exception exception = null, bool printToConsole = true) {
        Logger.Warn(message);
        if (exception != null){
            Logger.Debug(exception);
        }

        if (printToConsole){
            PrintToConsole(message, Level.Warn);
        }
    }

    private static void PrintToConsole(string message, Level priority) {
        Console.WriteLine($"{DateTime.Now.ToString(DatePattern)} [{priority.DisplayName}] {message}");
    }

    internal static void WriteEventsToFile(string path) {
        File.WriteAllText(path, LogEvents.ToString());
    }
}
}