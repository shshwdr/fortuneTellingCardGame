using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFXManager : Singleton<SFXManager>
{
    AudioSource audioSource;
    public AudioSource bgmSource;
    public float duckVolume = 0.3f;  // 降低到的音量比例
    public float fadeSpeed = 1f;     // 淡出淡入速度
    // Start is called before the first frame update
    void Awake()
    {
         audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void PlaySFX(string clipName, float duckDuration = 0f)
    {
        AudioClip clip = Resources.Load<AudioClip>("sfx/" + clipName);
        if (clip == null)
        {
            Debug.LogWarning($"SFX clip not found: {clipName}");
            return;
        }

        audioSource.PlayOneShot(clip);

        if (duckDuration > 0f)
        {
            StopCoroutine(nameof(DuckBGM)); // 防止多个叠加
            StartCoroutine(DuckBGM(duckDuration));
        }
    }

    private IEnumerator DuckBGM(float duration)
    {
        float originalVolume = bgmSource.volume;

        // 淡出 BGM 音量
        while (bgmSource.volume > duckVolume)
        {
            bgmSource.volume = Mathf.MoveTowards(bgmSource.volume, duckVolume, Time.deltaTime * fadeSpeed);
            yield return null;
        }

        // 保持降低状态一段时间
        yield return new WaitForSeconds(duration);

        // 淡入回原音量
        while (bgmSource.volume < originalVolume)
        {
            bgmSource.volume = Mathf.MoveTowards(bgmSource.volume, originalVolume, Time.deltaTime * fadeSpeed);
            yield return null;
        }

        bgmSource.volume = originalVolume;
    }

    public void PlaySFX(string clipName)
    {
        audioSource.PlayOneShot(Resources.Load<AudioClip>("sfx/"+clipName));
    }

    public void PlayMessage()
    {
        PlaySFX("message");
    }

    public void NewCustomer()
    {
        PlaySFX("Character_new_entry");
    }

    public void DayEnd()
    {
        PlaySFX("Day_end_sucsessfully",2);
    }
    public void DeckShuffle()
    {
        PlaySFX("Deck_shuffle");
    }

    public void Refuse()
    {
        PlaySFX("polite_reffusal",2);
    }

    public void Menu()
    {
        PlaySFX("shop_menu_pop");
    }
    
    
    public void Click()
    {
        PlaySFX("dialogue_option");
    }

    public void CardLock()
    {
        PlaySFX("card reverse back");
        
    }

    public void GoodFortune()
    {
        PlaySFX("good_omens",2);
    }
    public void BadFortune()
    {
        PlaySFX("sad_omen",2);
    }
}
