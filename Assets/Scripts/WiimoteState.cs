
using System.Collections.Generic;
using UnityEngine;
using WiimoteApi;

public class WiimoteState
{
    private static int _activeWiimotes;
    
    /// Button events to be associated with each button state.
    private static readonly WiimoteEvent[][] ButtonEvents =
    {
        new[] { WiimoteEvent.A_DOWN, WiimoteEvent.A_UP },
        new[] { WiimoteEvent.B_DOWN, WiimoteEvent.B_UP },
        new[] { WiimoteEvent.D_LEFT_DOWN, WiimoteEvent.D_LEFT_UP },
        new[] { WiimoteEvent.D_RIGHT_DOWN, WiimoteEvent.D_RIGHT_UP },
        new[] { WiimoteEvent.D_UP_DOWN, WiimoteEvent.D_UP_UP },
        new[] { WiimoteEvent.D_DOWN_DOWN, WiimoteEvent.D_DOWN_UP },
        new[] { WiimoteEvent.ONE_DOWN, WiimoteEvent.ONE_UP },
        new[] { WiimoteEvent.TWO_DOWN, WiimoteEvent.TWO_UP },
        new[] { WiimoteEvent.PLUS_DOWN, WiimoteEvent.PLUS_UP },
        new[] { WiimoteEvent.MINUS_DOWN, WiimoteEvent.MINUS_UP },
        new[] { WiimoteEvent.HOME_DOWN, WiimoteEvent.HOME_UP },
    };
    
    private readonly Wiimote _wiimote;
    private readonly int _playerID;
    private Vector2 _smoothedPointingPosition;
    private const float PointerSmoothingFactor = 0.05f;

    private bool[] _buttonStates = new bool[11];


    public WiimoteState(Wiimote wiimote)
    {
        _wiimote = wiimote;
        _playerID = _activeWiimotes;
        _activeWiimotes++;

        InitialiseWiimote();
    }

    /// Initialises the Wiimote, including attempting to set-up Wii Motion Plus
    private void InitialiseWiimote()
    {
        // Sets the Player LEDs according to their player number
        bool[] playerLights = new bool[4];
        playerLights[_playerID] = true;
        _wiimote.SendPlayerLED(playerLights[0], playerLights[1], playerLights[2], playerLights[3]);

        string debugPreface = "Player " + _playerID + ": ";
        
        // Attempts to activate the Wiimote's WMP
        if (_wiimote.RequestIdentifyWiiMotionPlus() && _wiimote.wmp_attached)
        {
            Debug.Log(debugPreface + "Found WMP extension attached.");
            if (_wiimote.ActivateWiiMotionPlus())
            {
                Debug.Log(debugPreface + "Sent WMP activation request.");
                if (_wiimote.current_ext == ExtensionController.MOTIONPLUS)
                {
                    Debug.Log(debugPreface + "WMP activated successfully.");
                }
            }
        }
        
        // Attempts to set up the Wiimote's IR camera
        if (_wiimote.SetupIRCamera())
        {
            Debug.Log(debugPreface + "Sent IR camera activation commands.");
        }
    }

    /// Updates the stored Wiimote's data.
    /// Should be done each Update before accessing events or IR data.
    public void UpdateWiimoteData()
    {
        int ret;
        do
        {
            ret = _wiimote.ReadWiimoteData();
        } while (ret > 0);
    }

    /// Gets an array of button states given ButtonData from the Wiimote. 
    private bool[] GetButtonStates(ButtonData buttonData) => new[] {
        buttonData.a,
        buttonData.b,
        buttonData.d_left,
        buttonData.d_right,
        buttonData.d_up,
        buttonData.d_down,
        buttonData.one,
        buttonData.two,
        buttonData.plus,
        buttonData.minus,
        buttonData.home
    };


    /// Returns a list of Wiimote button events.
    public IEnumerable<WiimoteEvent> GetEvents()
    {
        List<WiimoteEvent> events = new();

        // Gets new button states
        bool[] newButtonsState = GetButtonStates(_wiimote.Button);

        // Compares button states and adds relevant events
        for (int i = 0; i < _buttonStates.Length; i++)
        {
            if (newButtonsState[i] && !_buttonStates[i])
                events.Add(ButtonEvents[i][0]);
            if (_buttonStates[i] && !newButtonsState[i])
                events.Add(ButtonEvents[i][1]);
        }

        // Updates stored button states
        _buttonStates = newButtonsState;

        return events;
    }

    /// Returns the current (x, y) pointer position of the Wiimote
    public Vector2 GetPointingPosition()
    {
        float[] rawPosition = _wiimote.Ir.GetPointingPosition();
        Vector2 position = new(rawPosition[0], rawPosition[1]);

        _smoothedPointingPosition = 
            Vector2.Lerp(_smoothedPointingPosition, position, PointerSmoothingFactor);

        return _smoothedPointingPosition;
    }

    /// Returns the most probable two positions of the sensor bar sensors.
    public float[,] GetSensorBarPositions()
    {
        float[,] ir = _wiimote.Ir.GetProbableSensorBarIR();

        float[,] sensorBarPos = new float[2, 2];
        for (int i = 0; i < 2; i++)
        {
            // Gets x and y positions of the given IR point
            float x = ir[i, 0] / 1023f;
            float y = ir[i, 1] / 767f;

            // Accounts for IR readings outside of the window
            if (x < 0 || y < 0 || x > 1 || y > 1)
            {
                sensorBarPos[i, 0] = -1;
                sensorBarPos[i, 1] = -1;
            }
            
            // Stores the adjusted IR readings
            sensorBarPos[i, 0] = x;
            sensorBarPos[i, 1] = y;
        }
        return sensorBarPos;
    }

    /// Cleans up the Wiimote - only to be used on application quit.
    public void Cleanup()
    {
        WiimoteManager.Cleanup(_wiimote);
    }
}