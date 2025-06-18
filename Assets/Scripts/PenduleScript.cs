using UnityEngine;

public class PenduleScript : MonoBehaviour
{
    [Header("Paramètres de l'oscillation")]
    [SerializeField] private float angleMax = 45f; // Angle de l'amplitude
    [SerializeField] private float vitesse = 2f; // Vitesse d'oscillation
    [SerializeField] private float startDelay = 0f; // Décalage de démarrage

    private bool aCommencé = false;
    private float tempsDépart;

    void Start()
    {
        // On garde le temps de départ pour commencer après startDelay
        tempsDépart = Time.time;
    }

    void Update()
    {
        if (!aCommencé)
        {
            if (Time.time - tempsDépart >= startDelay)
            {
                aCommencé = true;
            }
            else return;
        }

        float angle = angleMax * Mathf.Sin((Time.time - tempsDépart - startDelay) * vitesse);
        transform.localRotation = Quaternion.Euler(0, 0, angle);
    }
}