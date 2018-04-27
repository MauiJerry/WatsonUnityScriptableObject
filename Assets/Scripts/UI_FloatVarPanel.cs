using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Ryan's ScriptableObject variables
using RoboRyanTron.Unite2017.Variables;

public class UI_FloatVarPanel : MonoBehaviour
{

    [Tooltip("Value to use as the current ")]
    public FloatReference Variable;

    [Tooltip("Min value that Variable to have no fill on Image.")]
    [SerializeField]
    private FloatReference Min;

    [Tooltip("Max value that Variable can be to fill Image.")]
    [SerializeField]
    private FloatReference Max;

    [Tooltip("Image to set the fill amount on.")]
    public Image Image;

    [SerializeField]
    private UnityEngine.UI.Text valueText;


    // Use this for initialization
    void Awake()
    {
        // could examine Component to get fill, lable, name components
        // but how to find as children/label

        if (valueText == null)
        {
            Debug.LogError("Missing valueText for UISO_FloatVarPanel");
        }
        else
            valueText.text = Variable.Value.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        
        if (valueText == null)
        {
            Debug.LogError("Missing valueText for UISO_FloatVarPanel");
            return;
        }
        valueText.text = Variable.Value.ToString();

        if (Image == null)
        {
            Debug.LogError("Missing Image for ImageFillSetter");
            return;
        }
        Image.fillAmount = Mathf.Clamp01(
            Mathf.InverseLerp(Min, Max, Variable));
    }
}
