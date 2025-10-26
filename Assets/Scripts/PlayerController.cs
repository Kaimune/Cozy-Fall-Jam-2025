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
    public ParticleSystem moneyPartical; // assign in inspector
    public ParticleSystem movingPartical; // assign in inspector

    [Header("Fruit Settings")]
    public float baseHeight = 2f;
    public float heightIncrement = 0.5f;

    [Header("punch Settings")]
    public float stunDuration = 2f;
    public float PunchCooldown = 0.5f;

    public bool isMoving;
    private float lastPunchTime = -Mathf.Infinity; // tracks cooldown
    private bool isStunned = false;    // currently stunned?
    private float stunTimer = 0f;

    [HideInInspector] public List<GameObject> collidingFruits = new List<GameObject>();
    private List<GameObject> collectedFruits = new List<GameObject>();
    private Transform currentMarker;
    private bool inDropOffZone = false;
    private List<GameObject> collidingPlayers = new List<GameObject>();
    public PlayerAnimator myAnimator;
    public PlayerSound mySound; //1=pick up 2=drop off 3=Got hit

    public bool inRadioZone = false;
    private GameObject currentRadio;

    private void Start()
    {
        if (indicator != null)
            indicator.SetActive(false);
    }

    private void Update()
    {
        if (isStunned)
        {
            stunTimer -= Time.deltaTime;
            if (stunTimer <= 0f)
            {
                isStunned = false;
            }
            // while stunned we skip inputs & movement
            return;
        }

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

        var emission = movingPartical.emission;
        emission.enabled = isMoving;

    }

    #region Mouse Controls
    private void HandleMouseMovement()
    {
        if (PlayerTransform != null && currentMarker != null)
        {
            Vector3 targetPos = new Vector3(currentMarker.position.x, PlayerTransform.position.y, currentMarker.position.z);
            int fruitCount = collectedFruits.Count;
            float currentMoveSpeed = moveSpeed - (0.1f * fruitCount);
            currentMoveSpeed = Mathf.Max(currentMoveSpeed, moveSpeed * 0.5f);
            currentMoveSpeed = currentMoveSpeed * FruitManager.Instance.globalSpeed;
            PlayerTransform.position = Vector3.MoveTowards(PlayerTransform.position, targetPos, currentMoveSpeed * Time.deltaTime);
            PlayerTransform.position = PlayAreaUtils.ClampPosition(PlayerTransform.position, FruitManager.Instance.obstacles);
            Vector3 moveDir = targetPos - PlayerTransform.position;
            isMoving = moveDir.sqrMagnitude > 0.001f;
            if (moveDir.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDir);
                PlayerTransform.rotation = Quaternion.Slerp(PlayerTransform.rotation, targetRotation, Time.deltaTime * 10f);
            }

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

        if (Input.GetMouseButtonDown(0) && inRadioZone)
        {
            SoundController.Instance.ChangeVolume(false);
        }

        if (Input.GetMouseButtonDown(1) && Time.time - lastPunchTime >= PunchCooldown)
        {
            AttemptPunch();
            lastPunchTime = Time.time;
        }

        if (Input.GetMouseButtonDown(1) && inRadioZone)
        {
            SoundController.Instance.ChangeVolume(true);
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        // Automatically move or create marker based on mouse position
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
        {
            if (currentMarker == null)
            {
                // Create a new marker if none exists
                GameObject markerObj = Instantiate(markerPrefab, hit.point, Quaternion.identity);
                currentMarker = markerObj.transform;
            }
            else
            {
                // Continuously update marker position to follow cursor
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
        int fruitCount = collectedFruits.Count;
        float currentMoveSpeed = moveSpeed - (0.1f * fruitCount);
        currentMoveSpeed = Mathf.Max(currentMoveSpeed, moveSpeed * 0.5f);
        currentMoveSpeed = currentMoveSpeed * FruitManager.Instance.globalSpeed;
        isMoving = inputDir != Vector3.zero;
        if (inputDir != Vector3.zero && PlayerTransform != null)
        {
            Vector3 targetPos = PlayerTransform.position + inputDir * currentMoveSpeed * Time.deltaTime;
            targetPos = PlayAreaUtils.ClampPosition(targetPos, FruitManager.Instance.obstacles);
            PlayerTransform.position = targetPos;

            Quaternion targetRotation = Quaternion.LookRotation(inputDir);
            PlayerTransform.rotation = Quaternion.Slerp(PlayerTransform.rotation, targetRotation, Time.deltaTime * 10f);
        }
    }

    private void HandleKeyboardInput()
    {
        // K key: pick up or drop off
        if (indicator != null && indicator.activeSelf && Input.GetKeyDown(KeyCode.J))
        {
            if (collidingFruits.Count > 0)
                pickUpFruit();
            else if (inDropOffZone)
                dropOffFruit();
        }

        if (Input.GetKeyDown(KeyCode.J)&&inRadioZone)
        {
            SoundController.Instance.ChangeVolume(false);
        }


        if (Input.GetKeyDown(KeyCode.K) && Time.time - lastPunchTime >= PunchCooldown)
        {
            AttemptPunch();
            lastPunchTime = Time.time;
        }

        if (Input.GetKeyDown(KeyCode.K) && inRadioZone)
        {
            SoundController.Instance.ChangeVolume(true);
        }
    }
    #endregion

    #region Collision
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Fruit") && !collidingFruits.Contains(other.gameObject))
        {
            var fruitState = other.gameObject.GetComponent<fruitState>();
            var bombScript = other.gameObject.GetComponent<FruitAI_Bomb>();

            if (bombScript != null && bombScript.isThrown && bombScript.launcher!= gameObject)
            {
                // Call the function on the thrown bomb instead of picking up
                ApplyStun(stunDuration);
                DropAllFruit();
                Debug.Log(gameObject.name + " got hit");
                bombScript.Blast(); // Example function in FruitAI_Bomb             
            }
            else if (fruitState != null && fruitState.active)
            {
                collidingFruits.Add(other.gameObject);
                UpdateIndicator();
            }
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

        if (other.CompareTag("Radio"))
        {
            inRadioZone = true;
            if (!SoundController.Instance.PlayerInZone.Contains(gameObject))
            {
                SoundController.Instance.PlayerInZone.Add(gameObject);               
            }
            SoundController.Instance.UpdateChangeIndicator();
            currentRadio = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Fruit") && collidingFruits.Contains(other.gameObject) && other.gameObject.GetComponent<fruitState>().active)
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

        if (other.CompareTag("Radio"))
        {
            inRadioZone = false;
            if (SoundController.Instance.PlayerInZone.Contains(gameObject))
            {
                SoundController.Instance.PlayerInZone.Remove(gameObject);
            }
            SoundController.Instance.UpdateChangeIndicator();
            currentRadio = null;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Fruit") && !collidingFruits.Contains(other.gameObject) && other.gameObject.GetComponent<fruitState>().active)
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

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && collision.gameObject != this.gameObject)
        {
            if (!collidingPlayers.Contains(collision.gameObject))
                collidingPlayers.Add(collision.gameObject);
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && collision.gameObject != this.gameObject)
        {
            if (collidingPlayers.Contains(collision.gameObject))
                collidingPlayers.Remove(collision.gameObject);
        }
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
            FruitManager.Instance.ActiveFruit.Remove(firstFruit);
            collectedFruits.Add(firstFruit);
            mySound.PlaySound(0);
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
                dropZone.myScore += fruit.GetComponent<fruitState>().fruitValue;
                mySound.PlaySound(1);
                Destroy(fruit);
                moneyPartical.Emit(3); // play the particle system once
                dropZone.UpdateScore();
            }
        }

        collectedFruits.Clear();
        UpdateIndicator();
        Debug.Log(gameObject.name + " dropped off all fruits. Total score: " + dropZone.myScore);
    }
    #endregion

    #region Punch Interaction
    private void AttemptPunch()
    {
        GameObject bombFruit = collectedFruits.Find(f => f != null && f.GetComponent<FruitAI_Bomb>() != null);
        myAnimator.PunchAnimation();
        if (bombFruit != null)
        {
            // Remove from collected list
            collectedFruits.Remove(bombFruit);
            for (int i = 0; i < collectedFruits.Count; i++)
            {
                GameObject fruit = collectedFruits[i];
                if (fruit != null)
                {
                    float newY = baseHeight + i * heightIncrement;
                    fruit.transform.localPosition = new Vector3(0, newY, 0);
                }
            }

            // Call the bomb's throw function, pass in player's forward direction
            FruitAI_Bomb bombScript = bombFruit.GetComponent<FruitAI_Bomb>();
            if (bombScript != null)
            {
                bombFruit.transform.SetParent(null);
                Vector3 throwDir = PlayerTransform.forward; // current facing direction
                bombScript.Throw(throwDir);
                //bombScript.myFruitState.active = true;
                bombScript.launcher = gameObject;
            }

            UpdateIndicator(); // update indicator since a fruit was used
            return; // skip normal punch
        }


        foreach (GameObject otherPlayer in collidingPlayers)
        {
            if (otherPlayer == null) continue;

            // Stun the other player's controller
            var otherController = otherPlayer.GetComponent<PlayerController>();
            if (otherController != null)
            {
                otherController.ApplyStun(stunDuration);
                Debug.Log("Punch");
            }           

            // Only hit the first colliding player
            break;
        }
    }
    public void ApplyStun(float duration)
    {
        isStunned = true;
        myAnimator.StunAnimation(duration);
        mySound.PlaySound(2);
        stunTimer = duration;
    }

    public void DropAllFruit()
    {
        foreach (GameObject fruit in collectedFruits)
        {
            if (fruit != null)
            {
                // Detach from player
                fruit.transform.SetParent(null);

                // Reset position Y to 0.5 (keep current X/Z)
                Vector3 pos = fruit.transform.position;
                fruit.transform.position = new Vector3(pos.x, 0.5f, pos.z);

                // Reactivate fruit if it has a fruitState
                var fruitState = fruit.GetComponent<fruitState>();
                if (fruitState != null)
                    fruitState.active = true;

                // Add back to ActiveFruit list
                if (!FruitManager.Instance.ActiveFruit.Contains(fruit))
                    FruitManager.Instance.ActiveFruit.Add(fruit);
            }
        }

        collectedFruits.Clear();
        UpdateIndicator();
    }
    #endregion
}
