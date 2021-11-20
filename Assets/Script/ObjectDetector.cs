using System.Collections.Generic;
using Unity.Barracuda;
using UnityEngine;

namespace TinyYoloV2 {

public sealed class ObjectDetector : System.IDisposable
{
    #region Internal objects

    ResourceSet _resources;
    ComputeBuffer _preBuffer;
    IWorker _worker;

    #endregion

    #region Public constructor

    public ObjectDetector(ResourceSet resources)
    {
        _resources = resources;
        _preBuffer = new ComputeBuffer(Config.InputSize, sizeof(float));
        _worker = ModelLoader.Load(_resources.model).CreateWorker();
    }

    #endregion

    #region IDisposable implementation

    public void Dispose()
    {
        _preBuffer?.Dispose();
        _preBuffer = null;

        _worker?.Dispose();
        _worker = null;
    }

    #endregion

    #region Public accessors

    private BoundingBox[] _objects = new BoundingBox[Config.MaxDetection];
    public IEnumerable<BoundingBox> DetectedObjects
      => _objects;

    #endregion

    #region Main image processing function

    public void ProcessImage
      (Texture sourceTexture, float scoreThreshold, float overlapThreshold)
    {
        // Preprocessing
        var pre = _resources.preprocess;
        var imageSize = Config.ImageSize;
        pre.SetTexture(0, "_Texture", sourceTexture);
        pre.SetBuffer(0, "_Tensor", _preBuffer);
        pre.SetInt("_ImageSize", imageSize);
        pre.Dispatch(0, imageSize / 8, imageSize / 8, 1);

        // Run the YOLO model.
        using (var tensor = new Tensor(1, imageSize, imageSize, 3, _preBuffer))
            _worker.Execute(tensor);

        // Output tensor (13x13x125) -> Temporary render texture (125x169)
    }

    #endregion
}

} // namespace TinyYoloV2
