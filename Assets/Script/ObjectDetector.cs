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
        List<BoundingBox> cands = new List<BoundingBox>();
        Vector2[] anchors = Config.GetAnchors();
        int classCount = Config.GetLabels().Length;
        using (var t = _worker.PeekOutput()) {
            for (int y0 = 0; y0 < Config.CellsInRow; y0++) {
                for (int x0 = 0; x0 < Config.CellsInRow; x0++) {
                    for (int k = 0; k < anchors.Length; k++) {
                        Vector2 anchor = anchors[k];
                        int b = (5+classCount) * k;
                        float x = (x0 + Sigmoid(t[0,y0,x0,b+0])) / Config.CellsInRow;
                        float y = (y0 + Sigmoid(t[0,y0,x0,b+1])) / Config.CellsInRow;
                        float w = (anchor.x * Mathf.Exp(t[0,y0,x0,b+2])) / Config.CellsInRow;
                        float h = (anchor.y * Mathf.Exp(t[0,y0,x0,b+3])) / Config.CellsInRow;
                        float conf = Sigmoid(t[0,y0,x0,b+4]);
                        float maxProb = -1;
                        int maxIndex = 0;
                        for (int index = 0; index < classCount; index++) {
                            float p = Sigmoid(t[0,y0,x0,b+5+index]);
                            if (maxProb < p) {
                                maxProb = p; maxIndex = index;
                            }
                        }
                        float score = conf * maxProb;
                        if (scoreThreshold <= score) {
                            BoundingBox box = new BoundingBox {
                                x = x, y = y,
                                w = w, h = h,
                                classIndex = (uint)maxIndex,
                                score = score,
                            };
                            cands.Add(box);
                        }
                    }
                }
            }
        }

        // Apply Soft-NMS.
        _objects.Clear();
        Dictionary<BoundingBox, float> cscore = new Dictionary<BoundingBox, float>();
        foreach (BoundingBox box in cands) {
            cscore[box] = box.score;
        }
        while (0 < cands.Count) {
            // argmax(cscore[box])
            float mscore = -1;
            int mi = 0;
            for (int i = 0; i < cands.Count; i++) {
                float score = cscore[cands[i]];
                if (mscore < score) {
                    mscore = score; mi = i;
                }
            }
            if (mscore < overlapThreshold) break;
            BoundingBox box = cands[mi];
            _objects.Add(box);
            cands.RemoveAt(mi);
            for (int i = 0; i < cands.Count; i++) {
                BoundingBox b1 = cands[i];
                float v = box.getIOU(b1);
                cscore[b1] *= Mathf.Exp(-3*v*v);
            }
        }
    }

    private float Sigmoid(float x) {
        return 1f/(1f+Mathf.Exp(-x));
    }

    #endregion
}

} // namespace TinyYoloV2
