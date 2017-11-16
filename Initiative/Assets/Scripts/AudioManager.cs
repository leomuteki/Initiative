using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : Singleton<AudioManager>
{

    public float highPitchRange = 1.2f;
    public float lowPitchRange = 0.7f;
    public AudioSource BGMSource;
    public AudioSource SFXSource;

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(Instance);
    }

    /// <summary>
    /// Set background music.
    /// </summary>
    public void SetBGM(AudioClip new_clip)
    {
        BGMSource.clip = new_clip;
        BGMSource.Play();
    }

    //Used to play single sound clips.
    public void PlayAudio(string clip_name)
    {
        SFXSource.clip = ResourceManager.Instance.GetAudio(clip_name);
        SFXSource.Play();
    }

    public void PlayAudioRandonPitch(string clip_name)
    {
        //Choose a random pitch to play back our clip at between our high and low pitch ranges.
        float randomPitch = Random.Range(lowPitchRange, highPitchRange);
        SFXSource.pitch = randomPitch;
        SFXSource.clip = ResourceManager.Instance.GetAudio(clip_name);
        SFXSource.Play();
    }

}
