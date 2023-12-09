using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankBullet : MonoBehaviour
{
    CustomLayers customLayers;
    public Tank owner;

    [SerializeField] float bulletSpeed = 5;
    [SerializeField] float dieTime = 5;

    private void Awake()
    {
        customLayers = GetComponent<CustomLayers>();
        GetComponent<Rigidbody>().velocity = transform.forward * bulletSpeed;
        Invoke(nameof(Kill), dieTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent<CustomLayers>(out var otherCustomLayers) || otherCustomLayers.gameLayer == customLayers.gameLayer)
        {
            if (other.CompareTag("Tank"))
            {
                if(other.GetComponent<Tank>() != owner)
                    owner.kills++;
                if(other.TryGetComponent<TankAgent>(out var agent))
                    agent.SetDeath(true);
            }
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
