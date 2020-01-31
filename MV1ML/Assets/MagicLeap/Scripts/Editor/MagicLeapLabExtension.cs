#if UNITY_EDITOR && PLATFORM_LUMIN
using System;
using System.Diagnostics;
using System.IO;
using UnityEditor;

namespace MagicLeap
{
    /// <summary>
    /// A helper class to allow Magic Leap Lab or Zero Iteration to launch from Unity.
    /// </summary>
    public class MagicLeapLabExtension : EditorWindow
    {
        private static EditorWindow _window;
        private static readonly string _LogFile = Path.Combine("Temp", "LastMLLabLaunchResult.txt");
        private static bool _CLLaunching = false;

        [MenuItem("Magic Leap/Launch Magic Leap Lab", priority = 0)]
        static void LaunchMagicleapLabStart()
        {
            StartMagicLeapLab();
        }

        [MenuItem("Magic Leap/Launch Magic Leap Lab", true)]
        static bool ValidateLaunchMagicLeapLabStart()
        {
            return MagicLeapLabExists();
        }

        [MenuItem("Magic Leap/ML Remote/Launch Zero Iteration")]
        static void LaunchZIStart()
        {
            StartMagicLeapLab(true);
        }

        [MenuItem("Magic Leap/ML Remote/Launch Zero Iteration", true)]
        static bool ValidateLaunchZIStart()
        {
            return MagicLeapLabExists();
        }

        private static bool MagicLeapLabExists()
        {
            if ((EditorUserBuildSettings.activeBuildTarget == BuildTarget.Lumin) && !string.IsNullOrWhiteSpace(UnityEditor.Lumin.UserBuildSettings.SDKPath) && System.IO.File.Exists(Path.Combine(UnityEditor.Lumin.UserBuildSettings.SDKPath, "labdriver")))
            {
                return true;
            }

            return false;
        }

        private static void StartMagicLeapLab(bool openZI = false)
        {
            if (!_CLLaunching)
            {
                _CLLaunching = true;
                UnityEngine.Debug.Log("Magic Leap Lab Launching");
                ExecuteProcess(openZI);
            }
            else
            {
                UnityEngine.Debug.Log("Magic Leap Lab is already launching.");
            }
        }

        private static void ExecuteProcess(bool openZI)
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
#if UNITY_EDITOR_WIN
            string commandToSend = openZI ? System.String.Format("/c {0}/labdriver -o {1} -pretty go-zi-module", UnityEditor.Lumin.UserBuildSettings.SDKPath, _LogFile) : System.String.Format("/c {0}/labdriver -o {1} -pretty start-gui", UnityEditor.Lumin.UserBuildSettings.SDKPath, _LogFile);
            startInfo.FileName = "CMD.exe";
            startInfo.UseShellExecute = true;
#elif UNITY_EDITOR_OSX
            string commandToSend = openZI ? System.String.Format("{0}/labdriver -o {1} -pretty go-zi-module", UnityEditor.Lumin.UserBuildSettings.SDKPath, _LogFile) : System.String.Format("{0}/labdriver -o {1} -pretty start-gui", UnityEditor.Lumin.UserBuildSettings.SDKPath, _LogFile);
            startInfo.FileName = "/bin/bash";
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardInput = true;
#endif
            startInfo.CreateNoWindow = true;
            startInfo.Arguments = commandToSend;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.EnableRaisingEvents = true;
            process.Exited += OnProcessExit;
            process.StartInfo = startInfo;

            process.Start();
        }

        private static void OnProcessExit(object sender, EventArgs e)
        {
            if (System.IO.File.Exists(_LogFile))
            {
                ProcessLogs();
            }
            else
            {
                UnityEngine.Debug.Log("Completed Magic Leap Lab launch process without a log file.");
            }

            _CLLaunching = false;
        }

        private static void ProcessLogs()
        {
            string logtoprint = "Completed Magic Leap Lab launch process ";

            StreamReader reader = new StreamReader(_LogFile);

            string line = reader.ReadLine();
            while(line != null)
            {
                line = line.Trim();

                if (line == "\"error\": [")
                {
                    string nextline = reader.ReadLine();
                    if (nextline.Trim() == "\"\"")
                    {
                        logtoprint += "without errors. Full logs located in " + _LogFile;
                        break;
                    }
                    else
                    {
                        logtoprint += "with errors. Full logs located in " + _LogFile + "\nErrors:";

                        while (nextline.Trim() != "]," && nextline.Trim() != "]" && nextline.Trim() != null)
                        {
                            logtoprint += "\n" + nextline.Trim();
                            nextline = reader.ReadLine();
                        }
                        break;
                    }
                }

                line = reader.ReadLine();
            }

            reader.Close();

            UnityEngine.Debug.Log(logtoprint + "\n");
        }
    }
}
#endif // UNITY_EDITOR && PLATFORM_LUMIN
