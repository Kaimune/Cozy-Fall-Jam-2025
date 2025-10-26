using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasManager : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject NotePad;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Space))
        {
            if (NotePad != null)
            {
                bool newState = !NotePad.activeSelf;
                NotePad.SetActive(newState);
            }
        }
    }
}
