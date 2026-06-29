using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace IntemStudio
{
    public class Log
    {
        // 로그 레벨 플래그
        public static bool LevelProgress = true;
        public static bool LevelInfo = true;
        public static bool LevelWarning = true;
        public static bool LevelError = true;
        public static bool LevelExcept = true;

        private static LogSystem logSystem = null;

        public static void Initialize()
        {
            if (logSystem == null)
            {
#if UNITY_EDITOR
                logSystem = new LogSystem("Editor", useFileLog: false);
#endif
            }

            logSystem?.Initialize();
        }

        private static bool IsInitialized => logSystem != null;

        private static void SafeLog(string tag, string content, Color? color)
        {
            if (!IsInitialized || logSystem == null)
            {
#if UNITY_EDITOR
                Initialize();
#else
                return;
#endif
            }

            logSystem.Log(tag, content, color);
        }

        // 로그 레벨별 색상 설정
        private static Color? GetColor(Level logLevel)
        {
#if !UNITY_EDITOR
            return null;
#endif

            return logLevel switch
            {
                Level.Progress => TSColors.DarkOliveGreen,
                Level.Warning => TSColors.ActivateYellow,
                Level.Error => TSColors.CherryRed,
                Level.Except => TSColors.RoyalBlue,
                _ => null,
            };
        }

        // 로그 레벨별 활성화 플래그 설정
        private static readonly Dictionary<Level, Func<bool>> LevelChecks = new()
        {
            { Level.Progress, () => LevelProgress },
            { Level.Info, () => LevelInfo },
            { Level.Warning, () => LevelWarning },
            { Level.Error, () => LevelError },
            { Level.Except, () => LevelExcept }
        };

        private static bool IsLogLevelEnabled(Level level)
        {
#if UNITY_EDITOR
            return LevelChecks.TryGetValue(level, out var check) && check();
#else
            return false;
#endif
        }

        // 매개변수 검증
        private static void ValidateArgs(string format, params object[] args)
        {
            if (string.IsNullOrEmpty(format))
            {
                throw new ArgumentException("포맷 스트링이 null이거나 비어있습니다.");
            }

            if (args == null || args.Length == 0)
            {
                throw new ArgumentException($"포맷 스트링에 필요한 인자가 null이거나 비어있습니다: {format}");
            }
        }

        // 문자열과 인자 포맷화
        private static string FormatMessage(string format, params object[] args)
        {
            StringBuilder sb = new StringBuilder();
            try
            {
                sb.AppendFormat(format, args);
            }
            catch (Exception exception)
            {
                Debug.LogErrorFormat($"{format}, {args.Length}, {args.JoinToString()}\n{exception}");
            }
            return sb.ToString();
        }

        #region 비동기 로그 처리

        private static readonly object logLock = new();

        private static void LogSync(string tag, string content, Color? color)
        {
            lock (logLock)
            {
                SafeLog(tag, content, color); // 스레드 안전 로그 처리 보장
            }
        }

        #endregion 비동기 로그 처리

        // 로그 메시지 처리 함수
        private static void LogMessage(string tag, string format, Level level, params object[] args)
        {
            if (false == IsLogLevelEnabled(level)) return; // 레벨 체크 후 조기 반환

            ValidateArgs(format, args);
            string content = FormatMessage(format, args);

            LogSync(tag, content, GetColor(level));
        }

        // 태그를 포함한 로그 메시지 처리
        private static void LogWithTag(LogTags tag, string format, Level level, params object[] args)
        {
            // 레벨 체크 후 조기 반환
            if (false == IsLogLevelEnabled(level)) return;
            if (false == TSScriptableDataManager.Instance.FindLog(tag)) return;

            ValidateArgs(format, args);
            string content = FormatMessage(format, args);

            LogSync(tag.ToString(), content, GetColor(level));
        }

        #region 로그 메서드

        public static void Progress(string content)
        {
            if (false == IsLogLevelEnabled(Level.Progress)) return;

            SafeLog(LogLevels.Progress, content, color: GetColor(Level.Progress));
        }

        public static void Progress(string format, params object[] args)
        {
            LogMessage(LogLevels.Progress, format, Level.Progress, args);
        }

        public static void Progress(LogTags tag, string content)
        {
            // 레벨 체크 후 조기 반환
            if (false == IsLogLevelEnabled(Level.Progress)) return;
            if (false == TSScriptableDataManager.Instance.FindLog(tag)) return;

            SafeLog(tag.ToString(), content, color: GetColor(Level.Progress));
        }

        public static void Progress(LogTags tag, string format, params object[] args)
        {
            LogWithTag(tag, format, Level.Progress, args);
        }

        public static void Info(string content)
        {
            if (false == IsLogLevelEnabled(Level.Info)) return;

            SafeLog(LogLevels.Info, content, color: GetColor(Level.Info));
        }

        public static void Info(string format, params object[] args)
        {
            LogMessage(LogLevels.Info, format, Level.Info, args);
        }

        public static void Info(LogTags tag, string content)
        {
            // 레벨 체크 후 조기 반환
            if (false == IsLogLevelEnabled(Level.Info)) return;
            if (false == TSScriptableDataManager.Instance.FindLog(tag)) return;

            SafeLog(tag.ToString(), content, color: GetColor(Level.Info));
        }

        public static void Info(LogTags tag, string format, params object[] args)
        {
            LogWithTag(tag, format, Level.Info, args);
        }

        public static void Warning(string content)
        {
            if (false == IsLogLevelEnabled(Level.Warning)) return;

            SafeLog(LogLevels.Warning, content, color: GetColor(Level.Warning));

#if UNITY_EDITOR
            Debug.LogWarning(content);
#endif
        }

        public static void Warning(string format, params object[] args)
        {
            LogMessage(LogLevels.Warning, format, Level.Warning, args);
        }

        public static void Warning(LogTags tag, string content)
        {
            // 레벨 체크 후 조기 반환
            if (false == IsLogLevelEnabled(Level.Warning)) return;
            if (false == TSScriptableDataManager.Instance.FindLog(tag)) return;

            SafeLog(tag.ToString(), content, color: GetColor(Level.Warning));

#if UNITY_EDITOR
            Debug.LogWarning(content);
#endif
        }

        public static void Warning(LogTags tag, string format, params object[] args)
        {
            LogWithTag(tag, format, Level.Warning, args);
        }

        public static void Error(string content)
        {
            if (false == IsLogLevelEnabled(Level.Error)) return;

            SafeLog(LogLevels.Error, content, color: GetColor(Level.Error));

#if UNITY_EDITOR
            Debug.LogError(content);
#endif
        }

        public static void Error(string format, params object[] args)
        {
            LogMessage(LogLevels.Error, format, Level.Error, args);
        }

        public static void Error(LogTags tag, string content)
        {
            // 레벨 체크 후 조기 반환
            if (false == IsLogLevelEnabled(Level.Error)) return;
            if (false == TSScriptableDataManager.Instance.FindLog(tag)) return;

            SafeLog(tag.ToString(), content, color: GetColor(Level.Error));

#if UNITY_EDITOR
            Debug.LogError(content);
#endif
        }

        public static void Error(LogTags tag, string format, params object[] args)
        {
            LogWithTag(tag, format, Level.Error, args);
        }

        public static void Except(string content)
        {
            if (false == IsLogLevelEnabled(Level.Except)) return;

            SafeLog(LogLevels.Except, content, color: GetColor(Level.Except));
        }

        public static void Except(string format, params object[] args)
        {
            LogMessage(LogLevels.Except, format, Level.Except, args);
        }

        public static void Except(LogTags tag, string format, params object[] args)
        {
            LogWithTag(tag, format, Level.Except, args);
        }

        #endregion 로그 메서드

        #region 로그 레벨

        public enum Level
        {
            Off,
            Error,
            Warning,
            Info,
            Progress,
            Except,
        }

        private static class LogLevels
        {
            public const string Progress = "Progress";
            public const string Info = "Info";
            public const string Warning = "Warning";
            public const string Error = "Error";
            public const string Except = "Except";
        }

        public static void LoadLevel()
        {
#if UNITY_EDITOR
            LevelProgress = GamePrefs.GetBoolOrDefault(GamePrefTypes.LOG_LEVEL_PROCESS, true);
            LevelInfo = GamePrefs.GetBoolOrDefault(GamePrefTypes.LOG_LEVEL_INFO, true);
            LevelWarning = GamePrefs.GetBoolOrDefault(GamePrefTypes.LOG_LEVEL_WARNING, true);
            LevelError = GamePrefs.GetBoolOrDefault(GamePrefTypes.LOG_LEVEL_ERROR, true);
            LevelExcept = GamePrefs.GetBoolOrDefault(GamePrefTypes.LOG_LEVEL_EXCEPT, true);
#else
            LevelProgress = GamePrefs.GetBoolOrDefault(GamePrefTypes.LOG_LEVEL_PROCESS, false);
            LevelInfo = GamePrefs.GetBoolOrDefault(GamePrefTypes.LOG_LEVEL_INFO, false);
            LevelWarning = GamePrefs.GetBoolOrDefault(GamePrefTypes.LOG_LEVEL_WARNING, false);
            LevelError = GamePrefs.GetBoolOrDefault(GamePrefTypes.LOG_LEVEL_ERROR, false);
            LevelExcept = GamePrefs.GetBoolOrDefault(GamePrefTypes.LOG_LEVEL_EXCEPT, false);
#endif            
        }

        public static void SetLogLevelOff()
        {
            LevelProgress = false;
            LevelInfo = false;
            LevelWarning = false;
            LevelError = false;
            LevelExcept = false;

            SaveLogLevelSettings();
        }

        public static void SetLogLevelAll()
        {
            LevelProgress = true;
            LevelInfo = true;
            LevelWarning = true;
            LevelError = true;
            LevelExcept = true;

            SaveLogLevelSettings();
        }

        private static void SaveLogLevelSettings()
        {
            GamePrefs.SetBool(GamePrefTypes.LOG_LEVEL_PROCESS, LevelProgress);
            GamePrefs.SetBool(GamePrefTypes.LOG_LEVEL_INFO, LevelInfo);
            GamePrefs.SetBool(GamePrefTypes.LOG_LEVEL_WARNING, LevelWarning);
            GamePrefs.SetBool(GamePrefTypes.LOG_LEVEL_ERROR, LevelError);
            GamePrefs.SetBool(GamePrefTypes.LOG_LEVEL_EXCEPT, LevelExcept);

            Debug.Log("Log Levels Saved.");
        }

        //

        public static void SwitchLogLevelProgress()
        {
            LevelProgress = !LevelProgress;
            SetLogLevel(GamePrefTypes.LOG_LEVEL_PROCESS, LevelProgress);
        }

        public static void SwitchLogLevelInfo()
        {
            LevelInfo = !LevelInfo;
            SetLogLevel(GamePrefTypes.LOG_LEVEL_INFO, LevelInfo);
        }

        public static void SwitchLogLevelWarning()
        {
            LevelWarning = !LevelWarning;
            SetLogLevel(GamePrefTypes.LOG_LEVEL_WARNING, LevelWarning);
        }

        public static void SwitchLogLevelError()
        {
            LevelError = !LevelError;
            SetLogLevel(GamePrefTypes.LOG_LEVEL_ERROR, LevelError);
        }

        public static void SwitchLogLevelExcept()
        {
            LevelExcept = !LevelExcept;
            SetLogLevel(GamePrefTypes.LOG_LEVEL_EXCEPT, LevelExcept);
        }

        private static void SetLogLevel(GamePrefTypes type, bool value)
        {
            GamePrefs.SetBool(type, value);
            Info($"로그 설정({type})을 저장합니다: {value.ToBoolString()}");
        }

        #endregion 로그 레벨
    }
}