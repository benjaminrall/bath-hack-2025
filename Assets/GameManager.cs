using UnityEngine;

public class GameManager : MonoBehaviour
{
    public int player1ID;
    public int player2ID;
    
    public PlayerController player1;
    public PlayerController player2;

    private PlayerData _player1Data;
    private PlayerData _player2Data;

    public float minY = -340;
    public float maxY = 340;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _player1Data = PlayerDataManager.Instance.PlayerDatas[player1ID];
        _player2Data = PlayerDataManager.Instance.PlayerDatas[player2ID];
        
        Debug.Log(_player1Data.bodyMaterial);
        Debug.Log(_player2Data.bodyMaterial);

        player1.UpdateMaterials(_player1Data.bodyMaterial, _player1Data.faceMaterial);
        player2.UpdateMaterials(_player2Data.bodyMaterial, _player2Data.faceMaterial);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
