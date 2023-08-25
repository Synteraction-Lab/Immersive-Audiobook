using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;

public class ValueChanger : MonoBehaviour
{
    public enum Field
    {
        Frequency,
        Timing,
        Scale
    }

    public Field listeningField;
    private ExperimentManager expManager;

    private TextMeshProUGUI value;
    public static UnityEvent onValueChange = new UnityEvent();

    // Start is called before the first frame update
    void Start()
    {
        expManager = FindObjectOfType<ExperimentManager>();
        if (listeningField != null)
        {
            value = GetComponent<TextMeshProUGUI>();
        }
        onValueChange.AddListener(UpdateValues);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void UpdateValues()
    {
        if(listeningField == Field.Frequency)
        {
            value.text = expManager.GetMeta().frequency.ToString();
            return;
        }
        if (listeningField == Field.Timing)
        {
            value.text = ((int) (expManager.GetMeta().timingOffset)).ToString();
            return;
        }
        if (listeningField == Field.Scale)
        {
            value.text = (Mathf.Round(expManager.GetMeta().scale * 10f) * 0.1f).ToString();
            return;
        }
    }
}
