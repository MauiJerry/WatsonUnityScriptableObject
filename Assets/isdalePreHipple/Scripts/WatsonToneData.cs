using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class WatsonToneData : ScriptableObject 
{
    public float Joy, Sadness, Fear, Disgust, Anger;
    public float Analytical, Confident, Tenatative;
    public float Consientious, 
            Extraversion, 
            Agreeable, EmotionalRange;
}
