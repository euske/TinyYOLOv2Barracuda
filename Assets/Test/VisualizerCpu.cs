using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UI = UnityEngine.UI;

namespace TinyYoloV2 {

sealed class VisualizerCpu : MonoBehaviour
{
    #region Editable attributes

    [SerializeField, Range(0, 1)] float _scoreThreshold = 0.1f;
    [SerializeField, Range(0, 1)] float _overlapThreshold = 0.2f;
    [SerializeField] ResourceSet _resources = null;
    [SerializeField] UI.RawImage _previewUI = null;
    [SerializeField] Marker _markerPrefab = null;
    [SerializeField] ARCameraManager _cameraManager = null;

    // Thresholds are exposed to the runtime UI.
    public float scoreThreshold { set => _scoreThreshold = value; }
    public float overlapThreshold { set => _overlapThreshold = value; }
    public ARCameraManager cameraManager { set => _cameraManager = value; }

    #endregion

    #region Internal objects

    XRCameraSubsystem _cameraSubsystem = null;
    WebCamTexture _webcamTexture = null;
    Texture2D _arcamTexture = null;

    RenderTexture _imageBuffer;
    ObjectDetector _detector;
    Marker[] _markers = new Marker[50];

    #endregion

    #region MonoBehaviour implementation

    void Start()
    {
        // Texture allocation
        _imageBuffer = new RenderTexture(Config.ImageSize, Config.ImageSize, 0);
        _previewUI.texture = _imageBuffer;

        // Object detector initialization
        _detector = new ObjectDetector(_resources);

        // Marker populating
        for (var i = 0; i < _markers.Length; i++)
            _markers[i] = Instantiate(_markerPrefab, _previewUI.transform);

        // Enable camera
        _cameraSubsystem = _cameraManager.subsystem;
        if (_cameraSubsystem != null) {
            Debug.Log("AR camera subsystem used.");
            _cameraManager.frameReceived += OnCameraFrameReceived;
        } else {
            Debug.Log("Regular webcam used.");
            _webcamTexture = new WebCamTexture();
            _webcamTexture.Play();
        }
    }

    void OnDisable()
    {
        if (_webcamTexture != null) {
            _webcamTexture.Stop();
        }
        if (_cameraManager != null) {
            _cameraManager.frameReceived -= OnCameraFrameReceived;
        }
    }

    void OnDestroy()
    {
        if (_arcamTexture != null) Destroy(_arcamTexture);
        if (_webcamTexture != null) Destroy(_webcamTexture);
        if (_imageBuffer != null) Destroy(_imageBuffer);
        for (var i = 0; i < _markers.Length; i++) Destroy(_markers[i]);
        _detector?.Dispose();
        _detector = null;
    }

    void OnCameraFrameReceived(ARCameraFrameEventArgs eventArgs)
    {
        Debug.Log("frame received.");
        // Attempt to get the latest camera image. If this method succeeds,
        // it acquires a native resource that must be disposed (see below).
        if (!_cameraSubsystem.TryAcquireLatestCpuImage(out XRCpuImage image)) return;

        // Once we have a valid XRCpuImage, we can access the individual image "planes"
        // (the separate channels in the image). XRCpuImage.GetPlane provides
        // low-overhead access to this data. This could then be passed to a
        // computer vision algorithm. Here, we will convert the camera image
        // to an RGBA texture and draw it on the screen.

        // Choose an RGBA format.
        // See XRCpuImage.FormatSupported for a complete list of supported formats.
        var format = TextureFormat.RGBA32;
        if (_arcamTexture == null ||
            _arcamTexture.width != image.width ||
            _arcamTexture.height != image.height) {
            _arcamTexture = new Texture2D(image.width, image.height, format, false);
        }

        // Convert the image to format, flipping the image across the Y axis.
        // We can also get a sub rectangle, but we'll get the full image here.
        XRCpuImage.Transformation transformation = XRCpuImage.Transformation.MirrorY;
        var conversionParams = new XRCpuImage.ConversionParams(image, format, transformation);

        // Texture2D allows us write directly to the raw texture data
        // This allows us to do the conversion in-place without making any copies.
        var rawTextureData = _arcamTexture.GetRawTextureData<byte>();
        try {
            image.Convert(conversionParams, rawTextureData);
        } finally {
            // We must dispose of the XRCpuImage after we're finished
            // with it to avoid leaking native resources.
            image.Dispose();
        }

        // Apply the updated texture data to our texture
        _arcamTexture.Apply();
    }

    void Update()
    {
        // Check if the webcam is ready (needed for macOS support)
        if (_webcamTexture != null) {
            if (_webcamTexture.width <= 16) return;

            // Input buffer update with aspect ratio correction
            var vflip = _webcamTexture.videoVerticallyMirrored;
            var aspect = (float)_webcamTexture.height / _webcamTexture.width;
            var scale = new Vector2(aspect, vflip ? -1 : 1);
            var offset = new Vector2((1 - aspect) / 2, vflip ? 1 : 0);
            Graphics.Blit(_webcamTexture, _imageBuffer, scale, offset);
        }
        if (_arcamTexture != null) {
            var aspect = (float)_arcamTexture.height / _arcamTexture.width;
            var scale = new Vector2(aspect, 1);
            var offset = new Vector2((1 - aspect) / 2, 0);
            Graphics.Blit(_arcamTexture, _imageBuffer, scale, offset);
        }

        // Run the object detector with the webcam input.
        _detector.ProcessImage
            (_imageBuffer, _scoreThreshold, _overlapThreshold);

        // Marker update
        var i = 0;
        foreach (var box in _detector.DetectedObjects) {
            if (i == _markers.Length) break;
            _markers[i++].SetAttributes(box);
        }
        for (; i < _markers.Length; i++) _markers[i].Hide();
    }

    #endregion
}

} // namespace TinyYoloV2
