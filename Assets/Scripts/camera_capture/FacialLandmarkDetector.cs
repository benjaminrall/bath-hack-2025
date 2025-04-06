using System.Diagnostics;
using System.IO;
using UnityEngine;

public class FacialLandmarkDetector : MonoBehaviour
{
    public static void GetFacialLandmarks()
    {
        string facePath = Path.Combine(Application.persistentDataPath, "face.png");
        string facePath2 = Path.Combine(Application.persistentDataPath, "face_150x150.png");
        string supportPath = Path.Combine(Application.streamingAssetsPath, "build_support/camera_capture");
        string detect_facial_landmarks_path = Path.Combine(supportPath, "detect_facial_landmarks.py");
        string pad_image_path = Path.Combine(supportPath, "pad_image.py");
        string pythonPath = Path.Combine(supportPath, "run_python.bat");


        // get the 150x150 image
        ProcessStartInfo detectFace = new ProcessStartInfo
        {
            FileName = pythonPath,
            Arguments = $"\"{detect_facial_landmarks_path}\" \"{facePath}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (Process process = Process.Start(detectFace))
        {
            string output = process.StandardOutput.ReadToEnd().Trim();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (!string.IsNullOrEmpty(error))
            {
                UnityEngine.Debug.LogError($"Python Error: {error}");
            }

            if (!string.IsNullOrEmpty(output))
            {
                UnityEngine.Debug.Log($"Python Output:\n{output}");
            }
        }

        // pad the image to 510x192
        ProcessStartInfo padImage = new ProcessStartInfo
        {
            FileName = pythonPath,
            Arguments = $"\"{pad_image_path}\" \"{facePath2}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        using (Process process = Process.Start(padImage))
        {
            string output = process.StandardOutput.ReadToEnd().Trim();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (!string.IsNullOrEmpty(error))
            {
                UnityEngine.Debug.LogError($"Python Error: {error}");
            }

            if (!string.IsNullOrEmpty(output))
            {
                UnityEngine.Debug.Log($"Python Output:\n{output}");
            }
        }
    }
}
