using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(CircleCollider2D))]
public class PlanetController : NetworkBehaviour {

    [SyncVar]
    public float OrbitSpeed;
    [SyncVar]
    public float RotationSpeed;
    public DrawOrbit thisOrbit;

    [SyncVar]
    private int currentPoint;
    [SyncVar]
    private Vector3 currentFrom;
    [SyncVar]
    private Vector3 currentTo;
    [SyncVar]
    private float currentTimeFrom;

    // Use this for initialization
    override public void OnStartServer()
    {
        thisOrbit.Start(gameObject);
        currentPoint = (int)(Random.value * thisOrbit.positions.Length-1);
        currentFrom = thisOrbit.positions[currentPoint];
        currentTo = thisOrbit.positions[currentPoint + 1];

        transform.position = currentFrom;
        currentTimeFrom = Time.time;
    }

    public override void OnStartClient()
    {
        thisOrbit.Start(gameObject);
    }

    void FixedUpdate()
    {
        if (!isServer) return;
        float distance = (currentTo - transform.position).magnitude;
        if (distance < 0.001f)
        {
            //advance to the next destination
            nextPointInOrbit();
        }
        transform.position = Vector3.Lerp(currentFrom, currentTo, OrbitSpeed * (Time.time - currentTimeFrom));
        transform.Rotate(new Vector3(0f, 0f, RotationSpeed));
    }

    private void nextPointInOrbit()
    {
        currentPoint++;
        if (currentPoint == thisOrbit.positions.Length) currentPoint = 0;

        currentFrom = thisOrbit.positions[currentPoint];
        if (currentPoint<thisOrbit.positions.Length-1)
        {
            currentTo = thisOrbit.positions[currentPoint + 1];
        }
        else
        {
            currentTo = thisOrbit.positions[0];
        }
        currentTimeFrom = Time.time;
    }

}
