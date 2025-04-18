using UnityEngine;
using UnityEngine.UI;


public class camera_capture : MonoBehaviour
{
    public GameObject cameraPanel;
    public RawImage cameraPreview;
    public AspectRatioFitter aspectFitter;
    public RectTransform overlayFrame;
    public Button captureButton;
    public Button openCameraButton;
    public Button closeCameraButton;

    private WebCamTexture _webcamTexture;
    private Texture2D _capturedImage;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        cameraPanel.SetActive(false); // hide camera UI at start

        openCameraButton.onClick.AddListener(OpenCamera);
        captureButton.onClick.AddListener(CapturePhoto);
        closeCameraButton.onClick.AddListener(CloseCamera);
        
    }

    // open camera and start preview with default camera
    void OpenCamera()
    {
        cameraPanel.SetActive(true);

        if (_webcamTexture != null)
        {
            Destroy(_webcamTexture);
        }

        WebCamDevice[] devices = WebCamTexture.devices;
        if (devices.Length > 0)
        {
            _webcamTexture = new WebCamTexture(devices[0].name);
            cameraPreview.texture = _webcamTexture;
            cameraPreview.material.mainTexture = _webcamTexture;
            _webcamTexture.Play();
        }
    }

    void CloseCamera()
    {
        cameraPanel.SetActive(false);

        if (_webcamTexture != null)
        {
            if (_webcamTexture.isPlaying)
                _webcamTexture.Stop();

            cameraPreview.texture = null;
            cameraPreview.material.mainTexture = null;

            Destroy(_webcamTexture); 
            _webcamTexture = null;
        }
    }

    // Update is called once per frame
    void Update()
    {   
        // Update the aspect ratio of the camera preview
        if (_webcamTexture != null && _webcamTexture.isPlaying)
        {
            float ratio = (float) _webcamTexture.width / _webcamTexture.height;
            aspectFitter.aspectRatio = ratio;
        }
        
    }

    // Capture a photo from the webcam
    void CapturePhoto()
    {
        if (_webcamTexture == null || !_webcamTexture.isPlaying)
            return;

        // Capture the full camera image
        Texture2D fullImage = new Texture2D(_webcamTexture.width, _webcamTexture.height);
        fullImage.SetPixels(_webcamTexture.GetPixels());
        fullImage.Apply();

        // Get overlay bounds relative to the cameraPreview RawImage
        Vector3[] overlayCorners = new Vector3[4];
        overlayFrame.GetWorldCorners(overlayCorners);

        Vector3[] previewCorners = new Vector3[4];
        cameraPreview.rectTransform.GetWorldCorners(previewCorners);

        // Calculate overlay position relative to camera preview
        float previewWidth = previewCorners[2].x - previewCorners[0].x;
        float previewHeight = previewCorners[2].y - previewCorners[0].y;

        float overlayX = overlayCorners[0].x - previewCorners[0].x;
        float overlayY = overlayCorners[0].y - previewCorners[0].y;

        float overlayWidth = overlayCorners[2].x - overlayCorners[0].x;
        float overlayHeight = overlayCorners[2].y - overlayCorners[0].y;

        // Flip Y axis for texture space
        float textureX = (overlayX / previewWidth) * _webcamTexture.width;
        float textureY = (overlayY / previewHeight) * _webcamTexture.height;
        float textureW = (overlayWidth / previewWidth) * _webcamTexture.width;
        float textureH = (overlayHeight / previewHeight) * _webcamTexture.height;

        // Convert to ints and clamp
        int texX = Mathf.Clamp((int)textureX, 0, _webcamTexture.width - 1);
        int texY = Mathf.Clamp((int)textureY, 0, _webcamTexture.height - 1);
        int texW = Mathf.Clamp((int)textureW, 0, _webcamTexture.width - texX);
        int texH = Mathf.Clamp((int)textureH, 0, _webcamTexture.height - texY);

        // Crop
        Color[] croppedPixels = fullImage.GetPixels(texX, texY, texW, texH);
        Texture2D croppedImage = new Texture2D(texW, texH);
        croppedImage.SetPixels(croppedPixels);
        croppedImage.Apply();

        // save the image
        byte[] bytes = croppedImage.EncodeToPNG();
        string filePath = System.IO.Path.Combine(Application.persistentDataPath, "face.png");
        System.IO.File.WriteAllBytes(filePath, bytes);
        Debug.Log("Image saved to: " + filePath);

        // Get the facial landmarks from the cropped image
        FacialLandmarkDetector landmarkDetector = GetComponent<FacialLandmarkDetector>();
        if (landmarkDetector != null)
        {
            FacialLandmarkDetector.GetFacialLandmarks();
        }
        else
        {
            Debug.LogWarning("FacialLandmarkDetector component not found.");
        }
    }



    // Clean up the webcam texture when the script is destroyed
    void OnDestroy()
    {
        if (_webcamTexture != null)
        {
            _webcamTexture.Stop();
        }
    }
}
