using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using NAudio.Wave;

[Serializable]
public class PlayerJson
{
    public string playerName;
    public Color colour;

    // Constructor for easy initialization
    public PlayerJson(string playerName, Color colour)
    {
        this.playerName = playerName;
        this.colour = colour;
    }
}

public class PlayerDataManager : MonoBehaviour
{
    public static string PlayerDataStore => Path.Combine(Application.persistentDataPath, "playerdata");
    
    private static PlayerDataManager _instance;
    public static PlayerDataManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<PlayerDataManager>();

                if (_instance == null)
                {
                    GameObject singletonObject = new GameObject("PlayerDataManager");
                    _instance = singletonObject.AddComponent<PlayerDataManager>();
                }
            }

            return _instance;
        }
    }
    
    private int _playerCount;
    public List<PlayerData> PlayerDatas;
    
    public Material templateMaterial;
    
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        
        LoadAllPlayers();
    }

    public string[] AddPlayer(string playerName, Color colour, string faceMapPath, string faceIconPath)
    {
        int id = _playerCount;
        _playerCount++;
        string playerFolder = Path.Combine(PlayerDataStore, id.ToString());
        string audioFolder = Path.Combine(playerFolder, "audios");

        Directory.CreateDirectory(playerFolder);
        Directory.CreateDirectory(audioFolder);

        string json = JsonUtility.ToJson(new PlayerJson(playerName, colour));

        string jsonPath = Path.Combine(playerFolder, "data.json");
        
        File.WriteAllText(jsonPath, json);
        
        CopyFileToFolder(faceMapPath, playerFolder, "map.png");
        CopyFileToFolder(faceIconPath, playerFolder, "icon.png");
        CopyFileToFolder(Path.Combine(Application.persistentDataPath, "recorded_audio.wav"), playerFolder, "reference.wav");
        
        return new [] { Path.Combine(playerFolder, "reference.wav"), audioFolder };
    }

    public PlayerData LoadPlayerData(int playerID)
    {
        string playerFolder = Path.Combine(PlayerDataStore, playerID.ToString());
        string jsonPath = Path.Combine(playerFolder, "data.json");
        string faceMatPath = Path.Combine(playerFolder, "map.png");
        string faceIconPath = Path.Combine(playerFolder, "icon.png");
        string audioFolder = Path.Combine(playerFolder, "audios");
        
        string json = File.ReadAllText(jsonPath);
        PlayerJson playerJson = JsonUtility.FromJson<PlayerJson>(json);

        Material bodyMaterial = new(templateMaterial)
        {
            color = playerJson.colour
        };

        Texture2D faceTexture = LoadTextureFromFile(faceMatPath);
        Material faceMaterial = new(templateMaterial)
        {
            mainTexture = faceTexture
        };

        Texture2D faceIconTexture = LoadTextureFromFile(faceIconPath);
        Sprite faceIcon = Sprite.Create(faceIconTexture,
            new Rect(0, 0, faceIconTexture.width, faceIconTexture.height), new Vector2(0.5f, 0.5f));
        
        PlayerData playerData = new(playerID, playerJson.playerName, bodyMaterial, faceMaterial, faceIcon, GetAudioClips(audioFolder));
        return playerData;
    }

    private Dictionary<string, AudioClip> GetAudioClips(string audioFolder)
    {
        Dictionary<string, AudioClip> audioClips = new();

        string[] files = Directory.GetFiles(audioFolder, "*.wav");

        foreach (string file in files)
        {
            string fileName = Path.GetFileNameWithoutExtension(file);

            AudioClip clip = LoadAudioClipFromWavFile(file);
            if (clip != null)
            {
                audioClips.Add(fileName, clip);
                //Debug.Log("Loaded audio: " + fileName);
            }
        }

        return audioClips;
    }
    
    Texture2D LoadTextureFromFile(string path)
    {
        if (File.Exists(path))
        {
            byte[] fileData = File.ReadAllBytes(path);
            Texture2D texture = new(2, 2); // Create a texture of any size; Unity will resize it
            texture.LoadImage(fileData);  // Load the image data into the texture
            return texture;
        }

        Debug.LogError("File not found at: " + path);
        return null;
    }
    // Synchronously load WAV file into an AudioClip
    private AudioClip LoadAudioClipFromWavFile(string filePath)
    {
        // Read the WAV file into a byte array
        byte[] wavFileData = File.ReadAllBytes(filePath);

        // Use NAudio to read the WAV data
        using (var memoryStream = new MemoryStream(wavFileData))
        using (var reader = new WaveFileReader(memoryStream))
        {
            // Convert WAV data into a float array (Unity expects data in floats between -1 and 1)
            int sampleCount = (int)reader.Length / 2; // 16-bit PCM has 2 bytes per sample
            float[] samples = new float[sampleCount];
            int sampleIndex = 0;

            // Read the samples into the float array
            while (reader.Position < reader.Length)
            {
                // Read one frame (one sample for each channel)
                float[] sampleFrame = new float[reader.WaveFormat.Channels];
                sampleFrame = reader.ReadNextSampleFrame();

                // Assuming we have mono or stereo, mix down stereo to mono
                if (reader.WaveFormat.Channels == 1)
                {
                    samples[sampleIndex] = sampleFrame[0]; // Mono
                }
                else if (reader.WaveFormat.Channels == 2)
                {
                    // Average left and right channels for stereo to mono
                    samples[sampleIndex] = (sampleFrame[0] + sampleFrame[1]) / 2.0f;
                }

                sampleIndex++;
            }

            // Create the AudioClip
            AudioClip clip = AudioClip.Create(Path.GetFileNameWithoutExtension(filePath), sampleCount, reader.WaveFormat.Channels, reader.WaveFormat.SampleRate, false);
            clip.SetData(samples, 0);

            return clip;
        }
    }
    
    //
    public void LoadAllPlayers()
    {
        PlayerDatas = new List<PlayerData>();
        _playerCount = Directory.GetDirectories(PlayerDataStore).Length;
        for (int i = 0; i < _playerCount; i++)
        {
            PlayerDatas.Add(LoadPlayerData(i));
        }
    }

    public void RefreshNewPlayers()
    {
        int playersStored = Directory.GetDirectories(PlayerDataStore).Length;
        
        if (_playerCount == playersStored) return;

        for (int i = _playerCount; i < playersStored; i++)
        {
            PlayerDatas.Add(LoadPlayerData(i));
        }
    }

    public void RefreshAudioClips()
    {
        foreach (PlayerData playerData in PlayerDatas)
        {
            string playerFolder = Path.Combine(PlayerDataStore, playerData.id.ToString());
            string audioFolder = Path.Combine(playerFolder, "audios");
            playerData.AudioClips = GetAudioClips(audioFolder);
        }
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    private void CopyFileToFolder(string imagePath, string targetFolder, string targetFileName)
    {
        if (File.Exists(imagePath))
        {
            string targetPath = Path.Combine(targetFolder, targetFileName);
            try
            {
                File.Copy(imagePath, targetPath, true);  // Overwrite if the file exists
                Debug.Log($"Successfully copied {imagePath} to {targetPath}");
            }
            catch (IOException ex)
            {
                Debug.LogError($"Failed to copy file from {imagePath} to {targetPath}: {ex.Message}");
            }
        }
        else
        {
            Debug.LogWarning($"Image file {imagePath} does not exist!");
        }
    }
}
