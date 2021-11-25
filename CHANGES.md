# CHANGES

## 2021-11-25

Currently supports some random model for YOLOv3-tiny.

 * Implemented: inference with Barracuda 2.0.
 * Implemented: post-processing (soft-NMS).
 * Changed: v2 -> v3 input/output handling.
 * Removed: need of own preprocessing (Barracuda handles this).

Difference between v2 and v3:

 * Object classes (v2: PASCAL/20, v3: COCO/80)
 * Output dimensions (v2: 13x13, v3: 13x13 + 26x26)
 * Class probabilities (v2: Softmax, v3: Sigmoid)
 * Rect anchors.

Difference in particular ONNX models:

 * Input/output names.
 * Tensor shapes. (N,C,H,W or N,H,W,C)
 * Pixel format. ([0,1] or [0,255])
 * Pixel unit. (agaist grid or entire image)
