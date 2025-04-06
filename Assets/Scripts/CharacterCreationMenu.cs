using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using WiimoteApi;

public class CharacterCreationMenu : MonoBehaviour
{
    public GameObject keyboardOverlay;
    public GameObject imageOverlay;
    public GameObject recordOverlay;
    private bool _capturingImage;
    private bool _capturingMic;
    private bool _playingMic;
    private bool _lockedOverlays;

    public WebcamCapture webcamCapture;
    public Material miiShirt;

    public MicCapture micCapture;
    public GameObject micIcon;
    public GameObject stopMicIcon;
    public GameObject playIcon;
    public GameObject stopPlayIcon;
    private Coroutine _stopRecordingCoroutine;
    private Coroutine _stopPlaybackCoroutine;
    
    public TextMeshProUGUI displayName;
    public TextMeshProUGUI entryName;

    public RectTransform wiimotePointer;
    public Sprite wiimotePointSprite;
    public Sprite wiimoteGrabSprite;
    private Image _wiimoteImage;
    
    private string _name;

    private EventSystem _eventSystem;
    private PointerEventData _pointerData;

    private bool _wiimoteInitialised;
    private WiimoteState _wiimote;
    
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        WiimoteManager.FindWiimotes();
        
        keyboardOverlay.SetActive(false);
        imageOverlay.SetActive(false);
        recordOverlay.SetActive(false);
        
        micIcon.SetActive(true);
        stopMicIcon.SetActive(false);
        playIcon.SetActive(true);
        stopPlayIcon.SetActive(false);

        _name = "";
        displayName.text = _name;
        
        _eventSystem = EventSystem.current;
        _pointerData = new PointerEventData(_eventSystem);

        _lockedOverlays = false;
        _capturingImage = false;
        
        _wiimoteImage = wiimotePointer.GetComponent<Image>();
        _wiimoteImage.sprite = wiimotePointSprite;
    }

    // Update is called once per frame
    void Update()
    {
        if (!_wiimoteInitialised)
        {
            _wiimoteInitialised = TryInitialiseWiimote();
            return;
        }
        
        _wiimote.UpdateWiimoteData();
        
        // Sets pointer position
        Vector2 pointer = _wiimote.GetPointingPosition();
        wiimotePointer.anchorMin = pointer;
        wiimotePointer.anchorMax = pointer;

        Vector2 screenPos = new Vector2(pointer.x * Screen.width, pointer.y * Screen.height);
        _pointerData.position = screenPos;
        List<RaycastResult> raycastResults = new();
        _eventSystem.RaycastAll(_pointerData, raycastResults);

        bool foundHits = raycastResults.Count > 0;

        EventSystem.current.SetSelectedGameObject(raycastResults.Count > 0 ? raycastResults[0].gameObject : null);

        // Handles Wiimote events
        foreach (WiimoteEvent wiimoteEvent in _wiimote.GetEvents())
        {
            switch (wiimoteEvent)
            {
                case WiimoteEvent.A_DOWN:
                    _wiimoteImage.sprite = wiimoteGrabSprite;
                    break;
                case WiimoteEvent.A_UP:
                    if (_capturingImage) SubmitImageCapture(true);

                    foreach (RaycastResult result in raycastResults) {
                        
                        Button button = result.gameObject.GetComponent<Button>();
                        if (button == null)
                        {
                            button = result.gameObject.GetComponentInParent<Button>();
                            if (button == null) continue;
                        };
                        
                        button.onClick.Invoke();
                        break;
                    }
                    _wiimoteImage.sprite = wiimotePointSprite;
                    break;
                case WiimoteEvent.B_UP:
                    if (_capturingImage) SubmitImageCapture(false);
                    break;
            }
        }
    }
    
    private bool TryInitialiseWiimote()
    {
        if (WiimoteManager.Wiimotes.Count < 1) return false;
        
        _wiimote = new WiimoteState(WiimoteManager.Wiimotes[0]);
        Debug.Log("Initialised Wiimote");

        return true;
    }

    public void OpenKeyboard()
    {
        if (_lockedOverlays) return;
        _lockedOverlays = true;
        keyboardOverlay.SetActive(true);

        entryName.text = _name;
    }

    private string ToFirstLetterUpper(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return "";

        input = input.ToLower(); // Make the whole string lowercase
        return char.ToUpper(input[0]) + input[1..];
    }
    
    public void PressKey(string letter)
    {
        if (_name.Length > 15) return;
        _name += letter;
        _name = ToFirstLetterUpper(_name);
        entryName.text = _name;
    }

    public void Backspace()
    {
        if (string.IsNullOrWhiteSpace(_name)) return;
        _name = _name[..^1];
        _name = ToFirstLetterUpper(_name);
        entryName.text = _name;
    }
    
    public void SubmitKeyboard()
    {
        _lockedOverlays = false;
        keyboardOverlay.SetActive(false);
        displayName.text = _name;
    }

    public void OpenImageCapture()
    {
        if (_lockedOverlays) return;
        _lockedOverlays = true;
        _capturingImage = true;
        imageOverlay.SetActive(true);
        webcamCapture.StartWebcamPreview();
    }

    public void SubmitImageCapture(bool captureImage)
    {
        _lockedOverlays = false;
        _capturingImage = false;
        imageOverlay.SetActive(false);
        
        if (!captureImage) return;

        webcamCapture.Capture();
    }

    public void OpenRecording()
    {
        if (_lockedOverlays) return;
        _lockedOverlays = true;
        recordOverlay.SetActive(true);
    }

    public void ToggleRecording()
    {
        if (_playingMic)
        {
            TogglePlayback();
        }
        
        if (_capturingMic)
        {
            _capturingMic = false;
            micIcon.SetActive(true);
            stopMicIcon.SetActive(false);
            micCapture.StopRecording();

            if (_stopRecordingCoroutine != null)
            {
                StopCoroutine(_stopRecordingCoroutine);
                _stopRecordingCoroutine = null;
            }
        }
        else
        {
            _capturingMic = true;
            micIcon.SetActive(false);
            stopMicIcon.SetActive(true);
            micCapture.StartRecording();

            _stopRecordingCoroutine = StartCoroutine(StopRecordingAfterDelay(10.0f));
        }
    }

    private IEnumerator StopRecordingAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (_capturingMic) ToggleRecording();
    }

    public void TogglePlayback()
    {
        if (_capturingMic)
        {
            ToggleRecording();
        }
        
        if (_playingMic)
        {
            _playingMic = false;
            playIcon.SetActive(true);
            stopPlayIcon.SetActive(false);
            micCapture.StopRecordedClip();
            
            if (_stopPlaybackCoroutine != null)
            {
                StopCoroutine(_stopPlaybackCoroutine);
                _stopPlaybackCoroutine = null;
            }
        }
        else
        {
            bool play = micCapture.PlayRecordedClip();
         
            if (!play) return;
            
            _playingMic = true;
            playIcon.SetActive(false);
            stopPlayIcon.SetActive(true);
            Debug.Log(micCapture.GetClipLength());
            _stopPlaybackCoroutine = StartCoroutine(StopPlayingAfterDelay(micCapture.GetClipLength()));
        }
    }
    
    private IEnumerator StopPlayingAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (_playingMic) TogglePlayback();
    }
    
    
    public void SubmitRecording()
    {
        _lockedOverlays = false;
        recordOverlay.SetActive(false);
    }

    public void ChangeColour(GameObject colourObject)
    {
        Color colour = colourObject.GetComponent<Image>().color;
        miiShirt.color = colour;
    }

    public void SavePlayerData()
    {
        micCapture.SaveAudio();
        
    }
}
