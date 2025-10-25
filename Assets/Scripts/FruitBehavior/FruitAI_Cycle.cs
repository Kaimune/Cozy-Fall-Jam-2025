using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FruitAI_Cycle : MonoBehaviour
{
    [Header("Orbit Settings")]
    public float orbitRadius = 2f;           // Distance from center 
    public float moveSpeed = 2f;
    public float orbitSpeed = 1f;
    public bool clockwise = true;            // Rotation direction

    [Header("Center Settings")]
    public float centerChangeInterval = 5f;  // Seconds between new centers
    public float centerLerpSpeed = 1f;       // How fast the center moves to new point
    public float minCenterDistance = 3f;     // Minimum distance from current center

    private Vector3 currentCenter;           // Current orbit center
    private Vector3 targetCenter;            // Next center to move toward
    private float timer;
    private float currentAngle;
    public fruitState myFruitState;


    void Start()
    {
        if (FruitManager.Instance == null)
        {
            Debug.LogError("No FruitManager in scene!");
            return;
        }

        PickRandomCenter();
        currentCenter = targetCenter;

        currentAngle = Random.Range(0f, 360f);
        timer = centerChangeInterval;
    }

    void Update()
    {
        if (!myFruitState.active)
        {
            return;
        }

        if (FruitManager.Instance == null) return;

        // Orbit angle update
        float deltaAngle = ((orbitSpeed*360)*FruitManager.Instance.globalSpeed)* Time.deltaTime;
        currentAngle += clockwise ? deltaAngle : -deltaAngle;
        float rad = currentAngle * Mathf.Deg2Rad;

        // Smoothly move center toward target
        //currentCenter = Vector3.MoveTowards(currentCenter, targetCenter, moveSpeed * Time.deltaTime);

        // Calculate orbit position
        Vector3 newPos = new Vector3(
            currentCenter.x + orbitRadius * Mathf.Cos(rad),
            transform.position.y,
            currentCenter.z + orbitRadius * Mathf.Sin(rad)
        );

        // Clamp inside play area
        newPos = PlayAreaUtils.ClampPosition(newPos, FruitManager.Instance.obstacles);

        transform.position = newPos;

        // Timer countdown
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            PickRandomCenter();
            timer = centerChangeInterval;
        }
    }

    void PickRandomCenter()
    {
        Vector3 playAreaMin = FruitManager.Instance.transform.position + Vector3.one * orbitRadius;
        Vector3 playAreaMax = new Vector3(
            FruitManager.Instance.transform.position.x + FruitManager.Instance.playAreaLength - orbitRadius * 2,
            0,
            FruitManager.Instance.transform.position.z + FruitManager.Instance.playAreaWidth - orbitRadius * 2
        );

        Vector3 newCenter = Vector3.zero;
        int attempts = 0;
        float safeDistance = orbitRadius + 0.1f; // safe margin from obstacles

        bool validCenter = false;

        while (!validCenter && attempts < 100)
        {
            attempts++;

            float randomX = Random.Range(playAreaMin.x, playAreaMax.x);
            float randomZ = Random.Range(playAreaMin.z, playAreaMax.z);
            newCenter = new Vector3(randomX, transform.position.y, randomZ);

            // Clamp inside play area
            newCenter = PlayAreaUtils.ClampPosition(newCenter, null); // obstacles handled separately

            // Check distance to all obstacles
            validCenter = true; // assume it's valid
            foreach (GameObject obs in FruitManager.Instance.obstacles)
            {
                if (obs == null) continue;
                Collider col = obs.GetComponent<Collider>();
                if (col != null)
                {
                    Vector3 closest = col.bounds.ClosestPoint(newCenter);
                    if (Vector3.Distance(newCenter, closest) < safeDistance)
                    {
                        validCenter = false; // too close to obstacle
                        break;
                    }
                }
            }

            // Also ensure minimum distance from current center
            if (Vector3.Distance(newCenter, currentCenter) < minCenterDistance)
                validCenter = false;
        }

        targetCenter = newCenter;
        StartCoroutine(DipEffect());
    }

    private System.Collections.IEnumerator DipEffect()
    {
        float dipHeight = 3f; // How far down it dips
        float dipTime = 0.5f;  // Time to go down/up

        float startY = transform.position.y;
        float downY = startY - dipHeight;

        // Dip down
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / dipTime;
            Vector3 pos = transform.position;
            pos.y = Mathf.Lerp(startY, downY, t);
            transform.position = pos;   // only change Y
            yield return null;
        }
        currentCenter = targetCenter; // jump to the new center instantly
        // Rise back up
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / dipTime;
            Vector3 pos = transform.position;
            pos.y = Mathf.Lerp(downY, startY, t);
            transform.position = pos;   // only change Y
            yield return null;
        }
    }
}
