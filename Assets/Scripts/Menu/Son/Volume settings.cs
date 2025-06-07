using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class Volumesettings : MonoBehaviour
{
    [SerializeField] private AudioMixer myMixer;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider SFXSlider;

    public void SetMusicVolume()
    {
        float volume = musicSlider.value; // slider 0 -> 1
        float dB = Mathf.Lerp(-80f, 10f, volume); // Max = -6 dB (moins fort que 0 dB)
        myMixer.SetFloat("Music", dB);
    }

    public void SetSFXVolume()
    {
        float volume = SFXSlider.value;
        float dB = Mathf.Lerp(-80f, 10f, volume);
        myMixer.SetFloat("SFX", dB);
    }
}
