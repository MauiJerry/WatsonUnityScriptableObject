//using System.Collections;
//using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Ryan's ScriptableObject variables
using RoboRyanTron.Unite2017.Variables;

public class TextPanelFiller : MonoBehaviour
{
    [SerializeField]
    StringVariable soText;
    [SerializeField]
    WatchedStringVariable wsoText;

    [SerializeField]
    Text displayText;

    /// <summary>
    /// Update displayText based on Value of StringVariable.
    /// might be better to use event based notification
    /// </summary>
    private void Update()
    {
        if (soText == null)
        {
            if (wsoText == null)
                displayText.text = "No SO StringVariable";
            else
                displayText.text = wsoText.Value;
        } else
            displayText.text = soText.Value;
    }

}
