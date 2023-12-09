using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TanksLayerManager : MonoBehaviour
{
    [SerializeField] TanksTrainer trainer;
    [SerializeField] MultiDropdown layerDropdown;

    public bool[] activeLayers;
    bool[] previousLayers;
    TankAgent focusedAgent;
    [SerializeField] TextMeshProUGUI layerTitle;
    [SerializeField] TextMeshProUGUI layerStats;

    void Start()
    {
        for (int i = 0; i < trainer.totalGames; i++)
        {
            layerDropdown.values.Add(i);
            layerDropdown.options.Add(new Dropdown.OptionData() { text = "Layer " + i });
        }
        layerDropdown.Init();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            if (focusedAgent == null)
            {
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, Mathf.Infinity) && hit.transform.TryGetComponent<TankAgent>(out var tankAgent))
                {
                    Focus(tankAgent.customLayers.gameLayer);
                    focusedAgent = tankAgent;
                }
            }
            else
            {
                UnFocus();
            }
        }
        if (focusedAgent != null)
            layerStats.text = focusedAgent.GetStats();
    }

    public void SelectLayer(int layer, bool value)
    {
        activeLayers[layer] = value;
        trainer.ShowAgents(layer, value);
    }

    public void GenerationReset()
    {
        if (activeLayers == null || activeLayers.Length != trainer.totalGames)
        {
            activeLayers = new bool[trainer.totalGames];
            for (int i = 0; i < trainer.totalGames; i++)
            {
                activeLayers[i] = true;
            }
        }
        else
        {
            for (int i = 0; i < trainer.totalGames; i++)
            {
                trainer.ShowAgents(i, activeLayers[i]);
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
    }

    void UnFocus()
    {
        for (int i = 0; i < activeLayers.Length; i++)
        {
            SelectLayer(i, previousLayers[i]);
        }

        focusedAgent = null;
    }
}
