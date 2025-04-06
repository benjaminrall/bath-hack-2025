using System.IO;
using UnityEngine;

public class MicCapture : MonoBehaviour
{
    private const int SampleRate = 44100;

    private AudioClip _audioClip;
    private AudioSource _audioSource;
    private bool _recording;
    private string _micName;

    public void StartRecording()
    {
        if (Microphone.devices.Length > 0)
        {
            // Choose the first microphone by default
            _micName = Microphone.devices[0];

            // Start recording
            _audioClip = Microphone.Start(_micName, false, 10, SampleRate);
            _recording = true;
            Debug.Log("Recording started");
        }
        else
        {
            Debug.LogError("No microphone detected");
        }
    }

    // Crop the audio clip to the actual recorded length
    private void CropAudioClip(int length)
    {
        // Create a new AudioClip with the exact length of the recording
        AudioClip croppedClip = AudioClip.Create(_audioClip.name, length, _audioClip.channels, _audioClip.frequency, false);
        
        // Get the audio samples from the original clip (as an array of floats)
        float[] samples = new float[length];
        _audioClip.GetData(samples, 0);

        // Set the data in the new cropped AudioClip
        croppedClip.SetData(samples, 0);

        // Replace the original _audioClip with the cropped version
        _audioClip = croppedClip;
    }
    
    public void StopRecording()
    {
        if (!_recording) return;
        
        int position = Microphone.GetPosition(_micName);
        
        Microphone.End(_micName);
        _recording = false;
        RecordedAudio = true;
        CropAudioClip(position);
    }

    public bool PlayRecordedClip()
    {
        if (_audioClip == null || (_audioSource != null && _audioSource.isPlaying)) return false;
        
        _audioSource = gameObject.AddComponent<AudioSource>();
        _audioSource.clip = _audioClip;
        _audioSource.loop = false;
        _audioSource.Play();
        return true;
    }

    public float GetClipLength()
    {
        return _audioClip.length;
    }

    public void StopRecordedClip()
    {
        if (_audioSource == null) return;
        
        _audioSource.Stop();
    }
    
    public bool RecordedAudio { get; private set; }

    public void SaveAudio()
    {
        string path = Path.Combine(Application.persistentDataPath, "recorded_audio.wav");
        AudioSaver.ExportClipData(_audioClip, path);
    }
}
