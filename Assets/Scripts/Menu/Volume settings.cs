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
        float volume = musicSlider.value;
        float normalizedVolume = volume / 100f;
        float dB = Mathf.Log10(Mathf.Max(normalizedVolume, 0.0001f)) * 20f;
        myMixer.SetFloat("Music", dB);
    }
    public void SetSFXVolume()
    {
        float volume = SFXSlider.value;
        float normalizedVolume = volume / 100f;
        float dB = Mathf.Log10(Mathf.Max(normalizedVolume, 0.0001f)) * 20f;
        myMixer.SetFloat("SFX", dB);
    }
}
