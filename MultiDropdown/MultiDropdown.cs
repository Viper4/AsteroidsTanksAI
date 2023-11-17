using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class MultiDropdown : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI captionText;
    [SerializeField] RectTransform dropdownList;
    public List<Dropdown.OptionData> options;
    [SerializeField] RectTransform optionTemplate;

    public List<int> values = new List<int>();

    List<RectTransform> optionToggles = new List<RectTransform>();

    public UnityEvent<int, bool> onOptionToggle;

    bool allOn;

    void OnValidate()
    {
        if(captionText != null)
        {
            UpdateCaptionText();
        }
    }

    private void Awake()
    {
        Init();   
    }

    public void Init()
    {
        for(int i = 0; i < options.Count; i++)
        {
            RectTransform newOption = Instantiate(optionTemplate, optionTemplate.parent);

            newOption.GetComponent<Toggle>().SetIsOnWithoutNotify(values.Contains(i));

            newOption.name = optionTemplate.name + " " + i;
            newOption.Find("Item Background").GetComponent<Image>().sprite = options[i].image;
            newOption.Find("Item Label").GetComponent<TextMeshProUGUI>().text = options[i].text;

            optionToggles.Add(newOption);
            newOption.gameObject.SetActive(true);
        }
        optionTemplate.gameObject.SetActive(false);

        UpdateCaptionText();
    }

    public void AddValue(int value)
    {
        Toggle toggleComponent = optionToggles[value].GetComponent<Toggle>();
        toggleComponent.SetIsOnWithoutNotify(true);
        if(!values.Contains(value))
        {
            values.Add(value);
            UpdateCaptionText();
        }
    }

    public void ToggleAll()
    {
        for (int i = 0; i < options.Count; i++)
        {
            Toggle toggleComponent = optionToggles[i].GetComponent<Toggle>();
            if(allOn)
            {
                if (!values.Contains(i))
                {
                    values.Add(i);
                }
            }
            else
            {
                values.Remove(i);
            }
            toggleComponent.isOn = allOn;
        }
        UpdateCaptionText();
        allOn = !allOn;
    }

    public void SwitchAll()
    {
        for(int i = 0; i < options.Count; i++)
        {
            Toggle toggleComponent = optionToggles[i].GetComponent<Toggle>();
            if (!values.Contains(i))
            {
                values.Add(i);
                toggleComponent.isOn = true;
            }
            else
            {
                values.Remove(i);
                toggleComponent.isOn = false;
            }
        }
        UpdateCaptionText();
    }

    void UpdateCaptionText()
    {
        if(values.Count == 0)
        {
            captionText.text = "None";
        }
        else if(values.Count > 1)
        {
            captionText.text = "Mixed";
        }
        else
        {
            int index = values[0];
            if(options.Count > 0 && index >= 0 && index < options.Count)
            {
                captionText.text = options[index].text;
            }
        }
    }

    public void OnOptionToggle(RectTransform toggle)
    {
        int value = optionToggles.IndexOf(toggle);
        bool isOn = toggle.GetComponent<Toggle>().isOn;
        onOptionToggle?.Invoke(value, isOn);

        if(isOn)
        {
            if(!values.Contains(value))
            {
                values.Add(value);
            }
        }
        else
        {
            values.Remove(value);
        }

        UpdateCaptionText();
    }

    public void ToggleDropdown()
    {
        dropdownList.gameObject.SetActive(!dropdownList.gameObject.activeSelf);
    }
}
