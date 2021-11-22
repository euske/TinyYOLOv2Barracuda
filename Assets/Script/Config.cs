using UnityEngine;

namespace TinyYoloV2
{
    public static class Config
    {
        public const int ImageSize = 416;
        public const int CellsInRow = 13;
        public const int AnchorCount = 5;
        public const int ClassCount = 20;

        public const int InputSize = ImageSize * ImageSize * 3;

        private static Vector2[] _anchorsV2 = new Vector2[] {
            new Vector2(1.08f, 1.19f),
            new Vector2(3.42f, 4.41f),
            new Vector2(6.63f, 11.38f),
            new Vector2(9.42f, 5.11f),
            new Vector2(16.62f, 10.52f),
        };
        private static Vector2[] _anchorsV3 = new Vector2[] {
            new Vector2(81f, 82f),
            new Vector2(135f, 169f),
            new Vector2(344f, 319f),
        };
        public static Vector2[] GetAnchors() {
            return _anchorsV2;
        }

        private static string[] _labelsV2 = new[]
        {
            "Plane", "Bicycle", "Bird", "Boat",
            "Bottle", "Bus", "Car", "Cat",
            "Chair", "Cow", "Table", "Dog",
            "Horse", "Motorbike", "Person", "Plant",
            "Sheep", "Sofa", "Train", "TV"
        };
        private static string[] _labelsV3 = new[]
        {
            "person",
            "bicycle",
            "car",
            "motorbike",
            "aeroplane",
            "bus",
            "train",
            "truck",
            "boat",
            "traffic light",
            "fire hydrant",
            "stop sign",
            "parking meter",
            "bench",
            "bird",
            "cat",
            "dog",
            "horse",
            "sheep",
            "cow",
            "elephant",
            "bear",
            "zebra",
            "giraffe",
            "backpack",
            "umbrella",
            "handbag",
            "tie",
            "suitcase",
            "frisbee",
            "skis",
            "snowboard",
            "sports ball",
            "kite",
            "baseball bat",
            "baseball glove",
            "skateboard",
            "surfboard",
            "tennis racket",
            "bottle",
            "wine glass",
            "cup",
            "fork",
            "knife",
            "spoon",
            "bowl",
            "banana",
            "apple",
            "sandwich",
            "orange",
            "broccoli",
            "carrot",
            "hot dog",
            "pizza",
            "donut",
            "cake",
            "chair",
            "sofa",
            "pottedplant",
            "bed",
            "diningtable",
            "toilet",
            "tvmonitor",
            "laptop",
            "mouse",
            "remote",
            "keyboard",
            "cell phone",
            "microwave",
            "oven",
            "toaster",
            "sink",
            "refrigerator",
            "book",
            "clock",
            "vase",
            "scissors",
            "teddy bear",
            "hair drier",
            "toothbrush",
        };

        public static string GetLabel(int index)
          => _labelsV2[index];
    }
}
