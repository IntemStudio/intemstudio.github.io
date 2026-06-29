using System.Reflection;
using System.Text;
using IntemStudio.Data;
using UnityEngine;

namespace IntemStudio
{
    public class Log
    {
        public enum Level
        {
            Off,

            Error,

            Warning,

            Info,

            Progress,

            Full,
        }

        public static bool LevelError;

        public static bool LevelWarning;

        public static bool LevelException;

        public static bool LevelInfo;

        public static bool LevelProgress;

        private static LogSystem logSystem = null;

        private static LogSystem LogSystem
        {
            get
            {
                if (null == logSystem)
                {
#if !UNITY_EDITOR && !DEVELOPMENT_BUILD && !DEMO_BUILD
                    SetLogLevelOff();
					SetLogLevelError();
                    logSystem = new LogSystem("Client", true);
#elif DEVELOPMENT_BUILD || DEMO_BUILD
					SetLogLevelOff();
					SetLogLevelError();
                    logSystem = new LogSystem("Development", true);
#else
                    logSystem = new LogSystem("Editor", false);
#endif
                }

                return logSystem;
            }
        }

        public static void LoadLevel()
        {
            if (GamePrefs.HasKey("LevelError"))
            {
                LevelError = GamePrefs.GetBool("LevelError");
            }
            else
            {
                LevelError = true;
            }

            if (GamePrefs.HasKey("LevelWarning"))
            {
                LevelWarning = GamePrefs.GetBool("LevelWarning");
            }
            else
            {
                LevelWarning = true;
            }

            if (GamePrefs.HasKey("LevelException"))
            {
                LevelException = GamePrefs.GetBool("LevelException");
            }
            else
            {
                LevelException = true;
            }

            if (GamePrefs.HasKey("LevelInfo"))
            {
                LevelInfo = GamePrefs.GetBool("LevelInfo");
            }
            else
            {
                LevelInfo = true;
            }

            if (GamePrefs.HasKey("LevelProgress"))
            {
                LevelProgress = GamePrefs.GetBool("LevelProgress");
            }
            else
            {
                LevelProgress = true;
            }
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void Process(string format, params object[] args)
        {
            if (LevelProgress)
            {
                LogSystem.Log("Process", string.Format(format, args), Color.green);
            }
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void Spare(string format, params object[] args)
        {
            // Info(format, args);
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void Info(string format, params object[] args)
        {
            if (LevelInfo)
            {
                LogSystem.Log("Info", string.Format(format, args));
            }
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void Info(LogTags infoType, string format, params object[] args)
        {
            if (LevelInfo == false)
            {
                return;
            }

            if (Data.AssetManager.CheckLogOn(infoType) == false)
            {
                return;
            }

            string tag = infoType.ToString();
            LogSystem.Log(tag, string.Format(format, args));
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void Exception(string format, params object[] args)
        {
            // Warning(format, args);
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void Warning(string format, params object[] args)
        {
            if (LevelWarning)
            {
                LogSystem.Log("Warning", string.Format(format, args), Color.yellow);
            }
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void Warning(LogTags infoType, string format, params object[] args)
        {
            if (LevelWarning == false)
            {
                return;
            }

            if (Data.AssetManager.CheckLogOn(infoType) == false)
            {
                return;
            }

            string tag = infoType.ToString();
            LogSystem.Log(tag, string.Format(format, args), Color.yellow);
        }

        public static void Error(string format, params object[] args)
        {
            if (LevelError)
            {
                string content = string.Format(format, args);

                LogSystem.Log("Error", content, Color.red);

                LogSystem.FileLog("Error", content);
            }
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void MethodName(int index = 1)
        {
            System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace();
            StringBuilder sb = new StringBuilder();
            System.Diagnostics.StackFrame frame;
            for (int i = index - 1; i >= 0; i--)
            {
                frame = stackTrace.GetFrame(i + 1);
                if (frame != null)
                {
                    MethodBase methodBase = frame.GetMethod();
                    sb.Append(methodBase.ReflectedType.Name);
                    sb.Append("::");
                    sb.Append(methodBase.Name);

                    if (i != 0)
                    {
                        sb.AppendLine();
                    }
                }
            }

            Process(sb.ToString());
        }

        public static void SetLogLevelOff()
        {
            LevelError = false;
            LevelWarning = false;
            LevelException = false;
            LevelInfo = false;
            LevelProgress = false;

            SetLogLevel("LevelError", LevelError);
            SetLogLevel("LevelWarning", LevelWarning);
            SetLogLevel("LevelException", LevelException);
            SetLogLevel("LevelInfo", LevelInfo);
            SetLogLevel("LevelProgress", LevelProgress);
        }

        public static void SetLogLevelError()
        {
            LevelError = !LevelError;

            SetLogLevel("LevelError", LevelError);
        }

        public static void SetLogLevelWarning()
        {
            LevelWarning = !LevelWarning;

            SetLogLevel("LevelWarning", LevelWarning);
        }

        public static void SetLogLevelException()
        {
            LevelException = !LevelException;

            SetLogLevel("LevelException", LevelException);
        }

        public static void SetLogLevelInfo()
        {
            LevelInfo = !LevelInfo;

            SetLogLevel("LevelInfo", LevelInfo);
        }

        public static void SetLogLevelProgress()
        {
            LevelProgress = !LevelProgress;

            SetLogLevel("LevelProgress", LevelProgress);
        }

        public static void SetLogLevelAll()
        {
            LevelError = true;
            LevelWarning = true;
            LevelException = true;
            LevelInfo = true;
            LevelProgress = true;

            SetLogLevel("LevelError", LevelError);
            SetLogLevel("LevelWarning", LevelWarning);
            SetLogLevel("LevelException", LevelException);
            SetLogLevel("LevelInfo", LevelInfo);
            SetLogLevel("LevelProgress", LevelProgress);
        }

        private static void SetLogLevel(string levelName, bool value)
        {
            GamePrefs.SetBool(levelName, value);

#if UNITY_EDITOR
            Debug.LogFormat("Log {0} : {1}", levelName, value.ToBoolString());
#endif
        }
    }
}

/*
 * Log 최적화

C# 조건 속성을 사용합니다.
참고하기: https://msdn.microsoft.com/en-us/library/4xssyw96(v=vs.90).aspx

using System;
using System.Diagnostics;

public class DebugX
{
    [Conditional("UnityEditor")]
    public static void Log(string msg)
    {
        Debug.Log(msg);
    }
}

PlayerSettings > Scripting Define Symbols 에 ACTIVE_DEBUG 가 없다면
아래 코드 호출은 아예 처음부터 없었던 것으로 처리됩니다.

DebugX.Log( "Hello world" );
클래스 내부에 Conditional 속성이 지정된 static void 함수를 추가합니다.

만약 Conditional 에 따라오는 심볼이 정의됬다면 static 함수는 존재합니다.
중요한것은 static 함수 뿐만이 아니라, 그것을 호출하는 부분 까지 모조리 빌드에서 빠져나가므로 매우 간편합니다.
조건 속성으로 묶인 디버그 함수를 사용한 코드들은 빌드시에 처음부터 존재하지도 않은 것이 됩니다.

다만 유니티 콘솔창에서 로그를 더블 클릭하면, 오류가 난 지점이 아닌 디버그를 랩핑한 코드로 이동하는 사소한 불편은 있습니다.
*/