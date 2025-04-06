using UnityEngine;
using WiimoteApi;

public class WiimoteInitialiser : MonoBehaviour
{
    public bool Initialised { get; private set; }

    private static WiimoteInitialiser _instance;

    public WiimoteState Wiimote1 { get; private set; }
    public WiimoteState Wiimote2 { get; private set;  }
    
    public static WiimoteInitialiser Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<WiimoteInitialiser>();

                if (_instance == null)
                {
                    GameObject singletonObject = new GameObject("WiimoteInitialiser");
                    _instance = singletonObject.AddComponent<WiimoteInitialiser>();
                }
            }

            return _instance;
        }
    }
    
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
        
        WiimoteManager.FindWiimotes();
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private bool TryInitialiseWiimote()
    {
        if (WiimoteManager.Wiimotes.Count < 2) return false;
        
        Wiimote1 = new WiimoteState(WiimoteManager.Wiimotes[0]);
        Wiimote2 = new WiimoteState(WiimoteManager.Wiimotes[1]);
        Debug.Log("Initialised Wiimote");

        return true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!Initialised)
        {
            Initialised = TryInitialiseWiimote();
        }
    }
}
