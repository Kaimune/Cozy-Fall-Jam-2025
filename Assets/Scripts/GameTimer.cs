using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameTimer : MonoBehaviour
{
    // Start is called before the first frame update
    public float totalTime = 180f; // 3 minutes in seconds
    public List<float> timeStamps = new List<float>(); // Time points in seconds

    public float currentTime;
    private int nextTimeIndex = 0;
    private bool timerRunning = false;

    private void Start()
    {
        // Sort timestamps in ascending order for safety
        timeStamps.Sort((a, b) => b.CompareTo(a));

        currentTime = totalTime;
        timerRunning = true;
    }

    private void Update()
    {
        if (!timerRunning) return;

        currentTime -= Time.deltaTime;

        // Check if we've reached the next timestamp
        if (nextTimeIndex < timeStamps.Count && currentTime <= timeStamps[nextTimeIndex])
        {
            FruitManager.Instance.globalSpeed += 1;
            Debug.Log($" Timestamp reached at time: {currentTime:F2}s");
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
    }
    public void TimesUp()
    {
        //Stop the Fruit Gen
        //Count the score
        //Show Winner
    }
}
