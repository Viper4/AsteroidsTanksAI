using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Events;

public class AsteroidsTrainer : MonoBehaviour
{
    [SerializeField] string modelFolder;
    [SerializeField] string fileName;
    [SerializeField] int[] layers;
    public int populationSize = 20;
    [SerializeField] GameObject agentPrefab;

    [SerializeField] [Range(0.0001f, 1)] float mutationChance = 0.05f;
    [SerializeField] [Range(0, 1)] float mutationStrength = 0.5f;
    [SerializeField] float gameSpeed = 1;
    [SerializeField] float resetTime;
    [SerializeField] NeuralNetwork.Activations activationFunction;

    List<NeuralNetwork> neuralNetworks;
    List<ShipAgent> agents;

    int generation = 0;

    [SerializeField] AsteroidSpawner asteroidSpawner;
    public UnityEvent onReset;

    [SerializeField] Transform bulletParent;

    private void Start()
    {
        // We want even population sizes so we can clone the top half to the bottom and mutate the bottom half
        if(populationSize % 2 != 0)
        {
            populationSize = 20;
        }

        InitNetworks();
        InvokeRepeating(nameof(InstantiateAgents), 0.1f, resetTime);
    }

    public void OnAgentDeath(int gameLayer)
    {
        asteroidSpawner.StopLayer(gameLayer);

        foreach(ShipAgent agent in agents)
        {
            if(!agent.dead)
            {
                return;
            }
        }

        CancelInvoke();
        InvokeRepeating(nameof(InstantiateAgents), 0.1f, resetTime);
    }

    void InitNetworks()
    {
        neuralNetworks = new List<NeuralNetwork>();
        for(int i = 0; i < populationSize; i++)
        {
            NeuralNetwork neuralNet = new NeuralNetwork(layers, activationFunction);
            neuralNet.Load(modelFolder + fileName);
            neuralNetworks.Add(neuralNet);
        }
    }

    void InstantiateAgents()
    {
        Time.timeScale = gameSpeed;

        if(agents != null)
        {
            SortNetworks();

            foreach(ShipAgent agent in agents)
            {
                Destroy(agent.meshRenderer.material);
                Destroy(agent.gameObject);
            }
        }

        agents = new List<ShipAgent>();
        for(int i = 0; i < populationSize; i++)
        {
            ShipAgent agent = Instantiate(agentPrefab, transform.position, transform.rotation, transform).GetComponent<ShipAgent>();
            agent.neuralNetwork = neuralNetworks[i];
            agent.trainer = this;
            agent.customLayers.gameLayer = i;
            agent.bulletParent = bulletParent;
            agent.GetComponent<MeshRenderer>().material.color = Color.HSVToRGB(0 + (float)i / populationSize, 1, 1);
            agents.Add(agent);
        }

        onReset?.Invoke();
    }

    void SortNetworks()
    {
        float totalFitness = 0;
        foreach(ShipAgent agent in agents)
        {
            totalFitness += agent.UpdateFitness();
        }

        neuralNetworks.Sort();
        neuralNetworks[^1].Save(modelFolder + fileName);
        Debug.Log("Asteroids generation " + generation + "\nAverage: " + (totalFitness / agents.Count) + ", Best: " + neuralNetworks[^1].fitness + ", Worst: " + neuralNetworks[0].fitness);

        generation++;
        for(int i = 0; i < populationSize / 2; i++)
        {
            neuralNetworks[i] = neuralNetworks[i + populationSize / 2].Copy(new NeuralNetwork(layers, activationFunction));
            neuralNetworks[i].Mutate(mutationChance, mutationStrength);
        }
    }

    public void ShowAgent(int index, bool show)
    {
        agents[index].Show(show);
    }

    public string GetAgentStats(int index)
    {
        return agents[index].GetStats();
    }
}
