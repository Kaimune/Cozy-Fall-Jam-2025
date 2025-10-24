using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldGenerator : MonoBehaviour
{
    public GameObject prefab; // The object to instantiate
    public float length = 10f;   // Size along X
    public float width = 10f;    // Size along Z
    public float yPosition = 0f; // Fixed Y height
    public float density = 1f;   // Density multiplier (1 = base, 2 = double, etc.)
    public float baseSpacing = 0.5f;

    // Start is called before the first frame update
    void Start()
    {
        GenerateField();
    }

    public void GenerateField()
    {
        if (prefab == null)
        {
            Debug.LogWarning("No prefab assigned!");
            return;
        }

        // Cleanup old objects
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        // Calculate number of columns and rows
        int columns = Mathf.RoundToInt(length * density);
        int rows = Mathf.RoundToInt(width * density);

        // Adjust spacing based on density
        float spacing = baseSpacing / density;

        Vector3 startPos = transform.position; // Start at generator's position


        // Start positions centered

        for (int x = 0; x < columns; x++)
        {
            for (int z = 0; z < rows; z++)
            {
                Vector3 spawnPos = new Vector3(
                    startPos.x + x * spacing,   // Positive X
                    yPosition,
                    startPos.z + z * spacing    // Positive Z
                );

                GameObject obj = Instantiate(prefab, spawnPos, Quaternion.identity, transform);
                obj.name = $"Tile_{x}_{z}";
            }
        }
    }
}
