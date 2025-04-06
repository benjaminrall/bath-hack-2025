using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

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

public class PlayerController : MonoBehaviour
{
    public static string PlayerDataStore => Path.Combine(Application.persistentDataPath, "playerdata");
    
    private static PlayerController _instance;
    public static PlayerController Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<PlayerController>();

                if (_instance == null)
                {
                    GameObject singletonObject = new GameObject("PlayerController");
                    _instance = singletonObject.AddComponent<PlayerController>();
                }
            }

            return _instance;
        }
    }
    
    private int _playerCount;

    public Material templateMaterial;
    
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public string AddPlayer(string playerName, Color colour, string faceMapPath, string faceIconPath)
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
        
        CopyImageToFolder(faceMapPath, playerFolder, "map.png");
        CopyImageToFolder(faceIconPath, playerFolder, "icon.png");

        return audioFolder;
    }

    private void ReadPlayerFolder()
    {
        
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
            new Rect(0, 0, faceTexture.width, faceTexture.height), new Vector2(0.5f, 0.5f));

        Dictionary<string, AudioClip> audioClips = new();
        
        
        
        PlayerData playerData = new(playerID, playerJson.playerName, bodyMaterial, faceMaterial, faceIcon, audioClips);
        return playerData;
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
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    private void CopyImageToFolder(string imagePath, string targetFolder, string targetFileName)
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
