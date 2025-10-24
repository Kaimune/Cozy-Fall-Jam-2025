using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerKeyboard : MonoBehaviour
{
    public float moveSpeed = 5f;
    // Start is called before the first frame update
    void Update()
    {
        // Step 1: Create raw input vector
        Vector3 inputDir = Vector3.zero;
        if (Input.GetKey(KeyCode.W)) inputDir += Vector3.forward;
        if (Input.GetKey(KeyCode.S)) inputDir += Vector3.back;
        if (Input.GetKey(KeyCode.A)) inputDir += Vector3.left;
        if (Input.GetKey(KeyCode.D)) inputDir += Vector3.right;

        // Step 2: Normalize to prevent diagonal speed boost
        inputDir = inputDir.normalized;

        // Step 3: Desired movement target (only X/Z)
        Vector3 targetPos = transform.position + inputDir * moveSpeed * Time.deltaTime;
        targetPos.y = transform.position.y;

        // Step 4: Smoothly move toward target
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPos,
            moveSpeed * Time.deltaTime
        );
        transform.position = PlayAreaUtils.ClampPosition(transform.position, FruitManager.Instance.obstacles);
    }
}
