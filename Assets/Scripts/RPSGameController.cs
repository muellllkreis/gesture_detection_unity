using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Drawing;
using UnityEngine.UI;

public class RPSGameController : MonoBehaviour
{

    OpenCVGestureDetection openCVGestureDetection;
    public GameObject screen;
    public GameObject handPrefab;
    public Material opponentMat;
    private GameObject playerHand;
    private GameObject opponentHand;
    private Animator playerAnimator;
    private Animator opponentAnimator;
    private int playerScore=0, opponentScore=0;
    [SerializeField]
    private Text UIText;
    [SerializeField]
    private Text PoseText;
    [SerializeField]
    private Text FingerText;
    [SerializeField]
    private Text playerScoreText;
    [SerializeField]
    private Text IAScoreText;
    [SerializeField]
    private Camera cam;
    [SerializeField]
    private ParticleSystem psEffect;

    void Start()
    {
        openCVGestureDetection = GetComponent<OpenCVGestureDetection>();
        screen.transform.localScale = new Vector3(openCVGestureDetection.GetCamWidth() / 10, 1, openCVGestureDetection.GetCamHeight() / 10);
        Vector3 scale = screen.transform.localScale;        
        screen.transform.position = new Vector3((scale[0] / 2) * 10, -(scale[2] / 2) * 10, 0);
        psEffect.Stop();

        //player
        playerHand = Instantiate(handPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        playerHand.name = "PlayerHand";
        playerHand.transform.eulerAngles = new Vector3(-180, 270, 90);
        playerHand.transform.position = new Vector3(screen.transform.position.x-100, screen.transform.position.y, 0);;
        playerAnimator = playerHand.transform.Find("Armature").GetComponent<Animator>();
        playAnimation(playerAnimator, "Scissors");                        

        //IA
        opponentHand = Instantiate(handPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        opponentHand.name = "IAHand";
        opponentHand.transform.eulerAngles = new Vector3(-180, 270, 90);
        opponentHand.transform.position = new Vector3(screen.transform.position.x+100, screen.transform.position.y, 0);
        opponentHand.transform.Find("Mesh").GetComponent<Renderer>().material = opponentMat;
        opponentAnimator = opponentHand.transform.Find("Armature").GetComponent<Animator>();
        playAnimation(opponentAnimator, "Paper");                        

        Debug.Log("Scale: " + screen.transform.localScale.x + "," + screen.transform.localScale.z);

        UIText.text = "";

        psEffect.transform.parent = cam.transform;
        psEffect.transform.localPosition = new Vector3(0, 0, 30);
        
    }

    Vector3 ImageToScreenCoord(float x, float y) {
        return new Vector3(x, -y, 0);
    }

    void toggleAnimation(Animator handAnimator, int fingerCount)
    {
        if (fingerCount == 0)
        {
            playAnimation(handAnimator, "Rock");
            PoseText.text = "ROCK";
        }
        else if (fingerCount < 4)
        {
            playAnimation(handAnimator, "Scissors");            
            PoseText.text = "SCISSORS";
        }
        else if (fingerCount >= 4)
        {
            playAnimation(handAnimator, "Paper");                        
            PoseText.text = "PAPER";
        }
        else
        {
            playAnimation(handAnimator, "Palm");                                    
        }
        FingerText.text = fingerCount.ToString();
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

    void Update()
    {
        if(openCVGestureDetection.IsTrackingPosition()) {
            Point playerHandPos = openCVGestureDetection.GetHandPos();
            int fingerCount = openCVGestureDetection.GetFingerTipsCount();
            Vector3 targetPosition = ImageToScreenCoord(playerHandPos.X, playerHandPos.Y);
            moveHand(playerHand, targetPosition);
            toggleAnimation(playerAnimator,fingerCount);            
            SetOpponentPosition(targetPosition);
        }
    }

    //launch the game
    public void RockPaperScissors()
    {
        StartCoroutine(Countdown());
    }

    //increase score of IA or player based on current poses
    void choseWinner()
    {
        string playerPose = "", IAPose="";
        foreach (AnimatorControllerParameter parameter in playerAnimator.parameters)
        {
            if (playerAnimator.GetBool(parameter.name))
            {
                playerPose = parameter.name;
            }
        }
        foreach (AnimatorControllerParameter parameter in opponentAnimator.parameters)
        {
            if (opponentAnimator.GetBool(parameter.name))
            {
                IAPose = parameter.name;
            }
        }
        int initialPlayerScore = playerScore;        
        int initialOpponentScore = opponentScore;        
        if (playerPose != IAPose)
        {
            if (playerPose == "Rock")
            {
                if (IAPose == "Paper") opponentScore++;
                else playerScore++;
            }
            if (playerPose == "Scissors")
            {
                if (IAPose == "Rock") opponentScore++;
                else playerScore++;
            }
            if (playerPose == "Paper")
            {
                if (IAPose == "Scissors") opponentScore++;
                else playerScore++;
            }
        }
        if (playerScore > initialPlayerScore)
        {
            psEffect.startColor = new UnityEngine.Color(0, 255, 225);
            StartCoroutine("PlayEffect", 1.5f);
        }
        else if (opponentScore > initialOpponentScore)
        {
            psEffect.startColor = UnityEngine.Color.red;
            StartCoroutine("PlayEffect", 1.5f);
        }
        IAScoreText.text = opponentScore.ToString();
        playerScoreText.text = playerScore.ToString();
    }

    //countdown + game routine
    IEnumerator Countdown()
    {
        //opponent play rock pose
        toggleAnimation(opponentAnimator, 0);
        //countdown
        UIText.text = "3";
        yield return new WaitForSeconds(1);
        UIText.text = "2";
        yield return new WaitForSeconds(1);
        UIText.text = "1";
        yield return new WaitForSeconds(1);
        UIText.text = "";
        //choose random pose for IA
        string[] poses = new string[3] { "Rock", "Paper", "Scissors" };
        string Pose = poses[Random.Range(0, 3)];
        playAnimation(opponentAnimator, Pose); 
        //determine who won this round
        choseWinner();        
    }

    IEnumerator PlayEffect(float seconds)
    {
        psEffect.Play();
        yield return new WaitForSeconds(seconds);
        psEffect.Stop();
    }
}
