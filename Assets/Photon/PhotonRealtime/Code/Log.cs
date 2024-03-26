// ----------------------------------------------------------------------------
// <copyright file="SupportLogger.cs" company="Exit Games GmbH">
// Photon Realtime API - Copyright (C) 2022 Exit Games GmbH
// </copyright>
// <summary>
// Logging Helper.
// </summary>
// <author>developer@photonengine.com</author>
// ----------------------------------------------------------------------------


#if UNITY_2017_4_OR_NEWER
#define SUPPORTED_UNITY
#endif


namespace Photon.Realtime
{
    using System;
    using System.Text;
    using Stopwatch = System.Diagnostics.Stopwatch;
    using Conditional = System.Diagnostics.ConditionalAttribute;
    using Photon.Client;

    #if SUPPORTED_UNITY
    using UnityEngine;
    #endif

    #if SUPPORTED_UNITY || NETFX_CORE
    using SupportClass = Photon.Client.SupportClass;
    #endif


    /// <summary>Static class to provide customizable log functionality.</summary>
    public static class Log
    {
        public enum PrefixOptions { None, Time, Level, TimeAndLevel }
        public enum LogOutputOption
        {
            /// <summary>Auto becomes UnityDebug if this is a Unity build. Otherwise it defaults to Console.</summary>
            Auto, 
            /// <summary>Logs via Console.WriteLine.</summary>
            Console,
            /// <summary>Logs via Debug.WriteLine.</summary>
            Debug,
            /// <summary>Logs via UnityEngine.Debug.Log.</summary>
            UnityDebug
        }
        public static PrefixOptions LogPrefix = PrefixOptions.None;

        private static Action<string> OnError;
        private static Action<string> OnWarn;
        private static Action<string> OnInfo;
        private static Action<string> OnDebug;
        private static Action<Exception, string> OnException;
        private static Stopwatch sw;


        /// <summary>Static constructor will initialize the logging actions.</summary>
        static Log()
        {
            Init(LogOutputOption.Auto);
        }

        /// <summary>Initializes the logging to selected output stream.</summary>
        /// <remarks>Auto becomes UnityDebug if this is a Unity build. Otherwise it defaults to Console.</remarks>
        /// <param name="logOutput"></param>
        public static void Init(LogOutputOption logOutput)
        {
            OnError = null;
            OnWarn = null;
            OnInfo = null;
            OnDebug = null;
            
            sw = new Stopwatch();
            sw.Restart();

            #if !SUPPORTED_UNITY
            if (logOutput == LogOutputOption.UnityDebug || logOutput == LogOutputOption.Auto)
            {
                logOutput = LogOutputOption.Console;
            }
            #else
            if (logOutput == LogOutputOption.UnityDebug || logOutput == LogOutputOption.Auto)
            {
                OnError = UnityEngine.Debug.LogError;
                OnWarn = UnityEngine.Debug.LogWarning;
                OnInfo = UnityEngine.Debug.Log;
                OnDebug = UnityEngine.Debug.Log;
                return;
            }
            #endif

            if (logOutput == LogOutputOption.Console)
            {
                OnError = (msg) => Console.WriteLine(msg);
                OnWarn = (msg) => Console.WriteLine(msg);
                OnInfo = (msg) => Console.WriteLine(msg);
                OnDebug = (msg) => Console.WriteLine(msg);
                return;
            }

            if (logOutput == LogOutputOption.Debug)
            {
                OnError = (msg) => System.Diagnostics.Debug.WriteLine(msg);
                OnWarn = (msg) => System.Diagnostics.Debug.WriteLine(msg);
                OnInfo = (msg) => System.Diagnostics.Debug.WriteLine(msg);
                OnDebug = (msg) => System.Diagnostics.Debug.WriteLine(msg);
                return;
            }
        }

        /// <summary>
        /// Initialize the logging actions to custom actions. Note: These are initialized by default.
        /// </summary>
        /// <param name="error">Log errors.</param>
        /// <param name="warn">Log warnings.</param>
        /// <param name="info">Log info.</param>
        /// <param name="debug">Log debugging / tracing.</param>
        public static void Init(Action<string> error, Action<string> warn, Action<string> info, Action<string> debug, Action<Exception, string> exception)
        {
            sw = new Stopwatch();
            sw.Restart();

            OnError = error;
            OnWarn = warn;
            OnInfo = info;
            OnDebug = debug;
            OnException = exception;
        }


