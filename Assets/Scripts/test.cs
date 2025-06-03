using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class test : MonoBehaviour
{
    public float speed = 5f;
    private Rigidbody rb;

    void Start()
    {
        Debug.Log("Script 'test' lancé");
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 move = new Vector3(x, 0, z).normalized;

        Debug.Log("Déplacement : " + move); // Affiche le vecteur de déplacement

        rb.MovePosition(rb.position + move * speed * Time.fixedDeltaTime);
    }
}
