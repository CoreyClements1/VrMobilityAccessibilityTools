using System.Collections;
using System.Collections.Generic;
using System.Xml;
using Unity.VisualScripting;
using UnityEngine;

public class SfxManager : MonoBehaviour
{

    #region VARIABLES


    [System.Serializable]
    public class SfxClipPair
    {
        public SoundEffect soundEffect;
        public List<AudioClip> audioClips = new List<AudioClip>();
    }

    public enum SoundEffect { Sweep, WoodChop, Shovel, PumpkinChop, Ouch, Fire, Boom, Pickaxe }

    public static SfxManager Instance;

    [SerializeField] private AudioSource audioSourcePrefab;
    [SerializeField] private List<SfxClipPair> soundEffectsList;
    private Dictionary<SoundEffect, List<AudioClip>> soundEffectsDictionary;


    #endregion


    #region MONOBEHAVIOUR


    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);

        Instance = this;

        soundEffectsDictionary = new Dictionary<SoundEffect, List<AudioClip>>();
        foreach (var pair in soundEffectsList)
        {
            if (!soundEffectsDictionary.ContainsKey(pair.soundEffect))
            {
                soundEffectsDictionary.Add(pair.soundEffect, pair.audioClips);
            }
        }
    }


    #endregion


    #region SFX


    public void PlaySfx(SoundEffect sound, Vector3 sfxPosition, bool randomPitch)
    {
        AudioSource audioSource = GameObject.Instantiate(audioSourcePrefab);
        audioSource.transform.position = sfxPosition;
        List<AudioClip> clipsToChoose = soundEffectsDictionary[sound];
        audioSource.clip = clipsToChoose[Random.Range(0, clipsToChoose.Count)];

        if (randomPitch)
        {
            audioSource.pitch = Random.Range(.6f, 1.4f);
        }

        audioSource.Play();

        GameObject.Destroy(audioSource.gameObject, audioSource.clip.length + .5f);
    }


    #endregion


} // END SfxManager.cs
