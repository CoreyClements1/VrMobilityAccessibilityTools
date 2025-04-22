using System.Collections;
using System.Collections.Generic;
using System.Xml;
using Unity.VisualScripting;
using UnityEngine;

public class VMAT_SoundsManager : MonoBehaviour
{

    #region VARIABLES


    [System.Serializable]
    public class SfxClipPair
    {
        public SoundEffectType soundEffectType;
        public AudioClip audioClip;
    }

    public enum SoundEffectType { Open, Close, Select, Navigate }

    [SerializeField] private AudioSource audioSourcePrefab;
    [SerializeField] private List<SfxClipPair> soundEffectsList;
    private Dictionary<SoundEffectType, AudioClip> soundEffectsDictionary;


    #endregion


    #region MONOBEHAVIOUR


    private void Awake()
    {
        soundEffectsDictionary = new Dictionary<SoundEffectType, AudioClip>();
        foreach (var pair in soundEffectsList)
        {
            if (!soundEffectsDictionary.ContainsKey(pair.soundEffectType))
            {
                soundEffectsDictionary.Add(pair.soundEffectType, pair.audioClip);
            }
        }
    }


    #endregion


    #region SFX


    public void PlaySfx(SoundEffectType soundEffectType)
    {
        AudioSource audioSource = GameObject.Instantiate(audioSourcePrefab);
        audioSource.transform.position = transform.position;
        audioSource.clip = soundEffectsDictionary[soundEffectType];
        audioSource.volume = VMAT_Options.Instance.menuAudioVolume;
        audioSource.Play();

        GameObject.Destroy(audioSource.gameObject, audioSource.clip.length + .5f);
    }


    #endregion


} // END SfxManager.cs
