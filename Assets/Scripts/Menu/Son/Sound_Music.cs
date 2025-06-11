using UnityEngine;

public class Sound_Music : MonoBehaviour
{
    [Header("------Audio Source--------")]
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource SFXSource;

    [Header("------Audio Clip--------")]
    public AudioClip background;
    public AudioClip FootSteps;
    // public AudioClip x;
    // public AudioClip x;
    // public AudioClip x;
    // public AudioClip x;
    // public AudioClip x;

    private void Start()
    {
        musicSource.clip = background;
        musicSource.Play();
        SFXSource.clip = FootSteps;
        SFXSource.Play();
    }
    public void PlaySFX(AudioClip clip){
        SFXSource.PlayOneShot(clip);
    }

}
// A mettre dans les scripts pour les sounds effects:

// AudioManager audioManager;
//private void Awake(){
//audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();//(mettre un tag Audio sur
// //Pour mettre le sfx:
//audioManager.PlaySFX(audioManager.LeNomDuSFX);
