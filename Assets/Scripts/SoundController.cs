using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class SoundController : MonoBehaviour
{
    public List<GameObject> PlayerInZone;
    public GameObject ChangeIndicator;
    public TextMeshPro VolunmDisplay;
    public float pitchIncrease = 0.1f;   // How much to increase each time
    public float maxPitch = 2f;          // Prevents extreme speed-up
    public float transitionDuration = 0.5f; // Smooth transition time
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

    public void SpeedUpMusic()
    {
        if (audioSources[0] == null) return;

        // Target pitch (clamped so it doesn't go above maxPitch)
        float targetPitch = Mathf.Clamp(audioSources[0].pitch + pitchIncrease, 0.1f, maxPitch);

        // Kill any previous tween on this AudioSource¡¯s pitch (prevent overlap)
        DOTween.Kill(audioSources[0]);

        // Smoothly tween the pitch
        DOTween.To(() => audioSources[0].pitch, x => audioSources[0].pitch = x, targetPitch, transitionDuration)
            .SetEase(Ease.OutSine)
            .SetTarget(audioSources[0]); // So Kill() knows what to stop next time
    }
}
