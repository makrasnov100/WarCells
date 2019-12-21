using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSystemController : MonoBehaviour
{
    private ParticleSystem thisPs;
    private float destroyTime;
    void Start()
    {
        thisPs = gameObject.GetComponent<ParticleSystem>();
        destroyTime = Time.time + 5;
    }

    void Update()
    {
        //When the particle system is done emitting-Destory it
        if (Time.time > destroyTime)
            Destroy(gameObject);
    }
}
