using UnityEngine;
using System.Collections;

public class PistonScript : MonoBehaviour
{
    [SerializeField] private Transform piston;
    [SerializeField] private float distance = 3f;
    [SerializeField] private float speed = 5f;
    [SerializeField] private float waitBeforeRetract = 1f;

    private Vector3 initialPosition;
    private bool isMoving = false;

    void Start()
    {
        initialPosition = piston.localPosition;
    }

    public void ActivatePiston()
    {
        if (!isMoving)
            StartCoroutine(MovePiston());
    }

    IEnumerator MovePiston()
    {
        isMoving = true;

        Vector3 target = initialPosition + piston.forward * distance;

        // Avancer
        while (Vector3.Distance(piston.localPosition, target) > 0.01f)
        {
            piston.localPosition = Vector3.MoveTowards(piston.localPosition, target, speed * Time.deltaTime);
            yield return null;
        }

        yield return new WaitForSeconds(waitBeforeRetract);

        // Revenir
        while (Vector3.Distance(piston.localPosition, initialPosition) > 0.01f)
        {
            piston.localPosition = Vector3.MoveTowards(piston.localPosition, initialPosition, speed * Time.deltaTime);
            yield return null;
        }

        isMoving = false;
    }
}