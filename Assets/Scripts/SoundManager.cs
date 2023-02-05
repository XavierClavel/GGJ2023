using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;
using System.Linq;

[Serializable]
public class sfxContainer : IEquatable<sfxContainer>
{
    public AudioClip audioClip;
    public float volume = 1;
    public sfxContainer(AudioClip audioClip, float volume)
    {
        this.audioClip = audioClip;
        this.volume = volume;
    }

    public bool Equals(sfxContainer other)
    {
        if (this.audioClip != other.audioClip) return false;
        if (this.volume != other.volume) return false;
        return true;
    }
}


public enum sfx
{
    grow, endPoint, wallColide
};

public class SoundManager : MonoBehaviour
{
    [SerializeField] List<AudioClip> musics;
    static float LowPitchRange = 0.9f;
    static float HighPitchRange = 1.1f;
    AudioClip sfxClip;
    List<clip> audioIds;

    [Header("Audio Sources")]
    [SerializeField] AudioSource musicSource;
    float volume;
    static int SIZE = System.Enum.GetValues(typeof(sfx)).Length;
    //[NamedArray(typeof(sfx))] public AudioClip[] audioClips = new AudioClip[SIZE];

    [HideInInspector] List<sfxContainer> controlGroup = new List<sfxContainer>();
    static Dictionary<sfx, AudioClip> sfxDictionary;
    static Dictionary<sfx, float> volumeDictionary;
    [Header(" ")]
    public sfxContainer[] audioClips;// = new sfxContainer[SIZE];
    //public sfxContainer[] test;



    public static SoundManager instance;

    struct clip
    {
        public sfx type;
        public GameObject audio;

        public clip(sfx type, GameObject audio)
        {    //Constructor
            this.type = type;
            this.audio = audio;
        }
    }

    private void Awake()
    {
        if (instance == null) instance = this;
        else if (instance != this) Destroy(gameObject);
        DontDestroyOnLoad(this);

        musicSource.Play();

        if (musics.Count > 0) PlayMusic(musics[0]);
        audioIds = new List<clip>();
        List<sfx> sfxList = Enum.GetValues(typeof(sfx)).Cast<sfx>().ToList();

        int i = 0;
        sfxDictionary = new Dictionary<sfx, AudioClip>();
        foreach (sfx sfxElement in sfxList)
        {
            sfxDictionary[sfxElement] = audioClips[i].audioClip;
            i++;
        }

        i = 0;
        volumeDictionary = new Dictionary<sfx, float>();
        foreach (sfx sfxElement in sfxList)
        {
            volumeDictionary[sfxElement] = audioClips[i].volume;
            i++;
        }
    }


    void PlayMusic(AudioClip music)
    {
        musicSource.clip = music;
        musicSource.Play();
    }

    public void StopTime()
    {
        musicSource.Pause();
        if (audioIds.Count > 0)
        {
            foreach (clip audioId in audioIds)
            {
                if (audioId.audio != null)
                {
                    audioId.audio.GetComponent<AudioSource>().Pause();
                }
            }
        }
    }

    public void ResumeTime()
    {
        musicSource.Play();
        if (audioIds.Count > 0)
        {
            foreach (clip audioId in audioIds)
            {
                if (audioId.audio != null)
                {
                    audioId.audio.GetComponent<AudioSource>().Play();
                }
            }
        }
    }

    public static void PlaySfx(Transform pos, sfx type)
    {
        float randomPitch = UnityEngine.Random.Range(LowPitchRange, HighPitchRange);
        AudioClip sfxClip = sfxDictionary[type];
        float volume = volumeDictionary[type];

        instance.PlayClipAt(sfxClip, pos.position, randomPitch, type, volume);

    }

    void PlayClipAt(AudioClip clip, Vector3 pos, float pitch, sfx type, float volume)       //create temporary audio sources for each sfx in order to be able to modify pitch
    {
        GameObject audioContainer = new GameObject("TempAudio"); // create the temporary object
        audioContainer.transform.position = pos; // set its position to localize sound
        AudioSource aSource = audioContainer.AddComponent<AudioSource>();
        aSource.pitch = pitch;
        aSource.clip = clip;
        aSource.volume = volume;
        clip audioId = new clip(type, audioContainer);
        audioIds.Add(audioId);
        aSource.Play(); // start the sound
        StartCoroutine(SfxTimer(clip.length, audioId));
    }

    IEnumerator SfxTimer(float time, clip audioId)  //2nd argument initially aSource
    {
        yield return new WaitForSeconds(time);
        audioIds.Remove(audioId);
        Destroy(audioId.audio);
    }

}
