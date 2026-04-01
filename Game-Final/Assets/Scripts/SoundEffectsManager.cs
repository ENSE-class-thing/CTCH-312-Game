using UnityEngine;

public class SoundEffectsManager : MonoBehaviour
{

    public static SoundEffectsManager instance;

    [SerializeField] private AudioSource soundEffectsObject;
    private bool canPlaySounds = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    private void Start()
    {
        // Delay sound activation by 1 second
        Invoke(nameof(EnableSounds), 1f);
    }

    private void EnableSounds()
    {
        canPlaySounds = true;
    }

    private bool ShouldPlaySound()
    {
        return canPlaySounds;
    }

    public void PlaySoundEffectsClip(AudioClip audioClip, Transform spawnTransform, float volume)
    {
        if (!ShouldPlaySound()) return;

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
        if (!ShouldPlaySound()) return;

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

        public void PlayOneSecondSoundEffectsClip(AudioClip audioClip, Transform spawnTransform, float volume)
    {
        if (!ShouldPlaySound()) return;

        //spawn in game object
        AudioSource audioSource = Instantiate(soundEffectsObject, spawnTransform.position, Quaternion.identity);

        //assign the audioclip
        audioSource.clip  = audioClip;

        //assign volume
        audioSource.volume = volume;

        //play sound
        audioSource.Play();

        //get length of clip
        float clipLength = 1f;

        //Destroy game object
        Destroy(audioSource.gameObject, clipLength);
    }

        public void PlayTenSecondSoundEffectsClip(AudioClip audioClip, Transform spawnTransform, float volume)
    {
        if (!ShouldPlaySound()) return;

        //spawn in game object
        AudioSource audioSource = Instantiate(soundEffectsObject, spawnTransform.position, Quaternion.identity);

        //assign the audioclip
        audioSource.clip  = audioClip;

        //assign volume
        audioSource.volume = volume;

        //play sound
        audioSource.Play();

        //get length of clip
        float clipLength = 10f;

        //Destroy game object
        Destroy(audioSource.gameObject, clipLength);
    }

    public void PlayNineSecondSoundEffectsClip(AudioClip audioClip, Transform spawnTransform, float volume)
    {
        if (!ShouldPlaySound()) return;
        
        //spawn in game object
        AudioSource audioSource = Instantiate(soundEffectsObject, spawnTransform.position, Quaternion.identity);

        //assign the audioclip
        audioSource.clip  = audioClip;

        //assign volume
        audioSource.volume = volume;

        //play sound
        audioSource.Play();

        //get length of clip
        float clipLength = 9f;

        //Destroy game object
        Destroy(audioSource.gameObject, clipLength);
    }




}
