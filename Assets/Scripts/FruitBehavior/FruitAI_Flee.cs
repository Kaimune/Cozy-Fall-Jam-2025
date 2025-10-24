using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FruitAI_Flee : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float targetDistance = 5f;       // preferred distance from player
    public float randomAngle = 30f;         // deviation for natural movement
    public float edgeBuffer = 1f;           // distance from edges
    public float centerPullBase = 1f;       // base centering force
    public float centerPullExponent = 2f;   // exponential scaling

    void Update()
    {
        if (FruitManager.Instance.Players.Length == 0) return;

        GameObject closestPlayer = null;
        float closestDistance = Mathf.Infinity;

        // Find closest player
        foreach (GameObject player in FruitManager.Instance.Players)
        {
            if (player == null) continue;

            float dist = Vector3.Distance(transform.position, player.transform.position);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                closestPlayer = player;
            }
        }

        if (closestPlayer == null) return;

        // Compute flee direction
        Vector3 fleeDirection = transform.position - closestPlayer.transform.position;
        fleeDirection.y = 0;

        // Adjust flee force based on distance
        float distanceFactor = targetDistance - closestDistance;
        // If too close, flee stronger; if too far, centering becomes stronger
        float fleeStrength = Mathf.Max(distanceFactor, 0.1f);

        fleeDirection.Normalize();

        // Add random deviation
        float angle = Random.Range(-randomAngle, randomAngle);
        fleeDirection = Quaternion.Euler(0, angle, 0) * fleeDirection;

        // Centering force (scaled exponentially)
        Vector3 playAreaCenter = FruitManager.Instance.transform.position +
                                 new Vector3(FruitManager.Instance.playAreaLength, 0, FruitManager.Instance.playAreaWidth) * 0.5f;
        Vector3 toCenter = playAreaCenter - transform.position;
        toCenter.y = 0;
        float normalizedDistance = Mathf.Clamp01(closestDistance / targetDistance);
        float centerForceStrength = Mathf.Pow(normalizedDistance, centerPullExponent) * centerPullBase;

        // Combine forces
        Vector3 moveDir = (fleeDirection * fleeStrength + toCenter.normalized * centerForceStrength).normalized;

        // Move fruit
        transform.Translate(moveDir * moveSpeed * Time.deltaTime, Space.World);

        // Clamp to play area and avoid obstacles
        transform.position = PlayAreaUtils.ClampPosition(transform.position, FruitManager.Instance.obstacles, edgeBuffer);
    }
}

