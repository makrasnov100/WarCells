using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LerpAtStart : MonoBehaviour
{
    Vector3 start = Vector3.zero;
    Vector3 end = Vector3.zero;
    float startTime = 0;
    float duration = 0;

    public void Construct(Vector3 start, Vector3 end, float duration)
    {
        this.start = start;
        this.end = end;
        startTime = Time.time;
        this.duration = duration;
    }

    void Update()
    {
        if (duration == 0)
            return;

        float timeTraveled = Time.time - startTime;
        if (timeTraveled > duration)
            Destroy(gameObject);

        gameObject.transform.position = Vector3.Lerp(start, end, timeTraveled/duration);
    }
}
