using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearChildrenOnReset : MonoBehaviour
{
    public void ClearChildren()
    {
        for(int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            Destroy(child.GetComponent<MeshRenderer>().material);
            if(child.TryGetComponent<TrailRenderer>(out var trailRenderer))
                Destroy(trailRenderer.material);
            Destroy(child.gameObject);
        }
    }
}
