using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FruitManager : MonoBehaviour
{
    public List<GameObject> fruitPrefabs;
    public GameObject[] Players;

    public List<GameObject> UnlockedFruit;

    public List<GameObject> ActiveFruit;

    public float playAreaLength = 10f;   // Size along X
    public float playAreaWidth = 10f;    // Size along Z
    public float spawnHeight = 0f;
    public float spawnintervel = 5f;

    public float globalSpeed = 1f;

    public GameObject[] obstacles; // assign in inspector

    //public Vector3 spawnPosition = Vector3.zero; // Where to spawn
    public Transform parentTransform; // Optional parent in hierarchy
                                      // Update is called once per frame
    private static FruitManager instance;
    public static FruitManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<FruitManager>();
                if (instance == null)
                {
                    Debug.LogError(" No FruitManager found in the scene!");
                }
            }
            return instance;
        }
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this; // Assign the instance
        }
        else if (instance != this)
        {
            Destroy(gameObject); // Ensure only one instance exists
        }

        if (obstacles == null || obstacles.Length == 0)
            obstacles = GameObject.FindGameObjectsWithTag("Obstacle");
    }



    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //FruitSpawner(); // Spawn a random fruit when pressing Space
        }
    }

    private void Start()
    {
        StartCoroutine(FruitSpawnRoutine());
    }

    private IEnumerator FruitSpawnRoutine()
    {
        while (true)
        {
            // Wait 10 seconds between checks
            yield return new WaitForSeconds(spawnintervel);

            // Only spawn if there are fewer than 5 active fruits
            if (ActiveFruit.Count < 5)
            {
                FruitSpawner();
            }
        }
    }

    public void FruitSpawner()
    {
        if (UnlockedFruit == null || UnlockedFruit.Count == 0)
        {
            Debug.LogWarning(" No fruit prefabs assigned!");
            return;
        }

        // Pick a random prefab
        GameObject randomFruit = UnlockedFruit[Random.Range(0, UnlockedFruit.Count)];

        // Calculate random position within the play area
        Vector3 startPos = transform.position; // Play area starts at this object
        float randomX = startPos.x + Random.Range(0f, playAreaLength);
        float randomZ = startPos.z + Random.Range(0f, playAreaWidth);
        Vector3 spawnPos = new Vector3(randomX, spawnHeight, randomZ);


        // Spawn the fruit
        GameObject fruit = Instantiate(randomFruit, spawnPos, Quaternion.identity);
        if (fruit.GetComponent<fruitState>()!= null)
        {
            fruit.GetComponent<fruitState>().active = true;
            fruit.GetComponent<fruitState>().spawnLocation = spawnPos;
        }
        ActiveFruit.Add(fruit);

        // Optionally parent it
        if (parentTransform != null)
        {
            fruit.transform.SetParent(parentTransform);
        }

        fruit.name = $"Fruit_{randomFruit.name}_{Time.frameCount}";
    }
}
