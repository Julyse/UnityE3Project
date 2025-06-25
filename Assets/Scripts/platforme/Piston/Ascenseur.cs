using UnityEngine;
using System.Collections;

public class Ascenseur : MonoBehaviour
{
    [Header("Paramètres de mouvement")]
    [SerializeField] private Transform[] points;  // liste des points à parcourir
    [SerializeField] private float speed = 10f;
    [SerializeField] private float delay = 1f;
    [SerializeField] private GameObject platform;
    [SerializeField] private bool startMoving = true;

    private int currentIndex = 0;
    private bool alreadyMoving = false;

    void Start()
    {
        if (points.Length < 2)
        {
            Debug.LogWarning("Il faut au moins 2 points pour que la plateforme se déplace.");
            return;
        }

        platform.transform.position = points[0].position;

        if (startMoving)
        {
            StartMoving();
        }
    }

    public void StartMoving()
    {
        if (alreadyMoving) return;

        alreadyMoving = true;
        StartCoroutine(MovePlatform());
    }

    IEnumerator MovePlatform()
    {
        while (true)
        {
            int nextIndex = (currentIndex + 1) % points.Length;
            Vector3 targetPosition = points[nextIndex].position;

            while ((targetPosition - platform.transform.position).sqrMagnitude > 0.01f)
            {
                platform.transform.position = Vector3.MoveTowards(platform.transform.position, targetPosition, speed * Time.deltaTime);
                yield return null;
            }

            currentIndex = nextIndex;
            yield return new WaitForSeconds(delay);
        }
    }
}