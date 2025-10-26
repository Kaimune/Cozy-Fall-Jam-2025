using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameTimer : MonoBehaviour
{
    // Start is called before the first frame update
    public float totalTime = 180f; // 3 minutes in seconds
    public TextMeshPro timerDisplay;
    public List<float> timeStamps = new List<float>(); // Time points in seconds
    public float delayAfterTimesUp = 2f; // seconds

    public float currentTime;
    private int nextTimeIndex = 0;
    private bool timerRunning = false;
    public DropOffZone Player1DropOff, Player2DropOff;

    private void Start()
    {
        // Sort timestamps in ascending order for safety
        timeStamps.Sort((a, b) => b.CompareTo(a));

        currentTime = totalTime;
        timerRunning = true;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartCurrentScene();
        }
        if (!timerRunning) return;

        currentTime -= Time.deltaTime;

        // Check if we've reached the next timestamp
        if (nextTimeIndex < timeStamps.Count && currentTime <= timeStamps[nextTimeIndex])
        {
            FruitManager.Instance.globalSpeed += 1;
            Debug.Log($" Timestamp reached at time: {currentTime:F2}s");
            SoundController.Instance.SpeedUpMusic();
            nextTimeIndex++;
        }

        // When timer finishes
        if (currentTime <= 0f)
        {
            currentTime = 0f;
            timerRunning = false;
            Debug.Log(" Timer finished!");
            TimesUp();
        }
        UpdateDisplay();
    }

    private void RestartCurrentScene()
    {
        // Get the currently active scene
        Scene currentScene = SceneManager.GetActiveScene();

        // Reload it
        SceneManager.LoadScene(currentScene.name);
    }
    private void UpdateDisplay()
    {
        if (timerDisplay == null) return;

        if (!timerRunning && currentTime <= 0f)
        {
            timerDisplay.text = "Time's up!";
            return;
        }

        int minutes = Mathf.FloorToInt(currentTime / 60f);
        int seconds = Mathf.FloorToInt(currentTime % 60f);

        timerDisplay.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
    public void TimesUp()
    {
        FruitManager.Instance.GameEnding();
        DOVirtual.DelayedCall(delayAfterTimesUp, () =>
        {
            DisplayWinner();
        });
    }

    public void DisplayWinner()
    {
        if(Player1DropOff.myScore>Player2DropOff.myScore)
        {
            timerDisplay.text = "Player 1 Win!";
        }

        if (Player2DropOff.myScore > Player1DropOff.myScore)
        {
            timerDisplay.text = "Player 2 Win!";
        }

        if (Player1DropOff.myScore == Player2DropOff.myScore)
        {
            timerDisplay.text = "Tie!";
        }

        DOVirtual.DelayedCall(delayAfterTimesUp*2, () =>
        {
            DisplayPlayAgain();
        });
    }
    public void DisplayPlayAgain()
    {
        timerDisplay.text = "Press R to play again";
    }
}
