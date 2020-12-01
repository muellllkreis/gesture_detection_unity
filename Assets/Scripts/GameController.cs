using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Drawing;

public class GameController : MonoBehaviour
{

    OpenCVGestureDetection openCVGestureDetection;
    public GameObject screen;
    public GameObject handPrefab;
    public Material opponentMat;
    private GameObject playerHand;
    private GameObject opponentHand;

    void Start()
    {
        openCVGestureDetection = GetComponent<OpenCVGestureDetection>();
        screen.transform.localScale = new Vector3(openCVGestureDetection.GetCamWidth() / 10, 1, openCVGestureDetection.GetCamHeight() / 10);

        playerHand = Instantiate(handPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        playerHand.transform.eulerAngles = new Vector3(-180, 270, 90);

        opponentHand = Instantiate(handPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        opponentHand.transform.eulerAngles = new Vector3(-180, 270, 90);
        opponentHand.transform.Find("Mesh").GetComponent<Renderer>().material = opponentMat;

        Debug.Log("Scale: " + screen.transform.localScale.x + "," + screen.transform.localScale.z);

    }

    Vector3 ImageToScreenCoord(float x, float y) {
        return new Vector3(x, -y, 0);
    }

    void toggleAnimation(int fingerCount) {
        if (fingerCount == 0) {
            playerHand.transform.Find("Armature").GetComponent<Animator>().Play("Rock");
        }
        else if (fingerCount <= 4) {
            playerHand.transform.Find("Armature").GetComponent<Animator>().Play("Scissors");
        }
        else if (fingerCount >= 5) {
            playerHand.transform.Find("Armature").GetComponent<Animator>().Play("Paper");
        }
        else {
            playerHand.transform.Find("Armature").GetComponent<Animator>().Play("Palm");
        }
    }

    void SetOpponentPosition(Vector3 playerPos) {
        float opponentY;
        opponentY = (screen.transform.localScale.z * 10) + playerPos[1];
        float opponentX;
        float distance = (screen.transform.localScale.x / 2) * 10;
        if (playerPos[0] >= screen.transform.localScale.x / 2) {
            opponentX = playerPos[0] - distance;
        }
        else {
            opponentX = playerPos[0] + distance;
        }
        Vector3 targetPosition = ImageToScreenCoord(opponentX, opponentY);
        moveHand(opponentHand, targetPosition);
    }

    void moveHand(GameObject hand, Vector3 targetPosition) {
        Vector3 velocity = Vector3.zero;
        hand.transform.position = Vector3.SmoothDamp(hand.transform.position, targetPosition, ref velocity, 0.3F);
    }

    void Update()
    {
        if(openCVGestureDetection.IsTrackingPosition()) {
            Point playerHandPos = openCVGestureDetection.GetHandPos();
            int fingerCount = openCVGestureDetection.GetFingerTipsCount();
            Vector3 targetPosition = ImageToScreenCoord(playerHandPos.X, playerHandPos.Y);
            moveHand(playerHand, targetPosition);
            toggleAnimation(fingerCount);
            SetOpponentPosition(targetPosition);
        }
    }
}
