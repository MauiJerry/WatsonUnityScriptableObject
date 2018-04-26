using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
// Ryan's ScriptableObject variables
using RoboRyanTron.Unite2017.Variables;

// SO is not a Component so can we require one?
//[RequireComponent(typeof(FloatVariable))]
[RequireComponent(typeof(UnityEngine.UI.Text))]
public class SOUI_FloatVarValue : MonoBehaviour {
    [SerializeField]
    private FloatVariable floatVariable;
    [SerializeField]
    private UnityEngine.UI.Text uiText;

    // Use this for initialization
    void Awake()
    {
        uiText.text = floatVariable.Value.ToString();
    }

    // Use this for initialization
    void Start()
    {
        uiText.text = floatVariable.Value.ToString();
    }
	
	// Update is called once per frame
	void Update () 
    {
        uiText.text = floatVariable.Value.ToString();
        Debug.Log("uiTextUpdated "+ floatVariable.Value.ToString());
	}
}
