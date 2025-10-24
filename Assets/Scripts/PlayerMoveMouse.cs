using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMoveMouse : MonoBehaviour
{
    public LayerMask groundLayer = Physics.DefaultRaycastLayers;
    private Transform currentMarker;
    public Transform Player1;
    public GameObject markerPrefab;
    public float moveSpeed = 5f;
    public float arriveThreshold = 0.05f;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Input.GetMouseButtonDown(0))
        {
            if (currentMarker != null)
            {
                Destroy(currentMarker.gameObject);
                currentMarker = null;
            }

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
            {
                // Create a new marker at hit point
                GameObject markerObj = Instantiate(markerPrefab, hit.point, Quaternion.identity);
                currentMarker = markerObj.transform;
            }
        }

        if (Input.GetMouseButton(0))
        {

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
            {
                if (currentMarker != null)
                {
                    currentMarker.position = hit.point;
                }
                //Debug.Log("Mouse over: " + hit.collider.name + " at " + hit.point);

                // Optional: visualize in Scene view
                //Debug.DrawLine(ray.origin, hit.point, Color.green);

                // Example: move an object to that point
                // myObject.position = hit.point;
            }
        }

        if (Player1 != null && currentMarker != null)
        {
            // Get target position (keep current Y to stay on ground)
            Vector3 targetPos = new Vector3(currentMarker.position.x, Player1.position.y, currentMarker.position.z);

            // Smoothly move toward target
            Player1.position = Vector3.MoveTowards(
                Player1.position,
                targetPos,
                moveSpeed * Time.deltaTime
            );

            Player1.position = PlayAreaUtils.ClampPosition(Player1.position, FruitManager.Instance.obstacles);
            float distance = Vector3.Distance(Player1.position, targetPos);
            if (distance <= arriveThreshold)
            {
                Destroy(currentMarker.gameObject);
                currentMarker = null;
            }
        }
    }

}
