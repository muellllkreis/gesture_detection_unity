using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SinglePlayerMenu : MonoBehaviour
{
    public void PlayRPS() {
        SceneManager.LoadScene("RockPaperScissors");
    }

    public void PlayDrawing() {
        SceneManager.LoadScene("Drawing");
    }
}
