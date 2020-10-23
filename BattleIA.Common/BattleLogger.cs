using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace BattleIA
{
    enum BattleLogLevel
    {
        EMERGENCY = 0x0,
        ALERT = 0x1,
        CRITICAL = 0x2,
        ERROR = 0x3,
        WARNING = 0x4,
        NOTICE = 0x5,
        INFO = 0x6,
        DEBUG = 0x7
    }
    public class BattleLogger
    {

        public static BattleLogger logger = new BattleLogger();

        DirectoryInfo logDir;
        FileInfo logFile;
        FileStream logFileS;

        private bool consoleDisplay = true;
        private static object _MessageLock = new object();

        public BattleLogger()
        {
            String appDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            logDir = new DirectoryInfo( appDirectory + Path.DirectorySeparatorChar + "logs");
            if (!logDir.Exists)
            {
                Directory.CreateDirectory(logDir.FullName);
            }
            String logFileName = DateTime.Now.ToString("MMMM-dd-yyyy") + ".log";
            logFile = new FileInfo(Path.Combine(logDir.FullName, logFileName));

            if(!logFile.Exists)
            {
                logFileS = logFile.Create();
            }
            else
            {
                logFileS = File.Open(logFile.FullName, FileMode.Open);
                
            }

            
        }

        void setConsoleDisplay(bool shouldDisplay)
        {
            this.consoleDisplay = shouldDisplay;
        }

        void log(BattleLogLevel level, String message, params object[] list)
        {

            _log(level, message, list);
        }
        void _log(BattleLogLevel level, String message, object[] list)
        {

            String levelStr = Enum.GetName(typeof(BattleLogLevel), level);

            String dateNow = DateTime.Now.ToString("s");
            String content = $"[{levelStr}] [{dateNow}]: {message}";
            content += String.Join(',', list);
            content += '\n';
            this.writeToFile(content);
            logFileS.Flush();
            if (this.consoleDisplay)
            {
                this.writeToConsole(level, message);
            }
        }

        void writeToFile(String content)
        {
            byte[] line = new UTF8Encoding(true).GetBytes(content);
            logFileS.Write(line, 0, line.Length);
        }

        void writeToConsole(BattleLogLevel level, String message)
        {
            lock (_MessageLock)
            {
                String levelStr = Enum.GetName(typeof(BattleLogLevel), level);
                switch (level)
                {
                    case BattleLogLevel.EMERGENCY:
                        Console.ForegroundColor = ConsoleColor.Red;
                        break;
                    case BattleLogLevel.ALERT:
                        Console.ForegroundColor = ConsoleColor.Red;
                        break;
                    case BattleLogLevel.CRITICAL:
                        Console.ForegroundColor = ConsoleColor.Red;
                        break;
                    case BattleLogLevel.ERROR:
                        Console.ForegroundColor = ConsoleColor.Red;
                        break;
                    case BattleLogLevel.WARNING:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        break;
                    case BattleLogLevel.NOTICE:
                        Console.ForegroundColor = ConsoleColor.White;
                        break;
                    case BattleLogLevel.INFO:
                        Console.ForegroundColor = ConsoleColor.Blue;
                        break;
                    case BattleLogLevel.DEBUG:
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        break;
                    default:
                        Console.ForegroundColor = ConsoleColor.White;
                        break;
                }
                Console.Write($"[{levelStr}] ");
                Console.ResetColor();
                Console.WriteLine(message);
            }
        }

        public void info(String message, params object[] list)
        {
            _log(BattleLogLevel.INFO, message, list);
        }
        public void debug(String message, params object[] list)
        {
            _log(BattleLogLevel.DEBUG, message, list);
        }
        public void error(String message, params object[] list)
        {
            _log(BattleLogLevel.ERROR, message, list);
        }
        public void warning(String message, params object[] list)
        {
            _log(BattleLogLevel.WARNING, message, list);
        }
    }
}
