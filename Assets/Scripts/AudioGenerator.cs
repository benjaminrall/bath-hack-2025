
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class AudioGenerator : MonoBehaviour
{
    private const string RefText = "To be or not to be, that is the question.";
    
    public static async Task GenerateAudioClips(string audioFolder)
    {
        string refPath = Path.Combine(Application.persistentDataPath, "recorded_audio.wav");
        string generationPath = Path.Combine(Application.persistentDataPath, "generation");
        string voicelinesPath = Path.Combine(generationPath, "voicelines.json");
        string modelPath = Path.Combine(generationPath, "model");
        string supportPath = Path.Combine(Application.streamingAssetsPath, "tts_server/");
        string generatePythonPath = Path.Combine(supportPath, "do_talkie.py");
        string pythonPath = Path.Combine(supportPath, "run_python.bat");
        
        ProcessStartInfo generate = new()
        {
            FileName = pythonPath,
            Arguments = $"\"{generatePythonPath}\" --prompt_text \"{RefText}\" --prompt_speech_path \"{refPath}\" --talkies_file \"{voicelinesPath}\" --model_dir \"{modelPath}\" --save_dir \"{audioFolder}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using Process process = Process.Start(generate);
        string output = (await process.StandardOutput.ReadToEndAsync()).Trim();
        string error = await process.StandardError.ReadToEndAsync();

        await Task.Run(() => process.WaitForExit());
        
        if (!string.IsNullOrEmpty(error))
        {
            UnityEngine.Debug.LogError($"Python Error: {error}");
        }

        if (!string.IsNullOrEmpty(output))
        {
            UnityEngine.Debug.Log($"Python Output:\n{output}");
        }
    }

    public static IEnumerator Run(string audioFolder)
    {
        Debug.Log("Starting generation run");
        yield return GenerateAudioClips(audioFolder).AsIEnumerator();
    }
}

public static class TaskExtensions
{
    public static IEnumerator AsIEnumerator(this Task task)
    {
        while (!task.IsCompleted)
            yield return null;
        if (task.IsFaulted)
            throw task.Exception;
    }
}