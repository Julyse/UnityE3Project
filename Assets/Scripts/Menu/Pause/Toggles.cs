using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ToggleManager : MonoBehaviour
{
    public Toggle toggleVitesse;
    public Toggle toggleAltitude;
    public Toggle toggleDistance;
    public Toggle toggleNiveau;
    public Toggle toggleChrono;
    public Toggle toggleFPS;

    public TMP_Text vitesseText;
    public TMP_Text altitudeText;
    public TMP_Text distanceText;
    public TMP_Text niveauText;
    public TMP_Text chronoText;
    public TMP_Text FPSText;

    void Start()
    {
        // Ajout des listeners pour cacher/montrer et réordonner
        toggleVitesse.onValueChanged.AddListener((isOn) => UpdateDisplay());
        toggleAltitude.onValueChanged.AddListener((isOn) => UpdateDisplay());
        toggleDistance.onValueChanged.AddListener((isOn) => UpdateDisplay());
        toggleNiveau.onValueChanged.AddListener((isOn) => UpdateDisplay());
        toggleChrono.onValueChanged.AddListener((isOn) => UpdateDisplay());
        toggleFPS.onValueChanged.AddListener((isOn) => UpdateDisplay());

        UpdateDisplay();
    }

    void UpdateDisplay()
    {
        // Active/désactive Texte
        vitesseText.enabled = toggleVitesse.isOn;
        altitudeText.enabled = toggleAltitude.isOn;
        distanceText.enabled = toggleDistance.isOn;
        niveauText.enabled = toggleNiveau.isOn;
        chronoText.enabled = toggleChrono.isOn;
        FPSText.enabled = toggleFPS.isOn;

        // Réorganiser l’ordre dans la hiérarchie 
 
        int index = 0;

        if (toggleVitesse.isOn)
            vitesseText.transform.SetSiblingIndex(index++);
        if (toggleAltitude.isOn)
            altitudeText.transform.SetSiblingIndex(index++);
        if (toggleDistance.isOn)
            distanceText.transform.SetSiblingIndex(index++);
        if (toggleNiveau.isOn)
            niveauText.transform.SetSiblingIndex(index++);
        if (toggleChrono.isOn)
            chronoText.transform.SetSiblingIndex(index++);
        if (toggleFPS.isOn)
            FPSText.transform.SetSiblingIndex(index++);
    }
}
