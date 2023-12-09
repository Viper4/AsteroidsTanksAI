using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Events;

public class TanksTrainer : MonoBehaviour
{
    [SerializeField] string modelFolder;
    [SerializeField] string fileName;
    [SerializeField] int[] layers;
    public int totalGames = 20;
    [SerializeField] GameObject agentPrefab;
    [SerializeField] Transform[] agentSpawns;
    int populationSize;

    [SerializeField, Range(0.0001f, 1)] float mutationChance = 0.05f;
    [SerializeField, Range(0, 1)] float mutationStrength = 0.5f;
    [SerializeField] float gameSpeed = 1;
    [SerializeField] float resetTime;
    [SerializeField] NeuralNetwork.Activations activationFunction;

    List<NeuralNetwork> neuralNetworks;
    List<TankAgent> agents;

    int generation = 0;

    public UnityEvent onReset;

    [SerializeField] Transform bulletParent;

    private void Start()
    {
        populationSize = totalGames * agentSpawns.Length;
        // We want even population sizes so we can clone the top half to the bottom and mutate the bottom half
        if (populationSize % 2 != 0)
        {
            Debug.LogWarning("Uneven population size of " + populationSize + ".");
        }

        InitNetworks();
        InvokeRepeating(nameof(InstantiateAgents), 0.1f, resetTime);
    }

    public void OnAgentDeath(int gameLayer)
    {
        foreach(TankAgent agent in agents)
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
        for(int i = 0; i < totalGames; i++)
        {
            for(int j = 0; j < agentSpawns.Length; j++)
            {
                NeuralNetwork neuralNet = new NeuralNetwork(layers, activationFunction);
                neuralNet.Load(modelFolder + fileName);
                neuralNetworks.Add(neuralNet);
            }
        }
    }

    void InstantiateAgents()
    {
        Time.timeScale = gameSpeed;

        if(agents != null)
        {
            SortNetworks();

            foreach(TankAgent agent in agents)
            {
                for(int i = 0; i < agent.meshRenderers.Length; i++)
                {
                    Destroy(agent.meshRenderers[i].material);
                }
                Destroy(agent.gameObject);
            }
        }

        agents = new List<TankAgent>();
        int netIndex = 0;
        for(int i = 0; i < totalGames; i++)
        {
            for(int j = 0; j < agentSpawns.Length; j++)
            {
                TankAgent agent = Instantiate(agentPrefab, agentSpawns[j].position, agentSpawns[j].rotation, transform).GetComponent<TankAgent>();
                agent.neuralNetwork = neuralNetworks[netIndex];
                agent.trainer = this;
                agent.customLayers.gameLayer = i;
                agent.bulletParent = bulletParent;
                foreach(MeshRenderer meshRenderer in agent.meshRenderers)
                {
                    List<Material> newMaterials = new List<Material>();
                    meshRenderer.GetMaterials(newMaterials);
                    for (int k = 0; k < newMaterials.Count; k++)
                    {
                        newMaterials[k].color = Color.HSVToRGB(0 + (float)i / totalGames, 1, 1);
                    }
                    meshRenderer.SetMaterials(newMaterials);
                }
                agents.Add(agent);
                netIndex++;
            }
        }

        for(int i = 0; i < agents.Count; i++)
        {
            for(int j = i + 1; j < agents.Count; j++)
            {
                if (agents[i].customLayers.gameLayer != agents[j].customLayers.gameLayer)
                    Physics.IgnoreCollision(agents[i].tankCollider, agents[j].tankCollider, true);
            }
        }

        onReset?.Invoke();
    }

    void SortNetworks()
    {
        float totalFitness = 0;
        foreach (TankAgent agent in agents)
        {
            totalFitness += agent.UpdateFitness();
        }

        neuralNetworks.Sort();
        neuralNetworks[^1].Save(modelFolder + fileName);
        Debug.Log("Tanks generation " + generation + "\nAverage: " + (totalFitness / agents.Count) + ", Best: " + neuralNetworks[^1].fitness + ", Worst: " + neuralNetworks[0].fitness);

        generation++;
        for (int i = 0; i < populationSize / 2; i++)
        {
            neuralNetworks[i] = neuralNetworks[i + populationSize / 2].Copy(new NeuralNetwork(layers, activationFunction));
            neuralNetworks[i].Mutate(mutationChance, mutationStrength);
        }
    }

    public void ShowAgents(int gameIndex, bool show)
    {
        for(int i = 0; i < agentSpawns.Length; i++)
        {
            agents[gameIndex * agentSpawns.Length + i].Show(show);
        }
    }
}
