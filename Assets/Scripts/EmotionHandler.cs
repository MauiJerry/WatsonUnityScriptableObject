using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // for Text in UI

public class EmotionHandler : MonoBehaviour
{
    // one of these for each emotion
    public GameObject joyObject;
    public GameObject sadnessObject;
    public GameObject fearObject;
    public GameObject disgustObject;
    public GameObject angerObject;
    // language tone
    public GameObject analyticalObject;
    public GameObject confidentObject;
    public GameObject tentativeObject;
    // Social tone
    //Consientious, Extroversion, Agreeable, EmotionalRange
    public GameObject consientiousObject;
    public GameObject extraversionObject;
    public GameObject agreeableObject;
    public GameObject emotionalRangeObject;

    public Text maxEmotionTextUI;

    // we use a Dictionary to map between WatsonToneID and GameObjects to be affected by them
    private static Dictionary<WatsonServiceConnection.WatsonToneID, GameObject> dictionary;

    public float emotion_threshold;
    private float maxEmotionScore = 0f;


    void Start()
    {
        // threshold above which a tone fires the handler
        emotion_threshold = 0.5f;  // for loose demo - above 75% seems to work well - may vary by signal
        dictionary = new Dictionary<WatsonServiceConnection.WatsonToneID, GameObject>
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
            GameObject go = dictionary[watsonToneId];
            if (go != null)
            {
                // Action to be applied
                // if different for different WatsonToneID, use a switch or dictionary
                // simple case is to toggle on/off based on emotion_threshold
                if (score > emotion_threshold)
                {
                    go.SetActive(true);

                }
                else
                    go.SetActive(false);
            }
        }

    }

}
