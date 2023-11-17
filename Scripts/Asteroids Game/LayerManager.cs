using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LayerManager : MonoBehaviour
{
    [SerializeField] AsteroidsTrainer trainer;
    [SerializeField] AsteroidSpawner asteroidSpawner;
    [SerializeField] MultiDropdown layerDropdown;

    public bool[] activeLayers;
    bool[] previousLayers;
    int focusedLayer = -1;
    [SerializeField] TextMeshProUGUI layerTitle;
    [SerializeField] TextMeshProUGUI layerStats;

    void Start()
    {
        for(int i = 0; i < trainer.populationSize; i++)
        {
            layerDropdown.values.Add(i);
            layerDropdown.options.Add(new Dropdown.OptionData() { text = "Layer " + i });
        }
        layerDropdown.Init();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Mouse0))
        {
            if(focusedLayer == -1)
            {
                if(Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, Mathf.Infinity) && hit.transform.TryGetComponent<CustomLayers>(out var customLayers))
                {
                    Focus(customLayers.gameLayer);
                }
            }
            else
            {
                UnFocus();
            }
        }
        if(focusedLayer != -1)
            layerStats.text = trainer.GetAgentStats(focusedLayer);
    }

    public void SelectLayer(int layer, bool value)
    {
        activeLayers[layer] = value;
        trainer.ShowAgent(layer, value);
        asteroidSpawner.ShowLayer(layer, value);
    }

    public void GenerationReset()
    {
        if(activeLayers == null || activeLayers.Length != trainer.populationSize)
        {
            activeLayers = new bool[trainer.populationSize];
            for(int i = 0; i < trainer.populationSize; i++)
            {
                activeLayers[i] = true;
            }
        }
        else
        {
            for(int i = 0; i < trainer.populationSize; i++)
            {
                trainer.ShowAgent(i, activeLayers[i]);
                asteroidSpawner.ShowLayer(i, activeLayers[i]);
            }
        }
    }

    void Focus(int layer)
    {
        previousLayers = new bool[activeLayers.Length];
        System.Array.Copy(activeLayers, previousLayers, activeLayers.Length);
        for (int i = 0; i < activeLayers.Length; i++)
        {
            SelectLayer(i, i == layer);
        }
        layerTitle.text = "Layer " + layer;

        focusedLayer = layer;
    }

    void UnFocus()
    {
        for (int i = 0; i < activeLayers.Length; i++)
        {
            SelectLayer(i, previousLayers[i]);
        }

        focusedLayer = -1;
    }
}
