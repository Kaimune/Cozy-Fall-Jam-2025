using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FruitAI_RandomWalk : MonoBehaviour
{
    public float moveSpeed = 3f;             // Units per second
    public float directionChangeInterval = 2f; // Seconds between random direction changes
    public float minTargetDistance = 2f;

    private Vector3 targetPos;           // Current movement direction
    public float timer;
    // Start is called before the first frame update
    void Start()
    {
        PickRandomTarget();
        timer = directionChangeInterval;
    }

    void Update()
    {
        // Move toward the target (X/Z only)
        Vector3 moveTarget = new Vector3(targetPos.x, transform.position.y, targetPos.z);
        transform.position = Vector3.MoveTowards(transform.position, moveTarget, moveSpeed * Time.deltaTime);

        // Clamp inside play area
        ClampPosition();

        // Timer countdown
        timer -= Time.deltaTime;

        // Pick a new target if we reached it OR timer expired
        if (Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z),
                             new Vector3(targetPos.x, 0, targetPos.z)) < 0.1f || timer <= 0f)
        {
            PickRandomTarget();
            timer = directionChangeInterval;
        }
    }

    // Pick a random normalized direction in X/Z plane
    void PickRandomTarget()
    {
        Vector3 newTarget;
        int attempts = 0;
        do
        {
            float randomX = Random.Range(FruitManager.Instance.transform.position.x, FruitManager.Instance.transform.position.x + FruitManager.Instance.playAreaLength);
        float randomZ = Random.Range(FruitManager.Instance.transform.position.z, FruitManager.Instance.transform.position.z + FruitManager.Instance.playAreaWidth);
            newTarget = new Vector3(randomX, transform.position.y, randomZ);
            attempts++;
            // Safety: avoid infinite loop
            if (attempts > 10) break;
        }
        while (Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z),
                                new Vector3(newTarget.x, 0, newTarget.z)) < minTargetDistance);

        targetPos = newTarget;
    }

    // Clamp the position inside the area
    void ClampPosition()
    {
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, FruitManager.Instance.transform.position.x, FruitManager.Instance.transform.position.x + FruitManager.Instance.playAreaLength);
        pos.z = Mathf.Clamp(pos.z, FruitManager.Instance.transform.position.z, FruitManager.Instance.transform.position.z + FruitManager.Instance.playAreaWidth);
        transform.position = pos;
    }
}
