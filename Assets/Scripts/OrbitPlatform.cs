using UnityEngine;

public class OrbitPlatform : MonoBehaviour
{
    [SerializeField] private Transform platform;
    [SerializeField] private float rotationSpeed = 30f;

    void Update()
    {
        // Tourne autour de l'axe Y du parent
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
    }
}