using System.Text;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer))]
public class EnemyController : MonoBehaviour {

    public float fireRate = 2.5f;
    public float gunsize = 1.0f;
    public float bulletSpeed = 100f;
    public float enginePower = 100.0f;
    public float turnSpeed = 50.0f;
    public BulletController weapon;

    public float maxSpeed = 0.5f;
    public float turnForce = 10f;
    public float distanceFromPlanet = 10f;
    public float distanceToAttack = 5f;
    public float timeBetweenGoalSwitches = 2f;

    public float gravitationalConstant = 5f;

    public Sprite SearchingSprite;
    public Sprite AttackingSprite;
    public Sprite AvoidingSprite;

    public const int GOAL_SEARCH = 0; //nothing else to do, so try to navigate closer to the player
    public const int GOAL_ATTACK = 1; //the player is close enough to attack. Shoot and try to improve aim
    public const int GOAL_AVOID = 2;  //too close to a planet, try to escape its pull

    public static string[] goalDescriptions =
    {
        "Search for Target",
        "Attack Player",
        "Escape from Planet/Sun"
    };

    public int currentGoal { get; private set; }
    public float lastPrioritySwitchTime { get; private set; } //to avoid dithering between two competing goals
    public Transform targetPlayer { get; private set;} //player I'm currently attacked. In multiplayer, I'll have to set this from a list of players

    private Text DebugPanel;
    private ParticleSystem steam;
    private SpriteRenderer thisSprite;
    private Rigidbody2D thisRigidBody;
    private List<PointEffector2D> gravityWells;  //list of things we'd rather keep away from

    private readonly Quaternion TURN_RIGHT = Quaternion.Euler(0f, 0f, 90f);
    private readonly Quaternion TURN_LEFT = Quaternion.Euler(0f, 0f, -90f);

    // Use this for initialization
    void Start () {

        DebugPanel = GameObject.FindGameObjectWithTag("Debug").GetComponent<Text>();
        targetPlayer = GameObject.FindGameObjectWithTag("Player").transform;
        gravityWells = new List<PointEffector2D>();
        gravityWells.AddRange(GameObject.FindObjectsOfType<PointEffector2D>());
        thisSprite = GetComponent<SpriteRenderer>();
        thisRigidBody = GetComponent<Rigidbody2D>();
        steam = GetComponentInChildren<ParticleSystem>();
        steam.enableEmission = false;

        setGoal(GOAL_AVOID);

        thisRigidBody.velocity = transform.up * 4f;
	
	}
	
	// Update is called once per frame
	void Update () {
	    if (lastPrioritySwitchTime + timeBetweenGoalSwitches < Time.realtimeSinceStartup)
        {
            evaluateGoals();
        }

        switch (currentGoal)
        {
            case GOAL_SEARCH: executeSearch(); break;
            case GOAL_ATTACK: executeAttack(); break;
            case GOAL_AVOID: executeAvoidance(); break;
        }
    }

    private void executeSearch()
    {
        Vector3 directionToPlayer = (targetPlayer.transform.position - transform.position);
        float distanceToPlayer = directionToPlayer.magnitude;
        directionToPlayer = directionToPlayer.normalized;

        //reset debug info
        thisSprite.sprite = SearchingSprite;
        DebugPanel.text = "";

        //are we far enough to bother correcting the velocity?
        if (distanceToPlayer > distanceFromPlanet)
        {

            //try to decrease distance by setting velocity towards player
            float velocityCorrection = Vector3.Angle(thisRigidBody.velocity, directionToPlayer);
            if (Mathf.Abs(velocityCorrection) < 30f)
            {

                if (thisRigidBody.velocity.magnitude < maxSpeed * 0.5f)
                {
                    //we're very low speed, we can use boost:
                    //DebugPanel.text = "almost, boosting";
                    steering_boostInDirection(directionToPlayer, 30f);
                }
                else
                {
                    //DebugPanel.text = "almost, coasting";
                    //pretty close; just look at the player and wait
                    steering_setYaw(directionToPlayer);
                    //no thrust
                    steam.enableEmission = false;
                }
            }
            else if (Mathf.Abs(velocityCorrection) < 90f)
            {

                //relatively small angle. Steering and boosting here might increase speed
                if (thisRigidBody.velocity.magnitude < maxSpeed)
                {
                    //DebugPanel.text = "hard correction, boost";
                    Quaternion neededrotation = Quaternion.FromToRotation(thisRigidBody.velocity, directionToPlayer);
                    Vector3 targetDirection = thisRigidBody.velocity;
                    targetDirection = neededrotation * neededrotation * targetDirection;

                    steering_boostInDirection(targetDirection, 20f);
                }
                else
                {
                    //I want to correct course, but I don't want to increase speed
                    //so my boosting should be done at a 90° angle with current velocity

                    thisSprite.sprite = AttackingSprite;
                    //compare angles with the desired direction:
                    Vector3 goright = TURN_RIGHT * thisRigidBody.velocity;
                    Vector3 goleft = TURN_LEFT * thisRigidBody.velocity;

                    if (Vector3.Angle(directionToPlayer, goright) < Vector3.Angle(directionToPlayer, goleft)) {
                        //DebugPanel.text = "hard correction RIGHT+ " + velocityCorrection;
                        steering_boostInDirection(goright, 20f);
                    }
                    else
                    {
                        //DebugPanel.text = "hard correction LEFT+ " + velocityCorrection;
                        steering_boostInDirection(goleft, 20f);
                    }
                }
            }
            else
            {
                //DebugPanel.text = "reverse";
                //going the wrong way; we need to scrub speed
                steering_boostInDirection(directionToPlayer, 20f);
            }
        }
        else
        {
            DebugPanel.text = "Planet SAFE";
            Debug.Log("Planet SAFE");
        }

    }

