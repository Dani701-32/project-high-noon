using System.Collections;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    [SerializeField][ReadOnly] AudioSource playing;
    [Space]
    [SerializeField] AudioSource startAudio;
    [SerializeField] AudioSource mainAudio;
    [SerializeField] AudioSource endAudio;

    AudioSource nextAudio;
    bool alreadyFading;

    void Start()
    {
        playing = GetComponent<AudioSource>();
        
        if (startAudio && startAudio.playOnAwake)
        { playing = startAudio; }
        
        else if (mainAudio && mainAudio.playOnAwake)
        { playing = mainAudio; } 
        
        else if (endAudio && endAudio.playOnAwake)
        { playing = endAudio; }
        
        startAudio?.Stop();
        mainAudio?.Stop();
        endAudio?.Stop();
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
        playing.Play();
    }
    
    private void PlayAfterSeconds(float secs, AudioSource aud)
    {
        if (alreadyFading)
        {
            nextAudio = aud;
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
            playing = aud;
            playing.Play();
            yield break;
        }

        alreadyFading = true;
        bool fading = true;
        float increment = playing.volume / (secs / Time.deltaTime);
        nextAudio = aud;
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
        playing = nextAudio;
        playing.volume = prevVol;
        playing.Play();
        alreadyFading = false;
    }
}
