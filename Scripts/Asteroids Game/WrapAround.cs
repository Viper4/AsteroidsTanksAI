using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WrapAround : MonoBehaviour
{
    [SerializeField] float maxX = 25;
    [SerializeField] float maxZ = 25;

    void FixedUpdate()
    {
        if(Mathf.Abs(transform.position.x) > maxX)
        {
            transform.position = new Vector3(-transform.position.x, transform.position.y, transform.position.z);
        }
        if(Mathf.Abs(transform.position.z) > maxZ)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, -transform.position.z);
        }
    }
}
