using UnityEngine;

public class SoundEffectsManager : MonoBehaviour
{

    public static SoundEffectsManager instance;

    [SerializeField] private AudioSource soundEffectsObject;
    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
    }

    public void PlaySoundEffectsClip(AudioClip audioClip, Transform spawnTransform, float volume)
    {
        //spawn in game object
        AudioSource audioSource = Instantiate(soundEffectsObject, spawnTransform.position, Quaternion.identity);

        //assign the audioclip
        audioSource.clip  = audioClip;

        //assign volume
        audioSource.volume = volume;

        //play sound
        audioSource.Play();

        //get length of clip
        float clipLength = audioSource.clip.length;

        //Destroy game object
        Destroy(audioSource.gameObject, clipLength);
    }


    public void PlayThreeSecondSoundEffectsClip(AudioClip audioClip, Transform spawnTransform, float volume)
    {
        //spawn in game object
        AudioSource audioSource = Instantiate(soundEffectsObject, spawnTransform.position, Quaternion.identity);

        //assign the audioclip
        audioSource.clip  = audioClip;

        //assign volume
        audioSource.volume = volume;

        //play sound
        audioSource.Play();

        //get length of clip
        float clipLength = 3f;

        //Destroy game object
        Destroy(audioSource.gameObject, clipLength);
    }


}
