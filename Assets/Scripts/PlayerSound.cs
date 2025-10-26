using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSound : MonoBehaviour
{
    public List<AudioClip> playerSFX = new List<AudioClip>();
    public AudioSource audioSource;

    public void PlaySound(int index)
    {
        // Safety checks
        if (audioSource == null)
        {
            Debug.LogWarning("AudioSource is missing on " + gameObject.name);
            return;
        }

        if (index < 0 || index >= playerSFX.Count)
        {
            Debug.LogWarning("Invalid sound index: " + index);
            return;
        }

        // Play the selected clip
        audioSource.clip = playerSFX[index];
        audioSource.Play();
    }
}
