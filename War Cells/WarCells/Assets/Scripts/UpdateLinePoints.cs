using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(LineRenderer))]
public class UpdateLinePoints : MonoBehaviour
{

    LineRenderer lr;
    public List<GameObject> points = new List<GameObject>();

    private void Start()
    {
        lr = GetComponent<LineRenderer>();
    }

    public void AddPoint(GameObject point)
    {
        points.Add(point);
    }

    // Update is called once per frame
    void Update()
    {
        if (points.Count == 0)
            return;

        for (int i = 0; i < points.Count; i++)
        {
            lr.SetPosition(i, points[i].transform.position);
        }
    }
}
