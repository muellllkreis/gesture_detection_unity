using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenManager : MonoBehaviour
{
    public GameObject screen;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(screen.transform.localScale);
        Vector3 scale = screen.transform.localScale;
        // move left corner of screen to origin so that it complies with image pixel coordinates
        screen.transform.position = new Vector3((scale[0]/2) * 10, -(scale[2] / 2) * 10, 0);
    }

    Vector3 imageToScreenCoord(float x, float y) {
        return new Vector3(x, -y, 0);
    }

    // Update is called once per frame
    void Update()
    {
    }
}
