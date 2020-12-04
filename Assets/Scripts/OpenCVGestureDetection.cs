using UnityEngine;
using System.Runtime.InteropServices;
using System.Drawing;

// Define the functions which can be called from the .dll.
internal static class OpenCVInterop {
    [DllImport("gestures")]
    internal static extern int openCam(ref int outCameraWidth, ref int outCameraHeight, int camNumber);

    [DllImport("gestures")]
    internal static extern void closeCam();

    [DllImport("gestures")]
    internal static extern void capture();

    [DllImport("gestures")]
    internal static extern void showOverlayFeed();

    [DllImport("gestures")]
    internal static extern HSVRange getMaskRange();

    [DllImport("gestures")]
    internal static extern void showBinaryFeed(HSVRange hsv_range, Thresholds thresholds);

    [DllImport("gestures")]
    internal static extern void drawHandContour();

    [DllImport("gestures")]
    internal static extern void getHandCenter(ref Position handPos);

    [DllImport("gestures")]
    internal unsafe static extern void getFingerTips(Position* allFingerTips, ref int detectedFingerTipsCount);
}

// Define the structure to be sequential and with the correct byte size (2 ints = 4 bytes * 2 = 8 bytes)
[StructLayout(LayoutKind.Sequential, Size = 8)]
public struct Thresholds {
    public int lowThreshold, highThreshold;
}

[StructLayout(LayoutKind.Sequential, Size = 16)]
public struct HSVRange {
    public float minH, minS, maxH, maxS;
}

[StructLayout(LayoutKind.Sequential, Size = 8)]
public struct Position {
    public int X, Y;
}

public class OpenCVGestureDetection : MonoBehaviour {
    public static Vector2 CameraResolution;

    private bool _ready;
    private bool trackingPosition = false;
    private Position[] fingerTips;
    private int maxFingerTipsCount = 5;
    private GestureCache gestureCache;
    private int camWidth = 0; 
    private int camHeight = 0;
    private int currentCount = 0;

    HSVRange hsvRange;
    Thresholds thresholds;
    Position handPos;

    public bool IsTrackingPosition() {
        return trackingPosition;
    }

    public int GetCamWidth() {
        return this.camWidth;
    }

    public int GetCamHeight() {
        return this.camHeight;
    }

    public Point GetHandPos() {
        return new Point(this.handPos.X, this.handPos.Y);
    }

    public int GetFingerTipsCount() {
        return this.currentCount;
    }

    public void SetLowThreshold(float value) {
        thresholds.lowThreshold = (int) value;
    }

    public void SetHighThreshold(float value) {
        thresholds.highThreshold = (int) value;
    }

    public Point getIndexPosition()
    {
        return new Point(this.fingerTips[0].X, this.fingerTips[0].Y);
    }

    void Awake() {
        int result = OpenCVInterop.openCam(ref camWidth, ref camHeight, 0);
        if (result < 0) {
            if (result == -1) {
                Debug.LogWarningFormat("[{0}] Failed to open camera stream.", GetType());
            }
            return;
        }

        Debug.Log("Camera Resolution is:" + camWidth + " " + camHeight);

        hsvRange.minH = 90F;
        hsvRange.maxH = 150F;
        hsvRange.minS = 0.15F;
        hsvRange.maxS = 0.25F;
        thresholds.lowThreshold = 0;
        thresholds.highThreshold = 30;
        handPos.X = 0;
        handPos.Y = 0;

        fingerTips = new Position[maxFingerTipsCount];
        gestureCache = new GestureCache();

        CameraResolution = new Vector2(camWidth, camHeight);
        _ready = true;
    }

    void OnApplicationQuit() {
        if (_ready) {
            OpenCVInterop.closeCam();
        }
    }

    void Update() {
        if (!_ready)
            return;

        // all DLL calls have to be in an unsafe block
        unsafe {
            //OpenCVInterop.capture();
            // use SPACE key to make mask of current HSV values in the ROIs
            if (!trackingPosition && Input.GetKeyDown(KeyCode.Space)) {
                hsvRange = OpenCVInterop.getMaskRange();
                Debug.Log(hsvRange.minH);
                Debug.Log(hsvRange.maxH);
                Debug.Log(hsvRange.minS);
                Debug.Log(hsvRange.maxS);
            }
            // draw contour if the mask has been confirmed and get the hand center
            if (trackingPosition) {
                OpenCVInterop.drawHandContour();
                OpenCVInterop.getHandCenter(ref handPos);
                //Debug.Log(handPos.X);
                //Debug.Log(handPos.Y);
                int detectedFingerTipsCount = 0;
                fixed (Position* allFingerTips = fingerTips) {
                    OpenCVInterop.getFingerTips(allFingerTips, ref detectedFingerTipsCount);
                    //Debug.Log(detectedFingerTipsCount);
                    gestureCache.Add(detectedFingerTipsCount);
                    currentCount = gestureCache.GetCachedGesture();
                    Debug.Log(currentCount);
                }

            }
            // if mask has not been confirmed show the normal feed with the ROIs
            else {
                OpenCVInterop.showOverlayFeed();
            }
            //Debug.Log(handPos.X);
            //Debug.Log(handPos.Y);
            // always show the binary mask preview
            //Debug.Log("Threshold" + thresholds.highThreshold);
            OpenCVInterop.showBinaryFeed(hsvRange, thresholds);

            // press RETURN key to confirm mask
            if (Input.GetKeyDown(KeyCode.Return)) {
                trackingPosition = true;
            }
        }
    }
}