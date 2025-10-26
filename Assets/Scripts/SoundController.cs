using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SoundController : MonoBehaviour
{
    public List<GameObject> PlayerInZone;
    public GameObject ChangeIndicator;
    public TextMeshPro VolunmDisplay;
    public static SoundController Instance { get; private set; }

    public float masterVolume = 0.5f;
    public List<AudioSource> audioSources = new List<AudioSource>();

    private void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void Start()
    {
        UpdateVolunm();
        UpdateChangeIndicator();
    }

    public void UpdateChangeIndicator()
    {
        if(PlayerInZone.Count>0)
        {
            ChangeIndicator.SetActive(true);
        }
        else
        {
            ChangeIndicator.SetActive(false);
        }
    }

    public void ChangeVolume(bool TuneUp) // tune up if true
    {
        masterVolume += TuneUp ? 0.1f : -0.1f;
        masterVolume = Mathf.Clamp01(masterVolume);
        UpdateVolunm();
    }
    public void UpdateVolunm()
    {
        // Convert to percentage
        int percent = Mathf.RoundToInt(masterVolume * 100f);

        // Update TMP text
        if (VolunmDisplay != null)
            VolunmDisplay.text = percent + "%";

        for (int i = 0; i < audioSources.Count; i++)
        {
            AudioSource src = audioSources[i];
            if (src != null)
                src.volume = masterVolume;
        }
    }
}
