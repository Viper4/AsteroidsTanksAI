using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Asteroid : MonoBehaviour
{
    CustomLayers customLayers;
    public AsteroidSpawner spawner;

    private void Start()
    {
        customLayers = GetComponent<CustomLayers>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other != null && customLayers != null && other.TryGetComponent<CustomLayers>(out var otherCustomLayers) && otherCustomLayers.gameLayer == customLayers.gameLayer)
        {
            spawner.RemoveAsteroid(customLayers.gameLayer, gameObject);
            Destroy(GetComponent<MeshRenderer>().material);
            Destroy(gameObject);
        }
    }
}
