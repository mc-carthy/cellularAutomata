using UnityEngine;

public class Player2D : MonoBehaviour {

    public float speed;

	private Rigidbody2D rb;
    private Vector2 velocity;

    private void Awake ()
    {
        rb = GetComponent<Rigidbody2D> ();
    }

    private void Update ()
    {
        velocity = new Vector2 (Input.GetAxisRaw ("Horizontal"), Input.GetAxisRaw ("Vertical")).normalized * speed;
    }

    private void FixedUpdate ()
    {
        rb.MovePosition (rb.position + velocity * Time.fixedDeltaTime);
    }

}
