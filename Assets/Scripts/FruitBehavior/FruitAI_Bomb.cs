using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FruitAI_Bomb : MonoBehaviour
{
    public float throwSpeed = 10f;  // units per second
    public float lifetime = 10f;    // seconds before self-destroy
    public float rollSpeed = 360f;
    public float stunDuration = 2;
    public fruitState myFruitState;
    public Transform model;
    public GameObject launcher;

    private Vector3 moveDirection;
    public  bool isThrown = false;

    public void Throw(Vector3 direction)
    {
        // Flatten Y to stay on ground
        transform.position = new Vector3(transform.position.x, FruitManager.Instance.spawnHeight, transform.position.z);
        moveDirection = direction.normalized;
        isThrown = true;
        // Destroy after lifetime
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        if (isThrown)
        {
            float fixedY = transform.position.y; // store current Y
            transform.position += moveDirection * (throwSpeed * FruitManager.Instance.globalSpeed) * Time.deltaTime;
            transform.position = new Vector3(transform.position.x, fixedY, transform.position.z); // keep Y constant

            // Rotate around X or Z for rolling effect
            Vector3 rollAxis = Vector3.Cross(Vector3.up, moveDirection).normalized;
            model.Rotate(rollAxis, rollSpeed * Time.deltaTime, Space.World);
        }
    }

    public void Blast()
    {
        Debug.Log("WaterMelon Hit");
        Destroy(gameObject);
    }
}