    private void steering_boostInDirection(Vector3 direction, float maxAngle)
    {
        float currentAngle = steering_setYaw(direction);
        if (currentAngle < maxAngle)
        {
            Debug.Log("BOOST " + currentAngle + " < " + maxAngle);
            //now that direction was set, apply thrust
            thisRigidBody.AddRelativeForce(new Vector2(0f, enginePower));
            steam.enableEmission = true;
            thisSprite.sprite = AttackingSprite;
        }
        else
        {
            Debug.Log("coast " + currentAngle + " < " + maxAngle);
            //wait for ship to face correct direction; no boost
            steam.enableEmission = false;
            thisSprite.sprite = AvoidingSprite;
        }
    }

    /**
     * this method will add torque in order to steer towards the given direction.
     * as the ship is pointing closer to the target, it will try to slow down so the turning doesn't overshoot the target.
     * Target speed will only be maximum if target direction is exactly left or right.
     * @TODO BUG: targetspeed will be very low if target direction is behind us
     *
     * @returns the angle still left to correct (this is an optimization so I don't have to calculate it in other methods)
     */
    private float steering_setYaw(Vector3 direction)
    {
        thisSprite.sprite = SearchingSprite;
        float currentAngle = Vector3.Angle(direction, transform.up);
        float turnRight = - Mathf.Sign(Vector3.Dot(transform.right, direction));
        float targetTurnSpeed = turnRight * currentAngle / 18f * turnForce;
        if (thisRigidBody.angularVelocity < targetTurnSpeed)
        {
            thisRigidBody.AddTorque(turnForce);
        }
        else
        {
            thisRigidBody.AddTorque(-turnForce);
        }
        return currentAngle;
    }

    private void executeAttack()
    {
        //placeholder
    }

    private void executeAvoidance()
    {
        //first, display all bodies pulling us
        StringBuilder sb = new StringBuilder("Pulling: ");
        foreach (PointEffector2D body in gravityWells)
        {
            sb.Append(body.name).Append(", ");
        }
        DebugPanel.text = sb.ToString();
        //Debug.Log(sb.ToString());

        foreach (PointEffector2D body in gravityWells)
        {
            Vector3 towardsBody = body.transform.position - transform.position;
            float distanceToBody = towardsBody.magnitude;
            towardsBody.Normalize();

            //approximation of the physical formula:
            float escapeVelocity = Mathf.Sqrt(-5f * body.forceMagnitude / distanceToBody);

            //is current velocity aimed straight at this planet?
            if (Vector3.Angle(thisRigidBody.velocity, towardsBody) < 15)
            {
                DebugPanel.text += "aimed straight at planet " + body.name + " esc velocity = " + escapeVelocity + ", cur velocity = " + thisRigidBody.velocity.magnitude;
                Debug.Log("aimed straight at planet " + body.name + " esc velocity = " + escapeVelocity + ", cur velocity = " + thisRigidBody.velocity.magnitude);
                Vector3 newDirection = TURN_LEFT * thisRigidBody.velocity;
                steering_boostInDirection(newDirection, 20f);

            }
            else
            {
                if (thisRigidBody.velocity.magnitude > escapeVelocity)
                {
                    DebugPanel.text += "escape velocity OK " + body.name + " esc velocity = " + escapeVelocity + ", cur velocity = " + thisRigidBody.velocity.magnitude;
                    Debug.Log("escape velocity OK " + body.name + " esc velocity = " + escapeVelocity + ", cur velocity = " + thisRigidBody.velocity.magnitude);
                }
                else
                {

                    DebugPanel.text += "need more speed    " + body.name + " esc velocity = " + escapeVelocity + ", cur velocity = " + thisRigidBody.velocity.magnitude;
                    Debug.Log("need more speed    " + body.name + " esc velocity = " + escapeVelocity + ", cur velocity = " + thisRigidBody.velocity.magnitude);
                    steering_boostInDirection(thisRigidBody.velocity, 80f);
                }
            }
        }

    }

    public void evaluateGoals()
    {
        //placeholder
        setGoal(GOAL_AVOID);
    }

    public void setGoal(int newGoal)
    {
        //Debug.Log("Setting new goal to " + goalDescriptions[newGoal]);
        currentGoal = newGoal;
        lastPrioritySwitchTime = Time.realtimeSinceStartup;
        /*
        switch (newGoal) {
            case GOAL_SEARCH: thisSprite.sprite = SearchingSprite; break;
            case GOAL_ATTACK: thisSprite.sprite = AttackingSprite; break;
            case GOAL_AVOID: thisSprite.sprite = AvoidingSprite; break;
        }
        */
            
    }

    /**
     * if I enter gravity pull of another planet, monitor its proximity and pull
     */
    void OnTriggerEnter2D(Collider2D other)
    {
        //is other object a planet?
        PointEffector2D pulling = other.GetComponent<PointEffector2D>();
        if (pulling)
        {
            Debug.Log("Getting close to " + other.gameObject.name);
            //gravityWells.Add(pulling);
        }
    }

    /**
     * if I enter gravity pull of another planet, monitor its proximity and pull
     */
    void OnTriggerExit2D(Collider2D other)
    {
        //is other object a planet?
        PointEffector2D pulling = other.GetComponent<PointEffector2D>();
        if (pulling)
        {
            Debug.Log("Getting away from " + other.gameObject.name);
            //gravityWells.Remove(pulling);
        }
    }
}
