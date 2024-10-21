using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
    AudioSource playing;
    [SerializeField][ReadOnly] AudioClip loadedClip;
    [Space]
    [SerializeField] AudioSource startAudio;
    [SerializeField] AudioSource mainAudio;
    [SerializeField] AudioSource endAudio;

    AudioClip nextAudio;
    bool alreadyFading;

    void Start()
    {
        playing = GetComponent<AudioSource>();
        
        if (startAudio && startAudio.playOnAwake)
        { playing.clip = startAudio.clip; }
        
        else if (mainAudio && mainAudio.playOnAwake)
        { playing.clip = mainAudio.clip; } 
        
        else if (endAudio && endAudio.playOnAwake)
        { playing.clip = endAudio.clip; }
        
        startAudio?.Stop();
        mainAudio?.Stop();
        endAudio?.Stop();
        loadedClip = playing.clip;
        playing.Play();
    }
    
    public void SetVolume(float newVol) { playing.volume = newVol; }
    
    public void PlayStartAfterSeconds(float secs) { PlayAfterSeconds(secs, startAudio); }
    public void PlayMainAfterSeconds(float secs) { PlayAfterSeconds(secs, mainAudio); }
    public void PlayEndAfterSeconds(float secs) { PlayAfterSeconds(secs, endAudio); }

    public void ArbitraryInstantSwitchTo(AudioSource next)
    {
        if (playing && playing.clip == next.clip) return;
        
        if(playing)
            playing.Stop();
        next.Stop();
        loadedClip = playing.clip = next.clip;
        playing.Play();
    }
    
    private void PlayAfterSeconds(float secs, AudioSource aud)
    {
        if (alreadyFading)
        {
            nextAudio = aud.clip;
            return;
        }

        IEnumerator fade = playing && playing.isPlaying ? FadeInto(secs, aud) : FadeInto(secs, aud, true);
        StartCoroutine(fade);
    }

    private IEnumerator FadeInto(float secs, AudioSource aud, bool waitAndPlay = false)
    {
        if (waitAndPlay)
        {
            yield return new WaitForSeconds(secs);
            loadedClip = playing.clip = aud.clip;
            playing.Play();
            yield break;
        }

        alreadyFading = true;
        bool fading = true;
        float increment = playing.volume / (secs / Time.deltaTime);
        nextAudio = aud.clip;
        float prevVol = playing.volume;
        while (fading)
        {
            playing.volume -= increment;
            if (playing.volume <= 0)
            {
                playing.Stop();
                fading = false;
            }
            yield return new WaitForEndOfFrame();
        }
        loadedClip = playing.clip = nextAudio;
        playing.volume = prevVol;
        playing.Play();
        alreadyFading = false;
    }
}
