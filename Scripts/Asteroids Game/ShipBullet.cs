using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipBullet : MonoBehaviour
{
    CustomLayers layerManager;
    public Ship owner;

    [SerializeField] float bulletSpeed = 5;
    [SerializeField] float dieTime = 5;

    private void Awake()
    {
        layerManager = GetComponent<CustomLayers>();
        GetComponent<Rigidbody>().velocity = transform.forward * bulletSpeed;
        Invoke(nameof(Kill), dieTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.GetComponent<CustomLayers>().gameLayer == layerManager.gameLayer)
        {
            if(other.transform.CompareTag("Asteroid") && owner != null)
                owner.asteroidsDestroyed++;
            Kill();
        }
    }

    void Kill()
    {
        owner.onShow.RemoveListener(Show);
        // Need to clean up material instances to stop memory leak
        Destroy(GetComponent<MeshRenderer>().material);
        Destroy(GetComponent<TrailRenderer>().material);
        Destroy(gameObject);
    }

    public void Show(bool value)
    {
        GetComponent<MeshRenderer>().enabled = value;
        GetComponent<TrailRenderer>().enabled = value;
    }
}
