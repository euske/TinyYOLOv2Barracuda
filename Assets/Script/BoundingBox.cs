using System.Runtime.InteropServices;

namespace TinyYoloV2
{
    //
    // Bounding box structure - The layout of this structure must be matched
    // with the one defined in Common.hlsl.
    //
    [StructLayout(LayoutKind.Sequential)]
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
    };
}
