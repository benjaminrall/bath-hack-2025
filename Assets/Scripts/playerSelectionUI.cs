using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSelectionUI : MonoBehaviour
{
    public GameObject playerButtonPrefab;
    public Transform buttonContainer; // Parent object in UI to hold buttons
    public TextMeshProUGUI nameBox;
    public bool isPlayer1;

    private List<GameObject> _spawnedButtons = new();

    void Start()
    {
        LoadPlayerButtons();
    }

    void LoadPlayerButtons()
    {
        List<PlayerData> players = PlayerDataManager.Instance.PlayerDatas;
        if (players == null || players.Count == 0)
        {
            Debug.LogError("No players found!");
            return;
        }

        for (int i = 0; i < players.Count; i++)
        {
            Debug.Log($"Loading player {i}: {players[i].playerName}");
            int playerIndex = i;
            PlayerData player = players[playerIndex];

            GameObject newButton = Instantiate(playerButtonPrefab, buttonContainer);
            Image iconImage = newButton.GetComponentInChildren<Image>();
            if (iconImage != null)
            {
                iconImage.sprite = player.faceIcon;
            }

            Button button = newButton.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() =>
                {
                    nameBox.text = player.playerName;
                    if (isPlayer1)
                    {
                        PlayerPrefs.SetInt("Player1Index", playerIndex);
                    }
                    else
                    {
                        PlayerPrefs.SetInt("Player2Index", playerIndex);
                    }
                    PlayerPrefs.SetInt("SelectedPlayerIndex", playerIndex);
                    PlayerPrefs.Save();
                });
            }

            _spawnedButtons.Add(newButton);
        }
    }
}
