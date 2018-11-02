using System;
using System.Linq;
using UnityEngine;

#if ENABLE_WINMD_SUPPORT
using Windows.Storage;
#endif // ENABLE_WINMD_SUPPORT

public enum AudioClipType
{
    Welcome,
    Resetting,
    LoadError
}
[Serializable]
public class AudioClipEntry
{
    [SerializeField]
    public AudioClipType clipType;

    [SerializeField]
    public AudioClip clip;
}
public class AudioController : MonoBehaviour
{
    [SerializeField]
    AudioSource audioSource;

    [SerializeField]
    AudioClipEntry[] audioClips;

    public void PlayClip(AudioClipType clipType)
    {
        this.audioSource.clip = this.audioClips.FirstOrDefault(ac => ac.clipType == clipType).clip;

        if (this.audioSource.clip != null)
        {
            this.audioSource.Play();
        }
    }
    void Start()
    {
        var playWelcome = true;

#if ENABLE_WINMD_SUPPORT

        playWelcome = !ApplicationData.Current.LocalSettings.Values.ContainsKey(FIRST_RUN_KEY_NAME);

        if (playWelcome)
        {   
            ApplicationData.Current.LocalSettings.Values[FIRST_RUN_KEY_NAME] = true;
        }

#endif // ENABLE_WINMD_SUPPORT

        if (playWelcome)
        {
            this.PlayClip(AudioClipType.Welcome);
        }
    }
    readonly string FIRST_RUN_KEY_NAME = "FirstRun";
}
