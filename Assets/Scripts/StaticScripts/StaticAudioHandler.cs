using System.Collections;
using UnityEngine;

public class StaticAudioHandler : MonoBehaviour
{

    public static float stdVolume = 0.5f;
    private static AudioSource audioSrcMusic;

    public static AudioSource playSound(AudioClip sound, string goName, float setPitch = 0, float randomPitch = 0f, float volumeSubtraction = 0)
    {
        if (sound == null || !SaveLoadData.loadSoundState())
            return null;
        if (goName == null)
            goName = "tmpAudioSrc";
        GameObject goAudioSrc = new GameObject(goName);
        AudioSource audioSrc = goAudioSrc.AddComponent<AudioSource>();
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
        goAudioSrc.AddComponent<StaticAudioHandler>().StartCoroutine(ReleaseSound(audioSrc));
        return audioSrc;
    }

    public static AudioSource playMusic(AudioClip musicClip, float volumeSubtraction = 0)
    {
        if (audioSrcMusic == null)
        {
            GameObject goAudioSrc = new GameObject("tmpAudioSrcMusic");
            audioSrcMusic = goAudioSrc.AddComponent<AudioSource>();
            DontDestroyOnLoad(goAudioSrc);
        }
        AudioSource audioSrc = audioSrcMusic;
        audioSrc.volume = stdVolume - volumeSubtraction;
        audioSrc.pitch = 1;
        audioSrc.mute = !SaveLoadData.loadMusicState();
        audioSrc.clip = musicClip;
        audioSrc.loop = true;
        audioSrc.Play();
        return audioSrc;
    }

    private static IEnumerator ReleaseSound(AudioSource audioSrc)
    {
        yield return new WaitForSecondsRealtime(audioSrc.clip.length / Mathf.Max(0.01f, Mathf.Abs(audioSrc.pitch)));
        if (audioSrc != null)
            Destroy(audioSrc.gameObject);
    }

    public static AudioSource getAudioSrcMusic()
    {
        return audioSrcMusic;
    }
}
