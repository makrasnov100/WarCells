using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LerpInRadius : MonoBehaviour
{
    Vector3 initParentPos;
    Vector3 parentOffset = Vector3.zero;

    Vector3 start = Vector3.zero;
    Vector3 curStart = Vector3.zero;
    Vector3 curEnd = Vector3.zero;
    float curDuration = 0;
    float startTime = -1;
    float radius = 0;
    float minTime = 0;
    float maxTime = 0;

    public void Construct(float radius, float minTime, float maxTime)
    {
        if(transform.parent != null)
            initParentPos = transform.parent.position;
        start = transform.position;
        curStart = transform.position;
        this.radius = radius;
        this.minTime = minTime;
        this.maxTime = maxTime;
        curDuration = Random.Range(minTime, maxTime);
        curEnd = new Vector3(Random.Range(start.x-radius, start.x+radius), Random.Range(start.y-radius, start.y + radius));
        startTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if (startTime == -1)
            return;

        float timePassed = Time.time - startTime;
        if (timePassed > curDuration)
        {
            startTime = Time.time;
            curStart = transform.position;
            curDuration = Random.Range(minTime, maxTime);
            curEnd = new Vector3(Random.Range(start.x - radius, start.x + radius), Random.Range(start.y - radius, start.y + radius));
        }
        else
        {
            if(transform.parent != null)
                parentOffset = transform.parent.position - initParentPos;
            transform.position = Vector3.Lerp(curStart, curEnd + parentOffset, timePassed / curDuration);
        }
    }
}
