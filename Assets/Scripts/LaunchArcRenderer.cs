using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LaunchArcRenderer : MonoBehaviour
{

    LineRenderer lr;
    PlayerController pc;

    public float v;
    public float angle;
    public int resolution = 10;
    public float maxDistance = 8;
    float g = 15;
    float radianAngle;

    private void Awake()
    {
        lr = GetComponent<LineRenderer>();
        pc = GetComponent<PlayerController>();
    }

    // Start is called before the first frame update
    void Start()
    {   
    }

    //populate LineRenderer with appropriate settings
    public void RenderArc(Vector3 velocity)
    {
        v = velocity.magnitude;
        radianAngle = Mathf.Atan(velocity.y / velocity.x);
        if (velocity.x < 0) radianAngle += Mathf.PI;
        RenderArc(v, radianAngle);
    }

    //populate LineRenderer with appropriate settings
    public void RenderArc(float v, float angle)
    {
        this.v = v;
        radianAngle = angle;
        lr.positionCount = resolution + 1;
        lr.SetPositions(CalculateArcArray());
    }

    //create an array of Vector 3 positions for arc
    Vector3[] CalculateArcArray()
    {
        Vector3[] arcArray = new Vector3[resolution + 1];
        Vector3 pos = Input.mousePosition;
        pos.z = 10;

        for (int i = 0; i <= resolution ; i++)
        {
            float t = (float)i / (float)resolution * v/20;
            arcArray[i] = CalculateArcPoint(t);
        }
        return arcArray;
    }

    Vector3 CalculateArcPoint(float t)
    {
        float x = v * t * Mathf.Cos(radianAngle);
        float y = v * t * Mathf.Sin(radianAngle) - (0.5f * g * t * t);
        return new Vector3(x + transform.position.x + pc.knifeOffset.x, y + transform.position.y + pc.knifeOffset.y);
    }

}
