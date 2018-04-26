using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// change of mind. instead of one TA object, we use multiple FloatVariables
// in the Service Connector itself.
// this seems to be better approach for composablity
// so this class eliminated in next implementation
//[CreateAssetMenu]
public class WatsonToneData : ScriptableObject 
{
    public float Joy, Sadness, Fear, Disgust, Anger;
    public float Analytical, Confident, Tenatative;
    public float Consientious, 
            Extraversion, 
            Agreeable, EmotionalRange;
}
