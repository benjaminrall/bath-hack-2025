using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using WiimoteApi;

public class WiimoteController : MonoBehaviour
{
    private WiimoteState _player1Wiimote;
    private WiimoteState _player2Wiimote;
    
    public RectTransform player1Pointer;
    public RectTransform player2Pointer;

    public RectTransform[] ir_dots;
    
    private bool _initialised;
    
    private void Start()
    {
        WiimoteManager.FindWiimotes();
    }

    private void GetWiimoteData(Wiimote wiimote)
    {
        int ret;
        do
        {
            ret = wiimote.ReadWiimoteData();
        } while (ret > 0);
    }

    private bool TryInitialise()
    {
        if (WiimoteManager.Wiimotes.Count < 2) return false;
        
        _player1Wiimote = new WiimoteState(WiimoteManager.Wiimotes[0]);
        _player2Wiimote = new WiimoteState(WiimoteManager.Wiimotes[1]);

        return true;
    }
    
    private void Update()
    {
        if (!_initialised)
        {
            _initialised = TryInitialise();
            return;
        }
        
        _player1Wiimote.UpdateWiimoteData();
        _player2Wiimote.UpdateWiimoteData();
        
        foreach (WiimoteEvent wiimoteEvent in _player1Wiimote.GetEvents())
        {
            Debug.Log(wiimoteEvent);
        }
        
        foreach (WiimoteEvent wiimoteEvent in _player2Wiimote.GetEvents())
        {
            Debug.Log(wiimoteEvent);
        }
        
        Vector2 pointer1 = _player1Wiimote.GetPointingPosition();
        player1Pointer.anchorMin = pointer1;
        player1Pointer.anchorMax = pointer1;
        Vector2 pointer2 = _player2Wiimote.GetPointingPosition();
        player2Pointer.anchorMin = pointer2;
        player2Pointer.anchorMax = pointer2;
        
        float[,] ir = _player1Wiimote.GetSensorBarPositions();
        for (int i = 0; i < 2; i++)
        {
            ir_dots[i].anchorMin = new Vector2(ir[i, 0], ir[i, 1]);
            ir_dots[i].anchorMax = new Vector2(ir[i, 0], ir[i, 1]);
        }
    }

    private void OnApplicationQuit()
    {
        _player1Wiimote?.Cleanup();
        _player2Wiimote?.Cleanup();
    }
}