        /// <summary>Prefixes the message with timestamp, log level and prefix.</summary>
        static string ApplyPrefixes(string msg, LogLevel lvl = LogLevel.Error, string prefix = null)
        {
            StringBuilder sb = new StringBuilder();

            if (LogPrefix == PrefixOptions.Time || LogPrefix == PrefixOptions.TimeAndLevel)
            {
                //sb.Append($"[{GetFormattedTimestamp()}]");
                TimeSpan span = sw.Elapsed;
                if (span.Minutes > 0)
                {
                    sb.Append($"[{span.Minutes}:{span.Seconds:D2}.{span.Milliseconds:D3}]");
                }
                else
                    sb.Append($"[{span.Seconds:D2}.{span.Milliseconds:D3}]");

            }
            if (LogPrefix == PrefixOptions.Level || LogPrefix == PrefixOptions.TimeAndLevel)
            {
                sb.Append($"[{lvl}]");
            }

            if (!string.IsNullOrEmpty(prefix))
            {
                sb.Append($"{prefix}: ");
            }
            else if (sb.Length > 0)
            {
                sb.Append(" ");
            }

            sb.Append(msg);
            return sb.ToString();
        }


        /// <summary>Check level, format message and call OnError to log.</summary>
        /// <param name="msg">The message to log.</param>
        /// <param name="lvl">The logging level of the instance that is about to log this message (if the level is equal or greater than this message.</param>
        /// <param name="prefix">String to place in front of the actual message.</param>
        public static void Exception(Exception ex, LogLevel lvl = LogLevel.Error, string prefix = null)
        {
            if (lvl < LogLevel.Error || OnException == null)
            {
                return;
            }

            string output = ApplyPrefixes("", lvl, prefix);
            OnException(ex, prefix);
        }

        /// <summary>Check level, format message and call OnError to log.</summary>
        /// <param name="msg">The message to log.</param>
        /// <param name="lvl">The logging level of the instance that is about to log this message (if the level is equal or greater than this message.</param>
        /// <param name="prefix">String to place in front of the actual message.</param>
        public static void Error(string msg, LogLevel lvl = LogLevel.Error, string prefix = null)
        {
            if (lvl < LogLevel.Error || OnError == null)
            {
                return;
            }

            string output = ApplyPrefixes(msg, lvl, prefix);
            OnError(output);
        }

        /// <summary>Check level, format message and call OnWarn to log.</summary>
        /// <param name="msg">The message to log.</param>
        /// <param name="lvl">The logging level of the instance that is about to log this message (if the level is equal or greater than this message.</param>
        /// <param name="prefix">String to place in front of the actual message.</param>
        [Conditional("DEBUG"), Conditional("LOG_WARNING")]
        public static void Warn(string msg, LogLevel lvl = LogLevel.Warning, string prefix = null)
        {
            if (lvl < LogLevel.Warning || OnWarn == null)
            {
                return;
            }

            string output = ApplyPrefixes(msg, lvl, prefix);
            OnWarn(output);
        }

        /// <summary>Check level, format message and call OnInfo to log.</summary>
        /// <param name="msg">The message to log.</param>
        /// <param name="lvl">The logging level of the instance that is about to log this message (if the level is equal or greater than this message.</param>
        /// <param name="prefix">String to place in front of the actual message.</param>
        [Conditional("DEBUG"), Conditional("LOG_INFO")]
        public static void Info(string msg, LogLevel lvl = LogLevel.Info, string prefix = null)
        {
            if (lvl < LogLevel.Info || OnInfo == null)
            {
                return;
            }

            string output = ApplyPrefixes(msg, lvl, prefix);
            OnInfo(output);
        }

        /// <summary>Check level, format message and call OnDebug to log.</summary>
        /// <param name="msg">The message to log.</param>
        /// <param name="lvl">The logging level of the instance that is about to log this message (if the level is equal or greater than this message.</param>
        /// <param name="prefix">String to place in front of the actual message.</param>
        [Conditional("DEBUG"), Conditional("LOG_DEBUG")]
        public static void Debug(string msg, LogLevel lvl = LogLevel.Debug, string prefix = null)
        {
            if (lvl < LogLevel.Debug || OnDebug == null)
            {
                return;
            }

            string output = ApplyPrefixes(msg, lvl, prefix);
            OnDebug(output);
        }
    }
}