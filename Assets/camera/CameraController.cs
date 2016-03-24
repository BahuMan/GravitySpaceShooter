using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

    public float accelleration = 2f;
    public float smooth = 0.99f;
    public float offset = 10f;
    public Transform target;

	void Start () {
        //increasing the timescale will  make elipsoid orbits more visible without having to use ridiculous masses and forces.
        //also accelleration will be more dramatic and visible to the naked eye
        Time.timeScale = accelleration;
	}
	
	void LateUpdate () {
        if (!target) return;
        Vector2 camPosition = target.position + (target.up * offset);
        transform.position = Vector2.Lerp(camPosition, transform.position, smooth);
        //after the interpolate, force the z to -1
        transform.position = new Vector3(transform.position.x, transform.position.y, -1f);
	}
}
