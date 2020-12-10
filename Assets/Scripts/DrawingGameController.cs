using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Drawing;
using UnityEngine.UI;

public class DrawingGameController : MonoBehaviour
{

    OpenCVGestureDetection openCVGestureDetection;
    public GameObject screen;    
    //public GameObject handPrefab;
    GameObject drawingPoint;
    [SerializeField]
    GameObject screenBLcorner;
    private Renderer screenRenderer;
    [SerializeField]
    private Canvas drawingCanvas;    
    private int resolution = 2048;
    [SerializeField]
    private int brushSize = 10;
    [SerializeField]
    private UnityEngine.Color brushColor = UnityEngine.Color.red;
    private UnityEngine.Color[] brushColors;
    Texture2D drawingTexture;
    private float camHeight, camWidth;
    int fingerCount = 0;
    int previousFingerCount = 0;

    void Start()
    {
        openCVGestureDetection = GetComponent<OpenCVGestureDetection>();
        screen.transform.localScale = new Vector3(openCVGestureDetection.GetCamWidth() / 10, 1, openCVGestureDetection.GetCamHeight() / 10);
        Vector3 scale = screen.transform.localScale;        
        screen.transform.position = new Vector3((scale[0] / 2) * 10, -(scale[2] / 2) * 10, 0);        
        screenRenderer = screen.GetComponent<Renderer>();
        drawingTexture = new Texture2D(resolution, resolution);                        
        drawingTexture.Apply();
        screenRenderer.material.mainTexture = drawingTexture;

        //player
        drawingPoint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        drawingPoint.transform.localScale = new Vector3(.5f, .5f, .5f);

        //camera
        camHeight = openCVGestureDetection.GetCamHeight();
        camWidth = openCVGestureDetection.GetCamWidth();

        //brush
        setBrushColor(brushColor);

        Debug.Log("Scale: " + screen.transform.localScale.x + "," + screen.transform.localScale.z);
    }

    Vector3 ImageToScreenCoord(float x, float y)
    {
        return new Vector3(x, -y, 0);
    }

    void moveHand(GameObject hand, Vector3 targetPosition)
    {
        Vector3 velocity = Vector3.zero;
        hand.transform.position = Vector3.SmoothDamp(hand.transform.position, targetPosition, ref velocity, 0.3F);
    }

    void setBrushColor(UnityEngine.Color color)
    {
        brushColors = new UnityEngine.Color[brushSize * brushSize];
        for (int i = 0; i < brushColors.Length; i++)
        {
            brushColors[i] = color;
        }
    }

    void playAnimation(Animator handAnimator, string RPSPose)
    {
        foreach (AnimatorControllerParameter parameter in handAnimator.parameters)
        {
            if (parameter.name != RPSPose)
            {
                handAnimator.SetBool(parameter.name, false);
            }
            else
            {
                handAnimator.SetBool(parameter.name, true);
            }
        }
    }

    Vector2Int getTextureCoord(Vector3 position)
    {
        return new Vector2Int(Mathf.RoundToInt((position.x-screenBLcorner.transform.position.x)), Mathf.RoundToInt((position.y - screenBLcorner.transform.position.y)));
    }

    void drawOnCanvas(Point position)
    {        
        if (position.X *resolution/camWidth - brushSize > 0 && position.Y*resolution/camHeight - brushSize > 0 && position.X*resolution/camWidth + brushSize < resolution && position.Y*resolution/camHeight + brushSize < resolution)
        {            
            drawingTexture.SetPixels(Mathf.RoundToInt(position.X * resolution / camWidth), resolution - 1 - Mathf.RoundToInt(position.Y * resolution / camHeight), brushSize, brushSize, brushColors);            
            drawingTexture.Apply();            
        }        
    }

    void resetCanvas()
    {
        for (int i = 0; i < resolution; i++)
        {
            for (int j = 0; j < resolution; j++)
            {
                drawingTexture.SetPixel(i, j, UnityEngine.Color.white);
            }
        }
    }

    void moveIndex(GameObject index, Vector3 targetPosition)
    {
        Vector3 velocity = Vector3.zero;
        index.transform.position = Vector3.SmoothDamp(index.transform.position, targetPosition, ref velocity, 0.3F);
    }


    void Update()
    {
        if (openCVGestureDetection.IsTrackingPosition())
        {
            Point indexPos = openCVGestureDetection.getIndexPosition(); 
            fingerCount = openCVGestureDetection.GetFingerTipsCount();
            Vector3 targetPosition = ImageToScreenCoord(indexPos.X, indexPos.Y);
            moveIndex(drawingPoint, targetPosition);            
            drawOnCanvas(indexPos);

            if (fingerCount >= 4 && previousFingerCount != fingerCount)
            {
                brushColor = new UnityEngine.Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
                setBrushColor(brushColor);
            }
            if (fingerCount == 0 && previousFingerCount != fingerCount)
            {
                resetCanvas();
            }

            previousFingerCount = fingerCount;
        }
    }
}
