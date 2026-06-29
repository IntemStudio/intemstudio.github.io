using System.Diagnostics;
using UnityEngine;

namespace IntemStudio.Core
{
    // 에디터 전용 개발 로그. 릴리스는 Debug.Log* 직접 사용.
    // Log API는 모두 Debug.Log + 색상 태그만 사용 — Unity 네이티브 Warning/Error 아이콘과 구분.
    public static class Log
    {
        private const string COLOR_PROGRESS = "#517348";
        private const string COLOR_WARNING = "#E2B53E";
        private const string COLOR_ERROR = "#D92B2B";
        private const string COLOR_EXCEPT = "#252C59";

        private static bool _levelsLoaded;

        public static bool LevelProgress { get; private set; } = true;
        public static bool LevelInfo { get; private set; } = true;
        public static bool LevelWarning { get; private set; } = true;
        public static bool LevelError { get; private set; } = true;
        public static bool LevelExcept { get; private set; } = true;

        public static void LoadLevel()
        {
#if UNITY_EDITOR
            bool defaultEnabled = true;
#else
            bool defaultEnabled = false;
#endif

            LevelProgress = GamePrefs.GetBoolOrDefault(GamePrefTypes.LOG_LEVEL_PROCESS, defaultEnabled);
            LevelInfo = GamePrefs.GetBoolOrDefault(GamePrefTypes.LOG_LEVEL_INFO, defaultEnabled);
            LevelWarning = GamePrefs.GetBoolOrDefault(GamePrefTypes.LOG_LEVEL_WARNING, defaultEnabled);
            LevelError = GamePrefs.GetBoolOrDefault(GamePrefTypes.LOG_LEVEL_ERROR, defaultEnabled);
            LevelExcept = GamePrefs.GetBoolOrDefault(GamePrefTypes.LOG_LEVEL_EXCEPT, defaultEnabled);

            _levelsLoaded = true;
        }

        public static void SetLogLevelAll()
        {
            EnsureLevelsLoaded();

            LevelProgress = true;
            LevelInfo = true;
            LevelWarning = true;
            LevelError = true;
            LevelExcept = true;

            SaveLevelSettings();
        }

        public static void SetLogLevelOff()
        {
            EnsureLevelsLoaded();

            LevelProgress = false;
            LevelInfo = false;
            LevelWarning = false;
            LevelError = false;
            LevelExcept = false;

            SaveLevelSettings();
        }

        public static void SwitchLevelProgress() => SetLevel(LogLevel.Progress, !LevelProgress);
        public static void SwitchLevelInfo() => SetLevel(LogLevel.Info, !LevelInfo);
        public static void SwitchLevelWarning() => SetLevel(LogLevel.Warning, !LevelWarning);
        public static void SwitchLevelError() => SetLevel(LogLevel.Error, !LevelError);
        public static void SwitchLevelExcept() => SetLevel(LogLevel.Except, !LevelExcept);

        public static void SetLevel(LogLevel level, bool enabled)
        {
            SetLevelEnabled(level, enabled);
        }

        public static bool IsLevelEnabled(LogLevel level)
        {
            EnsureLevelsLoaded();

            return level switch
            {
                LogLevel.Progress => LevelProgress,
                LogLevel.Info => LevelInfo,
                LogLevel.Warning => LevelWarning,
                LogLevel.Error => LevelError,
                LogLevel.Except => LevelExcept,
                _ => true,
            };
        }

        [Conditional("UNITY_EDITOR")]
        public static void Progress(string message)
        {
            Write(LogLevel.Progress, null, message);
        }

        [Conditional("UNITY_EDITOR")]
        public static void Progress(LogTags tag, string message)
        {
            Write(LogLevel.Progress, tag, message);
        }

        [Conditional("UNITY_EDITOR")]
        public static void Info(string message)
        {
            Write(LogLevel.Info, null, message);
        }

        [Conditional("UNITY_EDITOR")]
        public static void Info(LogTags tag, string message)
        {
            Write(LogLevel.Info, tag, message);
        }

        [Conditional("UNITY_EDITOR")]
        public static void Warning(string message)
        {
            Write(LogLevel.Warning, null, message);
        }

        [Conditional("UNITY_EDITOR")]
        public static void Warning(LogTags tag, string message)
        {
            Write(LogLevel.Warning, tag, message);
        }

        [Conditional("UNITY_EDITOR")]
        public static void Error(string message)
        {
            Write(LogLevel.Error, null, message);
        }

        [Conditional("UNITY_EDITOR")]
        public static void Error(LogTags tag, string message)
        {
            Write(LogLevel.Error, tag, message);
        }

        [Conditional("UNITY_EDITOR")]
        public static void Except(string message)
        {
            Write(LogLevel.Except, null, message);
        }

        [Conditional("UNITY_EDITOR")]
        public static void Except(LogTags tag, string message)
        {
            Write(LogLevel.Except, tag, message);
        }

        public enum LogLevel
        {
            Progress,
            Info,
            Warning,
            Error,
            Except,
        }

        private static void EnsureLevelsLoaded()
        {
            if (_levelsLoaded)
            {
                return;
            }

            LoadLevel();
        }

        private static void SetLevelEnabled(LogLevel level, bool enabled)
        {
            EnsureLevelsLoaded();

            switch (level)
            {
                case LogLevel.Progress:
                    LevelProgress = enabled;
                    break;
                case LogLevel.Info:
                    LevelInfo = enabled;
                    break;
                case LogLevel.Warning:
                    LevelWarning = enabled;
                    break;
                case LogLevel.Error:
                    LevelError = enabled;
                    break;
                case LogLevel.Except:
                    LevelExcept = enabled;
                    break;
            }

            SaveLevelSettings();
        }

        private static void SaveLevelSettings()
        {
            GamePrefs.SetBool(GamePrefTypes.LOG_LEVEL_PROCESS, LevelProgress);
            GamePrefs.SetBool(GamePrefTypes.LOG_LEVEL_INFO, LevelInfo);
            GamePrefs.SetBool(GamePrefTypes.LOG_LEVEL_WARNING, LevelWarning);
            GamePrefs.SetBool(GamePrefTypes.LOG_LEVEL_ERROR, LevelError);
            GamePrefs.SetBool(GamePrefTypes.LOG_LEVEL_EXCEPT, LevelExcept);
        }

        private static void Write(LogLevel level, LogTags? tag, string message)
        {
            if (!IsLevelEnabled(level))
            {
                return;
            }

            if (tag.HasValue && !LogTagFilter.IsTagEnabled(tag.Value))
            {
                return;
            }

            string tagPrefix = tag.HasValue ? $"[{tag}] " : string.Empty;
            string formatted = $"{tagPrefix}[{level}] {message}";
            string output = level switch
            {
                LogLevel.Progress => Colorize(COLOR_PROGRESS, formatted),
                LogLevel.Warning => Colorize(COLOR_WARNING, formatted),
                LogLevel.Error => Colorize(COLOR_ERROR, formatted),
                LogLevel.Except => Colorize(COLOR_EXCEPT, formatted),
                _ => formatted,
            };

            UnityEngine.Debug.Log(output);
        }

        private static string Colorize(string hex, string text)
        {
            return $"<color={hex}>{text}</color>";
        }
    }
}
