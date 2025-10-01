using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFXManager : Singleton<SFXManager>
{
    AudioSource audioSource;
    // Start is called before the first frame update
    void Awake()
    {
         audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void PlaySFX(AudioClip clip)
    {
        audioSource.PlayOneShot(clip);
    }

    public void PlaySFX(string clipName)
    {
        audioSource.PlayOneShot(Resources.Load<AudioClip>("sfx/"+clipName));
    }

    public void PlayMessage()
    {
        PlaySFX("message");
    }
}
