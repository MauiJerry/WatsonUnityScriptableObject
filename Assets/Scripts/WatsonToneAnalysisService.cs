/// <summary>
/// WatsonToneAnalysisService: Watson ToneAnalysis Service Connector
/// built as MonoBehavior but data passed in and out as Scriptable Objects
/// ToneAnalysis depends on the RecognizedText ScriptableObject
/// 
/// based on the various ExampleService.cs in SDK and RustyOldDrake GitHub
///   Note the IBM code uses its own Logging system, not Unity's Debug
/// Might be better to split in two services but for now do combined here.
/// RustyOldDrake and Watson ExampleStreaming all contain this in header
/* ############# HELLO VIRTUAL WORLD
* Copyright 2015 IBM Corp.All Rights Reserved.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
* THIS IS VERY VERY VERY VERY VERY VERY VERY VERY VERY VERY VERY VERY VERY VERY VERY VERY VERY VERY ROUGH CODE - WARNING :) 
*/
/// </summary>
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using IBM.Watson.DeveloperCloud.Logging;
using IBM.Watson.DeveloperCloud.Services.SpeechToText.v1;
using IBM.Watson.DeveloperCloud.Utilities;
using IBM.Watson.DeveloperCloud.DataTypes;

// added this from the TONE ANALYZER . CS file
using IBM.Watson.DeveloperCloud.Services.ToneAnalyzer.v3;
using IBM.Watson.DeveloperCloud.Connection;

// Ryan's ScriptableObject variables
using RoboRyanTron.Unite2017.Variables;

public class WatsonToneAnalysisService : MonoBehaviour
{
    [SerializeField]
    WatsonTACredentialSO ta_credentialSO;

    // Text to be analyized - from speech to text perhaps
    [SerializeField]
    WatchedStringVariable textToAnalyze;

    // collection of floats returned by Watson Tone Analysis
    // change of mind. instead of one TA object, we use multiple FloatVariables
    // this seems to be better approach for composablity
    //[SerializeField]
    //ToneAnalysis_ResponseSO toneAnalysisResponse;

    [SerializeField]
    private FloatVariable TA_EmotionThreshold; // values lower than this are ignored

    // Tone Analysis Reponses
    // Emotion Category
    [SerializeField]
    private FloatVariable TA_Joy;
    [SerializeField]
    private FloatVariable TA_Sadness;
    [SerializeField]
    private FloatVariable TA_Fear;
    [SerializeField]
    private FloatVariable TA_Disgust;
    [SerializeField]
    private FloatVariable TA_Anger;

    // Sentence Category
    [SerializeField]
    private FloatVariable TA_Analytical;
    [SerializeField]
    private FloatVariable TA_Confident;
    [SerializeField]
    private FloatVariable TA_Tenatative;

    /* Other Categories unsupported at present
    [SerializeField]
    private FloatVariable TA_Consientious;
    [SerializeField]
    private FloatVariable TA_Extraversion;
    [SerializeField]
    private FloatVariable TA_Agreeable;
    [SerializeField]
    private FloatVariable TA_EmotionalRange;
    */

    //------------------
    private Credentials credentials_STT;
    private Credentials credentials_TONE;

    private ToneAnalyzer _toneAnalyzer;
    private string _toneAnalyzerVersionDate = "2017-05-26";

    /// <summary>
    /// Awake this instance.
    ///   use Awake to copy values to the Watson data structure
    /// </summary>
	void Awake()
    {
        // Watson logging initialization
        LogSystem.InstallDefaultReactors();

        print("ToneAnalysis user: " + ta_credentialSO.Username);
        print("ToneAnalysis  pwd: " + ta_credentialSO.Password);
        print("ToneAnalysis  url: " + ta_credentialSO.URL);

        credentials_TONE = new Credentials(
            ta_credentialSO.Username,
            ta_credentialSO.Password,
            ta_credentialSO.URL);

        textToAnalyze.OnChanged += AnalyzeText;
    }

    void OnDestroy()
    {
        textToAnalyze.OnChanged -= AnalyzeText;
        // anthing needed to shutdown the Watson Service?
        // apparently not.
    }

    //------------------
    /// <summary>
    /// Start() method connects to the ToneAnalysis Watson Serice 
    /// </summary>
    void Start()
    {
        _toneAnalyzer = new ToneAnalyzer(credentials_TONE);
        _toneAnalyzer.VersionDate = _toneAnalyzerVersionDate;

    }

    //private void Update()
    //{
    //    // alternative to watched string - check if changed and sent to analysis
    //}

    /// <summary>
    /// Invoked when Watson Tone Analysis failure occurs
    /// </summary>
    /// <param name="error">Error.</param>
    /// <param name="customData">Custom data.</param>
    private void OnFail(RESTConnector.Error error, Dictionary<string, object> customData)
    {
        Log.Error("Watson Tone Analysis.OnFail()", "Error received: {0}", error.ToString());
    }

