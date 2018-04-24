using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // for Text in UI


public class EmotionHandlerUIText : MonoBehaviour
{
    // one of these for each emotion
    public Text joyObject;
    public Text sadnessObject;
    public Text fearObject;
    public Text disgustObject;
    public Text angerObject;
    // language tone
    public Text analyticalObject;
    public Text confidentObject;
    public Text tentativeObject;
    // Social tone
    //Consientious, Extroversion, Agreeable, EmotionalRange
    public Text consientiousObject;
    public Text extraversionObject;
    public Text agreeableObject;
    public Text emotionalRangeObject;

    public Text maxEmotionTextUI;

    // we use a Dictionary to map between WatsonToneID and GameObjects to be affected by them
    private static Dictionary<WatsonServiceConnection.WatsonToneID, Text> dictionary;

    public float emotion_threshold;
    private float maxEmotionScore = 0f;


    void Start()
    {
        // threshold above which a tone fires the handler
        emotion_threshold = 0.5f;  // for loose demo - above 75% seems to work well - may vary by signal
        dictionary = new Dictionary<WatsonServiceConnection.WatsonToneID, Text>
        {
            {WatsonServiceConnection.WatsonToneID.Joy, joyObject},
            {WatsonServiceConnection.WatsonToneID.Sadness, sadnessObject},
            {WatsonServiceConnection.WatsonToneID.Fear, fearObject},
            {WatsonServiceConnection.WatsonToneID.Disgust, disgustObject},
            {WatsonServiceConnection.WatsonToneID.Anger, angerObject},
            {WatsonServiceConnection.WatsonToneID.Analytical, analyticalObject},
            {WatsonServiceConnection.WatsonToneID.Confident, confidentObject},
            {WatsonServiceConnection.WatsonToneID.Tenatative, tentativeObject},
            {WatsonServiceConnection.WatsonToneID.Consientious, consientiousObject},
            {WatsonServiceConnection.WatsonToneID.Extraversion, extraversionObject},
            {WatsonServiceConnection.WatsonToneID.Agreeable, agreeableObject},
            {WatsonServiceConnection.WatsonToneID.EmotionalRange, emotionalRangeObject}
        };
    }

    public void HandleEmotion(WatsonServiceConnection.WatsonToneID watsonToneId, double score)
    {
        Debug.Log("EmotionHandler ");

        // display the highest scoring value
        if (score > maxEmotionScore)
        {
            if (maxEmotionTextUI != null)
                maxEmotionTextUI.text = WatsonServiceConnection.nameForWatsonToneID(watsonToneId);
            maxEmotionScore = (float)score;
        }

        // lookup the corresponding GameObject and apply action to it
        if (dictionary.ContainsKey(watsonToneId))
        {
            Text go = dictionary[watsonToneId];
            if (go != null)
            {
                // Action to be applied
                go.text = score.ToString();
            }
        }

    }

}
