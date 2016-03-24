using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController: NetworkBehaviour {

    public float fireRate = 2.5f;
    public float gunsize = 1.0f;
    public float bulletSpeed = 100f;
    public float enginePower = 100.0f;
    public float turnSpeed = 50.0f;
    public BulletController weapon;

    [SyncVar] float thrust;

    private Rigidbody2D thisShip;
    private ParticleSystem steam;

    private float lastFired = 0.0f;

	// Use this for initialization
	override public void OnStartLocalPlayer() {
        thisShip = GetComponent<Rigidbody2D>();
        steam = GetComponentInChildren<ParticleSystem>();
        steam.enableEmission = false;
    }

    // Update is called once per frame
    public void FixedUpdate () {

        if (thrust > 0.1f)
        {
            steam.enableEmission = true;
        }
        else
        {
            steam.enableEmission = false;
        }



        if (!isLocalPlayer) return;

        float rotate = -turnSpeed * Input.GetAxis("Horizontal");
        thisShip.AddTorque(rotate);

        thrust = enginePower * Mathf.Abs(Input.GetAxis("Vertical"));
        thisShip.AddRelativeForce(new Vector2(0f, thrust));

        if (Input.GetButton("Fire1") && ((lastFired + fireRate) < Time.realtimeSinceStartup))
        {
            Vector3 bulletPos = transform.position + transform.up * (gunsize + Mathf.Clamp(Vector3.Dot(transform.up, thisShip.velocity), 0f, 2f));
            Vector3 bulletVelocity = (Vector3)thisShip.velocity + (transform.up * bulletSpeed);
            CmdFire(bulletPos, bulletVelocity);
        }

        //Debug.DrawLine(transform.position, transform.position + (Vector3) thisShip.velocity);
    }

    [Command]
    void CmdFire(Vector3 firePos, Vector3 fireVelocity)
    {
        BulletController bullet = Instantiate<BulletController>(weapon);
        bullet.transform.position = firePos;
        bullet.pulse(fireVelocity);
        NetworkServer.Spawn(bullet.gameObject);
        lastFired = Time.realtimeSinceStartup;
    }
}