    /// <summary>
    /// Invoked when Watched String changes and sends to Watson ToneAnalysis 
    /// </summary>
    private void AnalyzeText()
    {
        Log.Debug("WatsonToneAnalysisService.AnalyzeText()", "Analyze: {0}", textToAnalyze.Value);

        if (!_toneAnalyzer.GetToneAnalyze(OnGetToneAnalyze, OnFail, textToAnalyze.Value))
            Log.Debug("WatsonToneAnalysisService.AnalyzeText()", "Failed to analyze!");
    }


    /// <summary>
    /// Invoked by Watson Tone Analysis to handle response to submission
    /// </summary>
    /// <param name="resp">Resp.</param>
    /// <param name="customData">Custom data.</param>
    private void OnGetToneAnalyze(ToneAnalyzerResponse resp, Dictionary<string, object> customData)
    {
        // extract response values from JSON response
        // watson Log.Debug; systemName, format, args
        Log.Debug("WatsonServiceConnection.OnGetToneAnalyze()", "{0}", customData["json"].ToString());

        //ResultsField.text = (customData["json"].ToString());  // works but long and cannot read
        //AnalyticsText.text = (customData["json"].ToString());  // works but long and cannot read

        // I dont particularly like the direct use of arrays and indicies
        // these Magic Numbers might change if IBM shuffles the responses
        // would be better to use lookup, but that may add a fair bit of overhead?
        // direct indexing is from R.Andersons's example git
        // the ToneAnalyzerResponse is an object with collection sof Document_tone 
        // and Sentence_tone
        // Document_tone is collection of ToneCategory Arrays,
        // Sentence_tone is a bit more involved and not used in the examples, so unused here.
        // Tone Categories are Id+Name + array of Tone
        // Tone's are score, tone_id and tone_name.
        // These look like classics for refactoring into SO Variables
        // 
        // Log Analysis Repsonse, read consol to be sure our index# match up to name
        // Emotional Tones
        Log.Debug("$$$$$ TONE Emotional ANGER", " {0} = {1}",
                  resp.document_tone.tone_categories[0].tones[0].tone_name,
                  resp.document_tone.tone_categories[0].tones[0].score
                 ); // ANGER resp.document_tone.tone_categories [0].tones [0].score);
        Log.Debug("$$$$$ TONE Emotional DISGUST", " {0} = {1}",
                  resp.document_tone.tone_categories[0].tones[1].tone_name,
                  resp.document_tone.tone_categories[0].tones[1].score); // DISGUST
        Log.Debug("$$$$$ TONE Emotional FEAR", " {0} = {1}",
                  resp.document_tone.tone_categories[0].tones[2].tone_name,
                  resp.document_tone.tone_categories[0].tones[2].score); // FEAR
        Log.Debug("$$$$$ TONE Emotional JOY", " {0} = {1}",
                  resp.document_tone.tone_categories[0].tones[3].tone_name,
                  resp.document_tone.tone_categories[0].tones[3].score); // JOY
        Log.Debug("$$$$$ TONE Emotional SAD", " {0} = {1}",
                  resp.document_tone.tone_categories[0].tones[4].tone_name,
                  resp.document_tone.tone_categories[0].tones[4].score); // SADNESS
                                                                         // language tones
        Log.Debug("$$$$$ TONE ANALYTICAL", " {0} = {1}",
                  resp.document_tone.tone_categories[1].tones[0].tone_name,
                  resp.document_tone.tone_categories[1].tones[0].score); // ANALYTICAL
        Log.Debug("$$$$$ TONE CONFIDENT", " {0} = {1}",
                  resp.document_tone.tone_categories[1].tones[1].tone_name,
                  resp.document_tone.tone_categories[1].tones[1].score); //  CONFIDENT
        Log.Debug("$$$$$ TONE TENTATIVE", " {0} = {1}",
                  resp.document_tone.tone_categories[1].tones[2].tone_name,
                  resp.document_tone.tone_categories[1].tones[2].score); //  TENTATIVE
                                                                         // category 2 = social tones
                                                                         // Consientious,  Extraversion Agreeable, EmotionalRange
                                                                         // not sure what other categories are implemented or planned or deprecated

        // Note that the original ExampleStreaming4RobotEmotion uses if/else
        // so only one value would be updated per pass.
        // here we are checking all values and setting the SO
        // if no EmotionThreshold assigned, then report all value updates
        // should either [Require ] or test for TA_ variables

        updateSOValue(resp, TA_Anger,   0, 0);
        updateSOValue(resp, TA_Disgust, 0, 1);
        updateSOValue(resp, TA_Fear,    0, 2);
        updateSOValue(resp, TA_Joy,     0, 3);
        updateSOValue(resp, TA_Sadness, 0, 4);

        // skip social tone for now
    }

    private void updateSOValue(ToneAnalyzerResponse resp, FloatVariable fv, int categoryId, int toneId)
    {
        if (fv == null) return; // no variable to update
        if (TA_EmotionThreshold == null || 
            resp.document_tone.tone_categories[categoryId].tones[toneId].score > TA_EmotionThreshold.Value)
            fv.Value = (float)resp.document_tone.tone_categories[categoryId].tones[toneId].score;
    }
}
