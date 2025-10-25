using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [Header("Player References")]
    public Transform player1;
    public Transform player2;
    public GameObject Camera;

    [Header("Camera Offsets")]
    public Vector3 offset = new Vector3(0f, 10f, -10f); // Default top-down offset
    public Vector3 CameraRotaion = new Vector3(30f, 30f, 0f); // Default top-down offset
    public float smoothSpeed = 5f; // How smoothly the camera moves

    void LateUpdate()
    {
        if (player1 == null || player2 == null)
            return;

        // Find the midpoint between two players
        Vector3 middlePoint = (player1.position + player2.position) / 2f;

        // Apply offset to the camera¡¯s position
        Vector3 desiredPosition = new Vector3(
            middlePoint.x + offset.x,
            offset.y,
            middlePoint.z + offset.z
        );
        Transform CameraTransform = Camera.GetComponent<Transform>();
        // Smooth follow (or just snap if you prefer)
        CameraTransform.position = Vector3.Lerp(CameraTransform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        // Optional: keep camera rotation fixed
        CameraTransform.rotation = Quaternion.Euler(CameraRotaion);
    }
}
