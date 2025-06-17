using UnityEngine;
using TMPro;

public class DonneesManager : MonoBehaviour
{
    public static DonneesManager Instance;
    public Rigidbody fakePlayerRb;
    public Transform fakePlayerTransform;
    public string nomNiveau = "Niveau 1 : Zone de Test";
    
    private TMP_Text vitesseText;
    private TMP_Text altitudeText;
    private TMP_Text distanceText;
    private TMP_Text niveauText;
    private TMP_Text chronoText;
    private TMP_Text FPSText;
    private Vector3 positionPrecedente;
    private float distanceParcourue = 0f;
    private float chrono;
    private float deltaTime = 0.0f;
    
    
    private bool chronoActive = false;
    private bool chronoEnPause = false;
    private Sound_Music audioManager;
    
    void Awake()
    {
        if (Instance == null) 
        {
            Instance = this;
            
            audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<Sound_Music>();
        }
        else Destroy(gameObject);
    }
    
    void Start()
    {
        vitesseText = transform.Find("Vitesse").GetComponent<TMP_Text>();
        altitudeText = transform.Find("Altitude").GetComponent<TMP_Text>();
        distanceText = transform.Find("Distance parcourue").GetComponent<TMP_Text>();
        niveauText = transform.Find("Niveau").GetComponent<TMP_Text>();
        chronoText = transform.Find("Chrono").GetComponent<TMP_Text>();
        FPSText = transform.Find("FPS").GetComponent<TMP_Text>();  
        
        positionPrecedente = fakePlayerTransform.position;
        niveauText.text = nomNiveau;
    }
    
    void Update()
    {
        if (fakePlayerRb == null || fakePlayerTransform == null)
            return;
            
        float vitesse = fakePlayerRb.linearVelocity.magnitude * 3.6f;
        float altitude = fakePlayerTransform.position.y + 10f;
        
        
        float distanceStep = Vector3.Distance(positionPrecedente, fakePlayerTransform.position);
        distanceParcourue += distanceStep;
        positionPrecedente = fakePlayerTransform.position;
        
        
        if (chronoActive && !chronoEnPause)
        {
            chrono += Time.deltaTime;
        }
        
        vitesseText.text = "Vitesse : " + vitesse.ToString("F1") + " km/h";
        altitudeText.text = "Altitude : " + altitude.ToString("F1") + " m";
        distanceText.text = "Distance : " + distanceParcourue.ToString("F1") + " m";
        chronoText.text = "Temps : " + FormatTime(chrono);
        
        
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.001f;
        float fps = 1.0f / deltaTime;
        FPSText.text = "FPS : " + Mathf.Ceil(fps).ToString();
    }
    
    string FormatTime(float time)
    {
        int heures = Mathf.FloorToInt(time / 3600f);
        int minutes = Mathf.FloorToInt((time % 3600f) / 60f);
        int secondes = Mathf.FloorToInt(time % 60f);
        return string.Format("{0:00}:{1:00}:{2:00}", heures, minutes, secondes);
    }
    
    public void MettreAJourZone(string nouveauNom)
    {
        if (niveauText != null)
        {
            niveauText.text = nouveauNom;
        }
    }
    
    
    public void DemarrerChrono()
    {
        if (!chronoActive)
        {
            chronoActive = true;
            chrono = 0f; 
            distanceParcourue = 0f; 
            
            
            if (audioManager != null)
            {
               
                audioManager.PlaySFX(audioManager.StartSFX);
               
            }
        }
    }
    

    public void ArreterChrono()
    {
        if (chronoActive && !chronoEnPause)
        {
            chronoEnPause = true;
            
            
            if (audioManager != null)
            {
                
                audioManager.PlaySFX(audioManager.FinishSFX);
                Debug.Log("Course terminée ! Son finish joué. Temps final : " + FormatTime(chrono));
            }
        }
    }
    
   
    public void ResetChrono()
    {
        chronoActive = false;
        chronoEnPause = false;
        chrono = 0f;
        distanceParcourue = 0f;
    }
    
    public void MettreAJourPositionPrecedente()
    {
        positionPrecedente = fakePlayerTransform.position;
    }
}