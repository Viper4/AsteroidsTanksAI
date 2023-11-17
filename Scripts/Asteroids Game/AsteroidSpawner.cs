using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidSpawner : MonoBehaviour
{
    [SerializeField] LayerManager layerManager;
    Dictionary<int, List<GameObject>> gameLayers = new Dictionary<int, List<GameObject>>();
    int numLayers = 1;
    [SerializeField] GameObject[] prefabs;
    [SerializeField] float rate = 5;
    [SerializeField] Vector2 xRange;
    [SerializeField] Vector2 zRange;
    [SerializeField] float[] startSpeeds;

    bool[] stoppedLayers;

    void InitGameLayers()
    {
        gameLayers = new Dictionary<int, List<GameObject>>();
        stoppedLayers = new bool[numLayers];
        for (int i = 0; i < numLayers; i++)
        {
            gameLayers.Add(i, new List<GameObject>());
        }
    }

    void Spawn(int layer)
    {
        Vector3 randomDirection = Random.insideUnitSphere;
        randomDirection.y = 0;
        Vector3 randomPosition = Random.Range(0, 4) switch
        {
            0 => new Vector3(Random.Range(xRange.x, xRange.y), 0, zRange.y),
            1 => new Vector3(Random.Range(xRange.x, xRange.y), 0, zRange.x),
            2 => new Vector3(xRange.y, 0, Random.Range(zRange.x, zRange.y)),
            3 => new Vector3(xRange.x, 0, Random.Range(zRange.x, zRange.y)),
            _ => new Vector3(Random.Range(xRange.x, xRange.y), 0, zRange.y),
        };
        int randomIndex = Random.Range(0, prefabs.Length);
        GameObject asteroid = Instantiate(prefabs[randomIndex], randomPosition, Random.rotation, transform);
        asteroid.GetComponent<Rigidbody>().velocity = randomDirection * startSpeeds[randomIndex];
        asteroid.GetComponent<CustomLayers>().gameLayer = layer;
        asteroid.GetComponent<Asteroid>().spawner = this;
        MeshRenderer asteroidRenderer = asteroid.GetComponent<MeshRenderer>();
        asteroidRenderer.enabled = layerManager.activeLayers[layer];
        asteroidRenderer.material.color = Color.HSVToRGB(0 + (float)layer / numLayers, 1, 1);
        gameLayers[layer].Add(asteroid);
    }

    void SpawnLayers()
    {
        for(int i = 0; i < numLayers; i++)
        {
            if(!stoppedLayers[i])
                Spawn(i);
        }
    }

    public void ResetSpawner(AsteroidsTrainer trainer)
    {
        CancelInvoke();
        numLayers = trainer.populationSize;
        InitGameLayers();
        InvokeRepeating(nameof(SpawnLayers), 0.01f, rate);
    }

    public void RemoveAsteroid(int layer, GameObject asteroid)
    {
        gameLayers[layer].Remove(asteroid);
    }

    public void ShowLayer(int layer, bool show)
    {
        foreach(GameObject prefab in gameLayers[layer])
        {
            prefab.GetComponent<MeshRenderer>().enabled = show;
        }
    }

    public void StopLayer(int layer)
    {
        foreach(GameObject asteroid in gameLayers[layer])
        {
            Destroy(asteroid);
        }
        stoppedLayers[layer] = true;
        gameLayers[layer].Clear();
    }
}
