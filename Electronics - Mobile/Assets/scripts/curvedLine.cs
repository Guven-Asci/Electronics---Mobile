using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class curvedLine : MonoBehaviour {

    LineRenderer line;
    int pointCount=20;
    public Vector3 firstProbe;
    public bool isDone=false;

	// Use this for initialization
	void Start () {
        line = GetComponent<LineRenderer>();
        GetComponent<Renderer>().material.color = new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f));
        line.SetVertexCount(pointCount);
        for (int i = 0; i < pointCount; i++)
        {
            line.SetPosition(i, Vector3.zero);
        }
        //line.numPositions = pointCount;
	}
	
	// Update is called once per frame
	void Update () {
        if (!isDone)
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.GetTouch(0).position.x, Input.GetTouch(0).position.y, 9.5f));
            mousePosition.y = 0;


            float yMax = 1f;
            List<Vector3> points = new List<Vector3>();
            for (int i = 0; i < pointCount; i++)
            {

                float x = ((mousePosition.x - firstProbe.x) / (pointCount - 1)) * i,
                    z = ((mousePosition.z - firstProbe.z) / (pointCount - 1)) * i,
                    y = (-1 * x * x + (mousePosition.x - firstProbe.x) * x) + (-1 * z * z + (mousePosition.z - firstProbe.z) * z);

                if (y > yMax)
                    yMax = y;
                points.Add(new Vector3(firstProbe.x + x, y, firstProbe.z + z));
            }
            for (int i = 0; i < pointCount; i++)
            {
                line.SetPosition(i, new Vector3(points[i].x, points[i].y / (yMax / 2), points[i].z));
            }



           
        }
	
	}
    public void finish(Vector3 finalPosition)
    {
        float yMax = 1f;
        List<Vector3> points = new List<Vector3>();
        for (int i = 0; i < pointCount; i++)
        {

            float x = ((finalPosition.x - firstProbe.x) / (pointCount - 1)) * i,
                z = ((finalPosition.z - firstProbe.z) / (pointCount - 1)) * i,
                y = (-1 * x * x + (finalPosition.x - firstProbe.x) * x) + (-1 * z * z + (finalPosition.z - firstProbe.z) * z);

            if (y > yMax)
                yMax = y;
            points.Add(new Vector3(firstProbe.x + x, y, firstProbe.z + z));
        }
        for (int i = 0; i < pointCount; i++)
        {
            line.SetPosition(i, new Vector3(points[i].x, points[i].y / (yMax / 2), points[i].z));
        }
        isDone = true;
    }
}
