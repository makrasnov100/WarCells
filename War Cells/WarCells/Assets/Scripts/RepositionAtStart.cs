using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RepositionAtStart : MonoBehaviour
{

    public GameObject anchor;
    public Vector3 offset;

    //Places an object to the position of another at start with a offset feature (useful for children)
    void Start()
    {
        transform.position = anchor.transform.position + offset;
    }
}
