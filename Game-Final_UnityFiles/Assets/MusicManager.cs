using UnityEngine;
using DG.Tweening;
using System;

public class MusicManager : MonoBehaviour
{
    private static MusicManager instance;
    public AudioSource audioSource;
    public AudioClip music;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        audioSource.clip = music;
        audioSource.loop = true;
        audioSource.volume = 0f;
        audioSource.Play();
        audioSource.DOFade(0.3f, 2f);
    }
}