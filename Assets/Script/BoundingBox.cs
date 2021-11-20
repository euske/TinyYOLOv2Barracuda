using System.Runtime.InteropServices;
using UnityEngine;

namespace TinyYoloV2
{
    //
    // Bounding box structure - The layout of this structure must be matched
    // with the one defined in Common.hlsl.
    //
    public struct BoundingBox
    {
        public float x, y, w, h;
        public uint classIndex;
        public float score;

        // sizeof(BoundingBox)
        public static int Size = 6 * sizeof(int);

        // String formatting
        public override string ToString()
          => $"({x},{y})-({w}x{h}):{classIndex}({score})";

        public float getIOU(BoundingBox b) {
            float x = Mathf.Max(this.x, b.x);
            float y = Mathf.Max(this.y, b.y);
            float w = Mathf.Min(this.x+this.w, b.x+b.w) - x;
            float h = Mathf.Min(this.y+this.h, b.y+b.h) - y;
            return (w*h)/(this.w*this.h);
        }
    };
}
