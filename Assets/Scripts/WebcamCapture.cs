using System;
using UnityEngine;
using UnityEngine.UI;

public class WebcamCapture : MonoBehaviour
{
    public RawImage cameraPreview;
    public AspectRatioFitter aspectFitter;
    public RectTransform overlayFrame;

    private WebCamTexture _webcamTexture;
    private Texture2D _capturedImage;


    public void StartWebcamPreview()
    {
        if (_webcamTexture != null)
        {
            Destroy(_webcamTexture);
        }

        WebCamDevice[] devices = WebCamTexture.devices;
        if (devices.Length > 0)
        {
            _webcamTexture = new WebCamTexture(devices[0].name);
            cameraPreview.texture = _webcamTexture;
            _webcamTexture.Play();
        }
    }

    public void StopWebcamPreview()
    {
        if (_webcamTexture == null) return;
        
        if (_webcamTexture.isPlaying)
            _webcamTexture.Stop();

        cameraPreview.texture = null;

        Destroy(_webcamTexture); 
        _webcamTexture = null;
    }
    
    // Update is called once per frame
    private void Update()
    {
        // Update the aspect ratio of the camera preview
        if (_webcamTexture == null || !_webcamTexture.isPlaying) return;
        
        float ratio = (float) _webcamTexture.width / _webcamTexture.height;
        aspectFitter.aspectRatio = ratio;

    }

    public void Capture()
    {
        if (_webcamTexture == null || !_webcamTexture.isPlaying)
            return;

        // Capture the full camera image
        Texture2D fullImage = new(_webcamTexture.width, _webcamTexture.height);
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
        float textureW = -1 * (overlayWidth / previewWidth) * _webcamTexture.width;
        float textureH = (overlayHeight / previewHeight) * _webcamTexture.height;
        Debug.Log(textureX + ", " + textureY + ", " + textureW + ", " + textureH);

        // Convert to ints and clamp
        int texX = Mathf.Clamp((int)(textureX - textureW), 0, _webcamTexture.width - 1);
        int texY = Mathf.Clamp((int)textureY, 0, _webcamTexture.height - 1);
        int texW = Mathf.Clamp((int)textureW, 0, _webcamTexture.width - texX);
        int texH = Mathf.Clamp((int)textureH, 0, _webcamTexture.height - texY);
        Debug.Log(texX + ", " + texY + ", " + texW + ", " + texH);
        
        // Crop
        Color[] croppedPixels = fullImage.GetPixels(texX, texY, texW, texH);
        Texture2D croppedImage = new(texW, texH);
        croppedImage.SetPixels(croppedPixels);
        croppedImage.Apply();

        // save the image
        byte[] bytes = croppedImage.EncodeToPNG();
        string filePath = System.IO.Path.Combine(Application.persistentDataPath, "face.png");
        System.IO.File.WriteAllBytes(filePath, bytes);
        Debug.Log("Image saved to: " + filePath);

        // Get the facial landmarks from the cropped image
        FacialLandmarkDetector.GetFacialLandmarks();
    }
}
