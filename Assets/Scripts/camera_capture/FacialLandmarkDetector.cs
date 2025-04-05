using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;

public class FacialLandmarkDetector : MonoBehaviour
{
    public static void GetFacialLandmarks()
    {   
        // build paths
        string imagePath = Path.Combine(Application.persistentDataPath, "face.png");
        string supportPath = Path.Combine(Application.streamingAssetsPath, "build_support/camera_capture");
        string scriptPath = Path.Combine(supportPath, "detect_facial_landmarks.py");
        string pythonPath = Path.Combine(supportPath, "run_python.sh");

        ProcessStartInfo start = new ProcessStartInfo
        {
            FileName = pythonPath,
            Arguments = $"\"{scriptPath}\" \"{imagePath}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (Process process = Process.Start(start))
        {
            string output = process.StandardOutput.ReadToEnd().Trim();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (!string.IsNullOrEmpty(error))
            {
                UnityEngine.Debug.LogError($"Error: {error}");
            }

            if (string.IsNullOrWhiteSpace(output))
            {
                UnityEngine.Debug.LogWarning("No output received from Python.");
                return;
            }

            try
            {
                UnityEngine.Debug.Log("Raw Python output:\n" + output);

                LandmarkArrayWrapper landmarkList = JsonUtility.FromJson<LandmarkArrayWrapper>(output);
                foreach (var lm in landmarkList.landmarks)
                {
                    UnityEngine.Debug.Log($"Landmark {lm.name} at ({lm.x}, {lm.y})");
                }
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError("JSON parsing failed: " + ex.Message);
            }
        }
    }
}

[System.Serializable]
public class Landmark
{
    public string name;
    public float x;
    public float y;
}

[System.Serializable]
public class LandmarkArrayWrapper
{
    public Landmark[] landmarks;
}
