using UnityEngine;

namespace TinyYoloV2
{
    public static class Config
    {
        public const int ImageSize = 416;
        public const int CellsInRow = 26;
        public const int AnchorCount = 3;
        public const int ClassCount = 80;

        public const int InputSize = ImageSize * ImageSize * 3;
        public const int TotalCells = CellsInRow * CellsInRow;
        public const int OutputPerCell = AnchorCount * (5 + ClassCount);
        public const int MaxBBoxes = TotalCells * AnchorCount;
        public static Vector2[] Anchors = new Vector2[] {
            new Vector2(81f, 82f),
            new Vector2(135f, 169f),
            new Vector2(344f, 319f),
        };

        public static string[] _labels = new[]
        {
            "Plane", "Bicycle", "Bird", "Boat",
            "Bottle", "Bus", "Car", "Cat",
            "Chair", "Cow", "Table", "Dog",
            "Horse", "Motorbike", "Person", "Plant",
            "Sheep", "Sofa", "Train", "TV"
        };

        public static string GetLabel(int index)
          => _labels[index];
    }
}
