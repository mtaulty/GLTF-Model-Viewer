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
    LoadError,
    WelcomeBack,
    FirstModelOpened,
    PickFileFrom3DObjectsFolder,
    ErrorDownloadingModel,
    FirstSharedModelOpened
}
[Serializable]
public class AudioClipEntry
{
    [SerializeField]
    public AudioClipType clipType;

    [SerializeField]
    public AudioClip clip;
}
public class AudioManager : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField]
    AudioSource audioSource;

    [SerializeField]
    AudioClipEntry[] audioClips;
#pragma warning restore 0649

    public void PlayClip(AudioClipType clipType)
    {
        this.audioSource.clip = this.audioClips.FirstOrDefault(ac => ac.clipType == clipType).clip;

        if (this.audioSource.clip != null)
        {
            this.audioSource.Play();
        }
    }
    public bool HasClipPlayedPreviously(AudioClipType clipType)
    {
#if ENABLE_WINMD_SUPPORT
        var clipName = clipType.ToString();

        return (ApplicationData.Current.LocalSettings.Values.ContainsKey(clipName));
#else
        throw new NotImplementedException();
#endif
    }
    public bool PlayClipOnceOnly(AudioClipType clipType)
    {
#if ENABLE_WINMD_SUPPORT
        bool play = !this.HasClipPlayedPreviously(clipType);
#else
        bool play = true;
#endif 

        if (play)
        {
            var clipName = clipType.ToString();

#if ENABLE_WINMD_SUPPORT
            ApplicationData.Current.LocalSettings.Values[clipName] = true;
#endif
            this.PlayClip(clipType);
        }
        return (play);
    }
    void Start()
    {
        if (!this.PlayClipOnceOnly(AudioClipType.Welcome))
        {
            this.PlayClip(AudioClipType.WelcomeBack);
        }
    }
}
