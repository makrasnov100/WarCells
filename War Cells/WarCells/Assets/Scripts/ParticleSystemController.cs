using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSystemController : MonoBehaviour
{
    private ParticleSystem thisPs;
    private float stopEmitTime;
    void Start()
    {
        thisPs = gameObject.GetComponent<ParticleSystem>();
    }

    void Update()
    {
        //When the particle system is done emitting-Destory it
        if(!thisPs.isEmitting)
        {
            stopEmitTime = Time.time;
        }
        else if(stopEmitTime - 5 > Time.time)
        {
            Destroy(gameObject);
        }
    }
}
