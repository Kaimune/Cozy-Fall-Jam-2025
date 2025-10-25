using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FuritAI_Bounce : MonoBehaviour
{
    public float moveSpeed = 3f;          // Units per second
    private Vector3 moveDirection;        // Current direction in X/Z
    public fruitState myFruitState;


    void Start()
    {
        PickRandomDirection();
    }

    void Update()
    {
        if (!myFruitState.active)
        {
            return;
        }
        // Move in current direction (X/Z only)
        Vector3 newPos = transform.position + moveDirection * moveSpeed * Time.deltaTime;
        newPos.y = transform.position.y; // Lock Y
        transform.position = newPos;

        // Bounce off edges
        BounceOffEdges();
    }

    void PickRandomDirection()
    {
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        moveDirection = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
    }

    void BounceOffEdges()
    {
        if (FruitManager.Instance == null) return;

        Vector3 min = FruitManager.Instance.transform.position;
        Vector3 max = new Vector3(
            FruitManager.Instance.transform.position.x + FruitManager.Instance.playAreaLength,
            transform.position.y,
            FruitManager.Instance.transform.position.z + FruitManager.Instance.playAreaWidth
        );

        Vector3 pos = transform.position;

        // Bounce X
        if (pos.x <= min.x || pos.x >= max.x)
        {
            moveDirection.x = -moveDirection.x;
            // Clamp inside
            pos.x = Mathf.Clamp(pos.x, min.x, max.x);
            transform.position = pos;
        }

        // Bounce Z
        if (pos.z <= min.z || pos.z >= max.z)
        {
            moveDirection.z = -moveDirection.z;
            pos.z = Mathf.Clamp(pos.z, min.z, max.z);
            transform.position = pos;
        }
    }
}
