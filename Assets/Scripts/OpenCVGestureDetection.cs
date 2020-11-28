using UnityEngine;
using System.Runtime.InteropServices;

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
    private bool _confirmedMask = false;
    public GameObject screen;
    public GameObject hand;

    HSVRange hsvRange;
    Thresholds thresholds;
    Position handPos;

    public void setLowThreshold(float value) {
        thresholds.lowThreshold = (int) value;
    }

    public void setHighThreshold(float value) {
        thresholds.highThreshold = (int) value;
    }

    void Awake() {
        int camWidth = 0, camHeight = 0;
        int result = OpenCVInterop.openCam(ref camWidth, ref camHeight, 1);
        if (result < 0) {
            if (result == -1) {
                Debug.LogWarningFormat("[{0}] Failed to open camera stream.", GetType());
            }
            return;
        }

        Debug.Log("Camera Resolution is:" + camWidth + " " + camHeight);
        screen.transform.localScale = new Vector3(camWidth / 10, 1, camHeight / 10);

        hand.transform.position = new Vector3(0, 0, 0);

        hsvRange.minH = 90F;
        hsvRange.maxH = 150F;
        hsvRange.minS = 0.15F;
        hsvRange.maxS = 0.25F;
        thresholds.lowThreshold = 0;
        thresholds.highThreshold = 30;
        handPos.X = 0;
        handPos.Y = 0;

        CameraResolution = new Vector2(camWidth, camHeight);
        _ready = true;
    }

    void OnApplicationQuit() {
        if (_ready) {
            OpenCVInterop.closeCam();
        }
    }

    Vector3 imageToScreenCoord(float x, float y) {
        return new Vector3(x, -y, 0);
    }

    void Update() {
        if (!_ready)
            return;

        // all DLL calls have to be in an unsafe block
        unsafe {
            //OpenCVInterop.capture();
            // use SPACE key to make mask of current HSV values in the ROIs
            if (!_confirmedMask && Input.GetKeyDown(KeyCode.Space)) {
                hsvRange = OpenCVInterop.getMaskRange();
                Debug.Log(hsvRange.minH);
                Debug.Log(hsvRange.maxH);
                Debug.Log(hsvRange.minS);
                Debug.Log(hsvRange.maxS);
            }
            // draw contour if the mask has been confirmed and get the hand center
            if (_confirmedMask) {
                OpenCVInterop.drawHandContour();
                OpenCVInterop.getHandCenter(ref handPos);
                hand.transform.position = imageToScreenCoord(handPos.X, handPos.Y);
            }
            // if mask has not been confirmed show the normal feed with the ROIs
            else {
                OpenCVInterop.showOverlayFeed();
            }
            Debug.Log(handPos.X);
            Debug.Log(handPos.Y);
            // always show the binary mask preview
            OpenCVInterop.showBinaryFeed(hsvRange, thresholds);

            // press RETURN key to confirm mask
            if (Input.GetKeyDown(KeyCode.Return)) {
                _confirmedMask = true;
            }
        }
    }
}