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

        if (play)
        {
            var clipName = clipType.ToString();
            ApplicationData.Current.LocalSettings.Values[clipName] = true;
            this.PlayClip(clipType);
        }
        return (play);
#else
        throw new NotImplementedException();
#endif 
    }
    void Start()
    {
        if (!this.PlayClipOnceOnly(AudioClipType.Welcome))
        {
            this.PlayClip(AudioClipType.WelcomeBack);
        }
    }
    readonly string FIRST_RUN_KEY_NAME = "FirstRun";
}
