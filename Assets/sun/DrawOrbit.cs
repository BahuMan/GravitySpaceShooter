using UnityEngine;
using System.Collections;

[System.Serializable]
public class DrawOrbit: System.Object {

    public float a, b;
    public int resolution;
    public Transform OrbitTarget;

    public Vector3[] positions { get; private set; }

    // Use this for initialization
    public void Start(GameObject myFriend)
    {
        positions = CreateEllipse(a, b, OrbitTarget.position, resolution);


        //trying to render earth's orbit as lines. This was an alternative option.
        //If z is within camera frustrum, this works, but the lines don't look nice.
        //if you enable this code, you should also enable the [RequireComponent] tag at class-level

        LineRenderer lr = myFriend.GetComponent<LineRenderer>();
        lr.SetVertexCount(resolution + 1);
        for (int i = 0; i <= resolution; i++)
        {
            lr.SetPosition(i, positions[i]);
        }

    }

    private static Vector3[] CreateEllipse(float a, float b, Vector3 center, int resolution)
    {

        Vector3[] positions = new Vector3[resolution + 1];

        for (int i = 0; i <= resolution; i++)
        {
            float angle = (float)i / (float)resolution * 2.0f * Mathf.PI;
            positions[i] = new Vector3(a * Mathf.Cos(angle), b * Mathf.Sin(angle), 0f);
            positions[i] = positions[i] + center;
        }

        return positions;
    }
}
