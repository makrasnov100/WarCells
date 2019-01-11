using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RepositionAtStart : MonoBehaviour
{

    public GameObject anchor;
    public Vector3 offset;

    // Start is called before the first frame update
    void Start()
    {
        transform.position = anchor.transform.position + offset;
    }
}
