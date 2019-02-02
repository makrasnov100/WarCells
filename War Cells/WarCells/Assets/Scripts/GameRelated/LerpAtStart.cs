using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LerpAtStart : MonoBehaviour
{
    Vector3 start = Vector3.zero;
    Vector3 end = Vector3.zero;
    float startTime = 0;
    float duration = 0;

    //Starts the animation by setting start and end locations
    public void Construct(Vector3 start, Vector3 end, float duration)
    {
        this.start = start;
        this.end = end;
        startTime = Time.time;
        this.duration = duration;
    }

    //Performs movement of game object (animation) each frame
    void Update()
    {
        if (duration == 0) //component not constructed yet -> ignore update
            return;

        float timeTraveled = Time.time - startTime;
        if (timeTraveled > duration)
            Destroy(gameObject);

        gameObject.transform.position = Vector3.Lerp(start, end, timeTraveled/duration);
    }
}
