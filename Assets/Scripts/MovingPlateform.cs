using UnityEngine;
using System.Collections;


public class MovingPlateform : MonoBehaviour
{
    [SerializeField] GameObject pointA;
    [SerializeField] GameObject pointB;
    [SerializeField] float speed = 10f;
    [SerializeField] float delay = 1f;
    [SerializeField] GameObject platform;
    [SerializeField] bool startMoving = true;

    private bool alreadyMoving = false;

    private Vector3 targetPosition;

    void Start()
    {
        platform.transform.position = pointA.transform.position;
        targetPosition = pointB.transform.position;
        // Si on veut une plateforme qui bouge tout le temps 
        // StartCoroutine(MovePlateform());
        if (startMoving)
        {
            StratMoving();
        }
    }

    public void StratMoving()
    {
        if (alreadyMoving)
        {
            return;
        }
        alreadyMoving = true;
        StartCoroutine(MovePlateform());
    }

    IEnumerator MovePlateform()
    {
        while (true)
        {
            while ((targetPosition - platform.transform.position).sqrMagnitude > 0.01f)
            {
                platform.transform.position = Vector3.MoveTowards(platform.transform.position, targetPosition, speed * Time.deltaTime);
                yield return null;
            }

            targetPosition = (targetPosition == pointA.transform.position)
                ? pointB.transform.position : pointA.transform.position;

            yield return new WaitForSeconds(delay);
        }
    }
}