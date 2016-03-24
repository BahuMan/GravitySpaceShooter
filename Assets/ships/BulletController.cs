using UnityEngine;
using UnityEngine.Networking;

public class BulletController : NetworkBehaviour {

    Vector2 initialSpeed;
    Rigidbody2D thisBody;
    public float lifetime = 60.0f;

	// Use this for initialization
	override public void OnStartServer () {
        Destroy(gameObject, lifetime);
        thisBody = GetComponent<Rigidbody2D>();
        thisBody.velocity = this.initialSpeed;
    }

    // Update is called once per frame
    void FixedUpdate () {
	}

    public void pulse(Vector2 force)
    {
        this.initialSpeed = force;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isServer) return;
        //Debug.Log("bullet hit " + collision.gameObject.name);
        Destroy(gameObject);
    }
}
