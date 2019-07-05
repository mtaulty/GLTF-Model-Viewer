using System;
using System.Linq;
using UnityEngine;

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
#pragma warning disable CS0649
    [SerializeField]
    AudioSource audioSource;

    [SerializeField]
    AudioClipEntry[] audioClips;
#pragma warning restore CS0649

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
        var clipName = clipType.ToString();

        return (PlayerPrefs.HasKey(clipName));
    }
    public bool PlayClipOnceOnly(AudioClipType clipType)
    {
        bool play = !this.HasClipPlayedPreviously(clipType);

        if (play)
        {
            var clipName = clipType.ToString();
            PlayerPrefs.SetFloat(clipName, 1);
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

