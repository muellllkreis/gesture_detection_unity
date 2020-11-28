using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraDistance : MonoBehaviour
{
    public Collider collider;
    public float elevation;
    public float cameraDistance;

    void Update() {
        Vector3 objectSizes = collider.bounds.max - collider.bounds.min;
        float objectSize = Mathf.Max(objectSizes.x, objectSizes.y, objectSizes.z);
        float cameraView = 2.0f * Mathf.Tan(0.5f * Mathf.Deg2Rad * Camera.main.fieldOfView); // Visible height 1 meter in front
        float distance = cameraDistance * objectSize / cameraView; // Combined wanted distance from the object
        distance += 0.5f * objectSize; // Estimated offset from the center to the outside of the object
        Camera.main.transform.position = collider.bounds.center - distance * Camera.main.transform.forward;
        Camera.main.transform.rotation = Quaternion.Euler(new Vector3(elevation, 0, 0));
    }

}
