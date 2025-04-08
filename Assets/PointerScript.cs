using UnityEngine;
using WiimoteApi;

public class PointerScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!WiimoteInitialiser.Instance.Initialised)
        {
            return;
        }

    }
}
