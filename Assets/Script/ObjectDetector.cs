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

    private List<BoundingBox> _objects = new List<BoundingBox>();
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
        using (var tensor = new Tensor(1, imageSize, imageSize, 3, _preBuffer)) {
            _worker.Execute(tensor);
        }

        // Output tensor (26x26x255)
        _objects.Clear();
        using (var tensor = _worker.PeekOutput()) {
            for (int i = 0; i < Config.CellsInRow; i++) {
                for (int j = 0; j < Config.CellsInRow; j++) {
                    for (int k = 0; k < Config.AnchorCount; k++) {
                        int b = Config.OutputPerCell * k;
                        float x = j + Sigmoid(tensor[0,i,j,b+0]);
                        float y = i + Sigmoid(tensor[0,i,j,b+1]);
                        float w = Config.Anchors[k].x * Mathf.Exp(tensor[0,i,j,b+2]);
                        float h = Config.Anchors[k].y * Mathf.Exp(tensor[0,i,j,b+3]);
                        float conf = Sigmoid(tensor[0,i,j,b+4]);
                        float totalProb = 0;
                        float maxProb = 0;
                        int maxIndex = 0;
                        for (int index = 0; index < Config.ClassCount; index++) {
                            float p = Mathf.Exp(tensor[0,i,j,b+5+index]);
                            if (maxProb < p) {
                                maxProb = p; maxIndex = index;
                            }
                            totalProb += p;
                        }
                        float score = conf * maxProb / totalProb;
                        if (scoreThreshold <= score) {
                            BoundingBox box = new BoundingBox {
                                x = x / Config.CellsInRow,
                                y = y / Config.CellsInRow,
                                w = w / Config.ImageSize,
                                h = h / Config.ImageSize,
                                classIndex = (uint)maxIndex,
                                score = score,
                            };
                            _objects.Add(box);
                        }
                    }
                }
            }
        }
    }

    private float Sigmoid(float x) {
        return 1f/(1f+Mathf.Exp(-x));
    }

    #endregion
}

} // namespace TinyYoloV2
