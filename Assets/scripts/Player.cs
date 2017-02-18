using UnityEngine;

public class Player : MonoBehaviour {

    public float speed;

	private Rigidbody rb;
    private Vector3 velocity;

    private void Awake ()
    {
        rb = GetComponent<Rigidbody> ();
    }

    private void Update ()
    {
        velocity = new Vector3 (Input.GetAxisRaw ("Horizontal"), 0, Input.GetAxisRaw ("Vertical")).normalized * speed;
    }

    private void FixedUpdate ()
    {
        rb.MovePosition (rb.position + velocity * Time.fixedDeltaTime);
    }

}
