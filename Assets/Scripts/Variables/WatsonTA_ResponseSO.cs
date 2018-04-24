using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Ryan's ScriptableObject variables
using RoboRyanTron.Unite2017.Variables;

// Watson Tone Analysis Response
// each of the fields SO should be named, or maybe like an Enum?
public class ToneAnalysis_ResponseSO : ScriptableObject 
{

    [SerializeField]
    private FloatVariable joy;
    [SerializeField]
    private FloatVariable sadness;
    [SerializeField]
    private FloatVariable disgust;
    [SerializeField]
    private FloatVariable anger;

    [SerializeField]
    private FloatVariable analytical;
    [SerializeField]
    private FloatVariable confident;
    [SerializeField]
    private FloatVariable tenatative;

    [SerializeField]
    private FloatVariable consientious;
    [SerializeField]
    private FloatVariable extraversion;
    [SerializeField]
    private FloatVariable agreeable;
    [SerializeField]
    private FloatVariable emotionalRange;

    // do we expose these here or only via composition of FloatVariables?
    // need a way to set them in Watson Service Connection
    public float Joy
    {
        get { return joy.Value; }
        set { joy.Value = value; }
    }
}
