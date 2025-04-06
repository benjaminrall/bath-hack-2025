
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerData
{
    public int id;
    public string playerName;
    public Material bodyMaterial;
    public Material faceMaterial;
    public Sprite faceIcon;
    public Dictionary<string, AudioClip> AudioClips;
    
    public PlayerData(int id, string playerName, Material bodyMaterial, Material faceMaterial, Sprite faceIcon, Dictionary<string, AudioClip> audioClips)
    {
        this.id = id;
        this.playerName = playerName;
        this.bodyMaterial = bodyMaterial;
        this.faceMaterial = faceMaterial;
        this.faceIcon = faceIcon;
        AudioClips = audioClips;
    }
}


