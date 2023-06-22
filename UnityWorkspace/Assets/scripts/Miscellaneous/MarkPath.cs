using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;
public class MarkPath : MonoBehaviour
{
    public PathCreator prefabPath;
    public float checkEachXSeconds = 0.2f;

    private PathCreator path;
    private float lastCheck = 0f;
    private int pathIndex = 0;

    // Start is called before the first frame update
    private void OnEnable()
    {
        pathIndex = 0;
        path = Instantiate(prefabPath);
        path.name = "path";
    }

    // Update is called once per frame
    void Update()
    {
        if (lastCheck + checkEachXSeconds < Time.time)
        {
            if (4 > pathIndex)
            {
                path.bezierPath.MovePoint(pathIndex, transform.position);
            }
            else
            {
                path.bezierPath.AddSegmentToEnd(transform.position);
            }
            //Debug.Log("point " + pathIndex);
            pathIndex += 3;
            lastCheck = Time.time;
        }
    }
}
