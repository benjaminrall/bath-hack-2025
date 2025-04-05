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
    
    private bool _initialised;
    
    private void Start()
    {
        WiimoteManager.FindWiimotes();
    }
    
    private bool TryInitialise()
    {
        if (WiimoteManager.Wiimotes.Count < 2) return false;
        
        _player1Wiimote = new WiimoteState(WiimoteManager.Wiimotes[0]);
        _player2Wiimote = new WiimoteState(WiimoteManager.Wiimotes[1]);
        Debug.Log("Initialised Wiimotes");

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
        
        Vector2 pointer1 = _player1Wiimote.GetPointingPosition();
        player1Pointer.anchorMin = pointer1;
        player1Pointer.anchorMax = pointer1;
        
        foreach (WiimoteEvent wiimoteEvent in _player1Wiimote.GetEvents())
        {
            Debug.Log(wiimoteEvent);
        }
        
        Vector2 pointer2 = _player2Wiimote.GetPointingPosition();
        player2Pointer.anchorMin = pointer2;
        player2Pointer.anchorMax = pointer2;
        
        foreach (WiimoteEvent wiimoteEvent in _player2Wiimote.GetEvents())
        {
            Debug.Log(wiimoteEvent);
        }
    }

    private void OnApplicationQuit()
    {
        _player1Wiimote?.Cleanup();
        _player2Wiimote?.Cleanup();
    }
}
