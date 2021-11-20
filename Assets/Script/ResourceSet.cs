using UnityEngine;
using Unity.Barracuda;

namespace TinyYoloV2
{
    [CreateAssetMenu(fileName = "TinyYOLOv2",
                     menuName = "ScriptableObjects/TinyYOLOv2 Resource Set")]
    public sealed class ResourceSet : ScriptableObject
    {
        public NNModel model;
        public ComputeShader preprocess;
    }
}
