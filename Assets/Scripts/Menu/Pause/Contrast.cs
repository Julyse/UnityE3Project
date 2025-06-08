using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ContrastSlider : MonoBehaviour
{
    public Slider contrastSlider;
    public Volume postProcessVolume;

    private ColorAdjustments colorAdjustments;

    void Start()
    {
        if (postProcessVolume == null)
        {
            Debug.LogError("PostProcessVolume non assign� !");
            return;
        }

        if (!postProcessVolume.profile.TryGet<ColorAdjustments>(out colorAdjustments))
        {
            Debug.LogError("ColorAdjustments introuvable dans le Volume !");
            return;
        }

        colorAdjustments.active = true;
        colorAdjustments.contrast.value = 50f; // forcer une valeur visible au d�part

        contrastSlider.minValue = -100f;
        contrastSlider.maxValue = 100f;
        contrastSlider.value = 50f;

        contrastSlider.onValueChanged.AddListener(OnContrastChanged);
    }

    void OnContrastChanged(float value)
    {
        Debug.Log("Slider chang�: " + value);
        if (colorAdjustments != null)
        {
            colorAdjustments.active = true;
            colorAdjustments.contrast.value = value;
        }
    }
}
