using UnityEngine;
using System.Collections;

public class InfiniteBackground : MonoBehaviour {

    public GameObject background;
    public float tileHorizontal;
    public float tileVertical;

	// Use this for initialization
	void Start () {
        GameObject up = GameObject.Instantiate(background);
        up.name = "bg_up";
        up.transform.Translate(new Vector3(0f, -tileVertical, 0f));
        up.transform.localScale = new Vector3(1f, -1f, 1f);
        up.transform.parent = transform;

        GameObject down = GameObject.Instantiate(background);
        down.name = "bg_down";
        down.transform.Translate(new Vector3(0f, tileVertical, 0f));
        down.transform.localScale = new Vector3(1f, -1f, 1f);
        down.transform.parent = transform;

        GameObject left = GameObject.Instantiate(background);
        left.name = "bg_left";
        left.transform.Translate(new Vector3(-tileHorizontal, 0f, 0f));
        left.transform.localScale = new Vector3(-1f, 1f, 1f);
        left.transform.parent = transform;

        GameObject right = GameObject.Instantiate(background);
        right.name = "bg_right";
        right.transform.Translate(new Vector3(tileHorizontal, 0f, 0f));
        right.transform.localScale = new Vector3(-1f, 1f, 1f);
        right.transform.parent = transform;
    }

    // Update is called once per frame
    void Update () {
	
	}
}
