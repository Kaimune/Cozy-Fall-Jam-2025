using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Settings")]
    public bool isPlayer2 = false;           // Switch between Player1 (mouse) or Player2 (keyboard)
    public float moveSpeed = 5f;
    public float arriveThreshold = 0.05f;

    [Header("References")]
    public Transform PlayerTransform;        // Player object to move
    public GameObject markerPrefab;          // For mouse player
    public LayerMask groundLayer = Physics.DefaultRaycastLayers;
    public GameObject indicator;             // Assign in inspector
    public GameObject myDropOff;             // Drop-off assigned to this player

    [Header("Fruit Settings")]
    public float baseHeight = 2f;
    public float heightIncrement = 0.5f;

    [HideInInspector] public List<GameObject> collidingFruits = new List<GameObject>();
    private List<GameObject> collectedFruits = new List<GameObject>();
    private Transform currentMarker;
    private bool inDropOffZone = false;

    private void Start()
    {
        if (indicator != null)
            indicator.SetActive(false);
    }

    private void Update()
    {
        if (!isPlayer2)
        {
            HandleMouseMovement();
            HandleMouseInput();
        }
        else
        {
            HandleKeyboardMovement();
            HandleKeyboardInput();
        }
    }

    #region Mouse Controls
    private void HandleMouseMovement()
    {
        if (PlayerTransform != null && currentMarker != null)
        {
            Vector3 targetPos = new Vector3(currentMarker.position.x, PlayerTransform.position.y, currentMarker.position.z);
            PlayerTransform.position = Vector3.MoveTowards(PlayerTransform.position, targetPos, moveSpeed * Time.deltaTime);
            PlayerTransform.position = PlayAreaUtils.ClampPosition(PlayerTransform.position, FruitManager.Instance.obstacles);

            float distance = Vector3.Distance(PlayerTransform.position, targetPos);
            if (distance <= arriveThreshold)
            {
                Destroy(currentMarker.gameObject);
                currentMarker = null;
            }
        }
    }

    private void HandleMouseInput()
    {
        // Left mouse button: pick up or drop off
        if (indicator != null && indicator.activeSelf && Input.GetMouseButtonDown(0))
        {
            if (collidingFruits.Count > 0)
                pickUpFruit();
            else if (inDropOffZone)
                dropOffFruit();
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        // Right mouse button: set marker
        if (Input.GetMouseButtonDown(1))
        {
            if (currentMarker != null)
            {
                Destroy(currentMarker.gameObject);
                currentMarker = null;
            }

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
            {
                GameObject markerObj = Instantiate(markerPrefab, hit.point, Quaternion.identity);
                currentMarker = markerObj.transform;
            }
        }

        // Hold right mouse button: move marker
        if (Input.GetMouseButton(1))
        {
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
            {
                if (currentMarker != null)
                    currentMarker.position = hit.point;
            }
        }
    }
    #endregion

    #region Keyboard Controls
    private void HandleKeyboardMovement()
    {
        float h = Input.GetAxisRaw("Horizontal"); // A/D keys
        float v = Input.GetAxisRaw("Vertical");   // W/S keys
        Vector3 inputDir = new Vector3(h, 0, v).normalized;

        if (inputDir != Vector3.zero && PlayerTransform != null)
        {
            Vector3 targetPos = PlayerTransform.position + inputDir * moveSpeed * Time.deltaTime;
            targetPos = PlayAreaUtils.ClampPosition(targetPos, FruitManager.Instance.obstacles);
            PlayerTransform.position = targetPos;
        }
    }

    private void HandleKeyboardInput()
    {
        // K key: pick up or drop off
        if (indicator != null && indicator.activeSelf && Input.GetKeyDown(KeyCode.Space))
        {
            if (collidingFruits.Count > 0)
                pickUpFruit();
            else if (inDropOffZone)
                dropOffFruit();
        }
    }
    #endregion

    #region Collision
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Fruit") && !collidingFruits.Contains(other.gameObject))
        {
            collidingFruits.Add(other.gameObject);
            UpdateIndicator();
        }

        if (other.CompareTag("DropOff"))
        {
            var dropOff = other.GetComponent<DropOffZone>();
            if (dropOff != null && dropOff.gameObject == myDropOff)
            {
                inDropOffZone = true;
                UpdateIndicator();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Fruit") && collidingFruits.Contains(other.gameObject))
        {
            collidingFruits.Remove(other.gameObject);
            UpdateIndicator();
        }

        if (other.CompareTag("DropOff"))
        {
            var dropOff = other.GetComponent<DropOffZone>();
            if (dropOff != null && dropOff.gameObject == myDropOff)
            {
                inDropOffZone = false;
                UpdateIndicator();
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Fruit") && !collidingFruits.Contains(other.gameObject))
        {
            collidingFruits.Add(other.gameObject);
            UpdateIndicator();
        }
    }

    private void UpdateIndicator()
    {
        if (indicator != null)
            indicator.SetActive(collidingFruits.Count > 0 || inDropOffZone);
    }
    #endregion

    #region Fruit Interaction
    private void pickUpFruit()
    {
        if (collidingFruits.Count == 0) return;

        GameObject firstFruit = collidingFruits[0];
        if (firstFruit != null)
        {
            var fruitState = firstFruit.GetComponent<fruitState>();
            if (fruitState != null)
                fruitState.active = false;

            // Attach fruit and stack
            firstFruit.transform.SetParent(transform);
            float newY = baseHeight + collectedFruits.Count * heightIncrement;
            firstFruit.transform.localPosition = new Vector3(0, newY, 0);

            collectedFruits.Add(firstFruit);
            collidingFruits.RemoveAt(0);
            UpdateIndicator();
        }
    }

    public void dropOffFruit()
    {
        if (!inDropOffZone || myDropOff == null) return;

        DropOffZone dropZone = myDropOff.GetComponent<DropOffZone>();
        if (dropZone == null) return;

        foreach (GameObject fruit in collectedFruits)
        {
            if (fruit != null)
            {
                Destroy(fruit);
                dropZone.myScore += 1;
            }
        }

        collectedFruits.Clear();
        UpdateIndicator();
        Debug.Log(gameObject.name + " dropped off all fruits. Total score: " + dropZone.myScore);
    }
    #endregion
}
