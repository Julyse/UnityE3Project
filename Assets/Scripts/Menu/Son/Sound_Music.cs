using UnityEngine;

public class Sound_Music : MonoBehaviour
{
    [Header("------Audio Source--------")]
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource SFXSource;

    [Header("------Audio Clip--------")]
    public AudioClip background;
    public AudioClip CriSaut1;
    public AudioClip CriSaut2;
    public AudioClip WalkStepGrass1;
    public AudioClip WalkStepGrass2;
    public AudioClip WalkStepGrass3;
    public AudioClip WalkStepGrass4;
    public AudioClip WalkStepGrass5;
    public AudioClip RunStepGrass1;
    public AudioClip RunStepGrass2;
    public AudioClip RunStepGrass3;
    public AudioClip RunStepGrass4;
    public AudioClip RunStepGrass5;
    public AudioClip Crouch;
    public AudioClip Checkpoint;
    public AudioClip StartSFX;
    public AudioClip FinishSFX;

   
    // public AudioClip x;

    private void Start()
    {
        musicSource.clip = background;
        musicSource.Play();
    }
    public void PlaySFX(AudioClip clip){
        SFXSource.PlayOneShot(clip);
    }

}
// Exemple d'utilisation dans un autre script:
// Sound_Music soundManager;
// private void Awake(){
//     soundManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<Sound_Music>();
// }
// Pour jouer un son:
// soundManager.PlaySFX(soundManager.Checkpoint);
