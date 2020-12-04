using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Drawing;
using UnityEngine.UI;

public class DrawingGameController : MonoBehaviour
{

    OpenCVGestureDetection openCVGestureDetection;
    public GameObject screen;
    [SerializeField]
    private GameObject background;
    //public GameObject handPrefab;
    [SerializeField]
    GameObject drawingPoint;
    [SerializeField]
    GameObject screenBLcorner;
    private Renderer screenRenderer;
    [SerializeField]
    private Canvas drawingCanvas;    
    private int resolution = 512;
    [SerializeField]
    private int brushSize = 10;
    [SerializeField]
    private UnityEngine.Color brushColor = UnityEngine.Color.red;
    private UnityEngine.Color[] brushColors;
    Texture2D drawingTexture;
    private float camHeight, camWidth;

    void Start()
    {
        openCVGestureDetection = GetComponent<OpenCVGestureDetection>();
        screen.transform.localScale = new Vector3(openCVGestureDetection.GetCamWidth() / 10, 1, openCVGestureDetection.GetCamHeight() / 10);
        Vector3 scale = screen.transform.localScale;
        background.transform.localScale = scale * 2f;
        screen.transform.position = new Vector3((scale[0] / 2) * 10, -(scale[2] / 2) * 10, 0);
        background.transform.position = screen.transform.position + new Vector3(0, 0, 2);
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
        brushColors = new UnityEngine.Color[brushSize * brushSize];
        for (int i = 0; i < brushColors.Length; i++)
        {
            brushColors[i] = brushColor;
        }

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

    
    void Update()
    {
        if (openCVGestureDetection.IsTrackingPosition())
        {
            Point indexPos = openCVGestureDetection.getIndexPosition();                   
            Vector3 targetPosition = ImageToScreenCoord(indexPos.X, indexPos.Y);
            moveHand(drawingPoint, targetPosition);            
            drawOnCanvas(indexPos);
        }
    }
}
