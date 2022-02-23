using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticAudioHandler : MonoBehaviour
{

    public static float stdVolume = 0.5f;
    private static List<AudioSource> audioSrcSoundList = new List<AudioSource>();
    private static AudioSource audioSrcMusic;

    public static AudioSource playSound(AudioClip sound, string goName, float setPitch = 0, float randomPitch = 0f, float volumeSubtraction = 0)
    {
        if (goName == null)
            goName = "tmpAudioSrc";
        GameObject goAudioSrc = new GameObject(goName);
        AudioSource audioSrc = goAudioSrc.AddComponent<AudioSource>();
        audioSrcSoundList.Add(audioSrc);
        audioSrc.playOnAwake = false;
        audioSrc.loop = false;
        audioSrc.volume = stdVolume - volumeSubtraction;
        audioSrc.clip = sound;

        if (setPitch != 0)
            audioSrc.pitch = setPitch;
        if (randomPitch != 0)
            audioSrc.pitch = Random.Range(audioSrc.pitch - randomPitch, audioSrc.pitch + randomPitch);
        
        audioSrc.Play();
        DontDestroyOnLoad(goAudioSrc);
        Destroy(goAudioSrc, audioSrc.clip.length);
        return audioSrc;
    }

    public static AudioSource playMusic(AudioClip musicClip, float volumeSubtraction = 0)
    {
        if (audioSrcMusic != null)
            Destroy(audioSrcMusic);
        GameObject goAudioSrc = new GameObject("tmpAudioSrcMusic");
        AudioSource audioSrc = goAudioSrc.AddComponent<AudioSource>();
        audioSrcMusic = audioSrc;
        audioSrc.volume = stdVolume - volumeSubtraction;
        audioSrc.clip = musicClip;
        audioSrc.loop = true;
        audioSrc.Play();
        DontDestroyOnLoad(goAudioSrc);
        return audioSrc;
    }

    public static AudioSource getAudioSrcMusic()
    {
        return audioSrcMusic;
    }
}
