using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DropOffZone : MonoBehaviour
{
    //public GameObject myPlayer;
    public float myScore = 0;
    public TextMeshPro ScoreDisplay;
    // Start is called before the first frame update
    public void UpdateScore()
    {
        ScoreDisplay.text = myScore.ToString();
    }
}
