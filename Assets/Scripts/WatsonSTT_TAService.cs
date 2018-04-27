/// <summary>
/// WatsonSTT_TAService: Watson SpeechToText and ToneAnalysis Service Connector
/// built as MonoBehavior but data passed in and out as Scriptable Objects
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

public class WatsonSTT_TAService : MonoBehaviour
{

    [SerializeField]
    WatsonCredentialsSO stt_credentialSO;
    [SerializeField]
    WatsonTACredentialSO ta_credentialSO;

    // Text returned by Watson Speech-Text
    [SerializeField]
    StringVariable recognizedText;

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

    // fields from ExampleService to deal with Watson SDK
    // to record speech
    private int _recordingRoutine = 0;
    private string _microphoneID = null;
    private AudioClip _recording = null;
    private int _recordingBufferSize = 1;
    private int _recordingHZ = 22050;

    // links to Watson tools
    private SpeechToText _speechToText;
    private ToneAnalyzer _toneAnalyzer;
    private string _toneAnalyzerVersionDate = "2017-05-26";
    //-----------------
    // Public property to start/pause/restart Service
    // caveat: not clear if pause/restart works - untested
    public bool Active
    {
        get { return _speechToText.IsListening; }
        set
        {
            if (value && !_speechToText.IsListening)
            {
                _speechToText.DetectSilence = true;
                _speechToText.EnableWordConfidence = true;
                _speechToText.EnableTimestamps = true;
                _speechToText.SilenceThreshold = 0.01f;
                _speechToText.MaxAlternatives = 0;
                _speechToText.EnableInterimResults = true;
                _speechToText.OnError = OnError;
                _speechToText.InactivityTimeout = -1;
                _speechToText.ProfanityFilter = false;
                _speechToText.SmartFormatting = true;
                _speechToText.SpeakerLabels = false;
                _speechToText.WordAlternativesThreshold = null;
                _speechToText.StartListening(OnRecognize, OnRecognizeSpeaker);
            }
            else if (!value && _speechToText.IsListening)
            {
                _speechToText.StopListening();
            }
        }
    }

    /// <summary>
    /// Awake this instance.
    ///   use Awake to copy values to the Watson data structure
    /// </summary>
	void Awake()
    {
        print("Speech2Text user: " + stt_credentialSO.Username);
        print("Speech2Text  pwd: " + stt_credentialSO.Password);
        print("Speech2Text  url: " + stt_credentialSO.URL);

        print("ToneAnalysis user: " + ta_credentialSO.Username);
        print("ToneAnalysis  pwd: " + ta_credentialSO.Password);
        print("ToneAnalysis  url: " + ta_credentialSO.URL);

        credentials_STT = new Credentials(
            stt_credentialSO.Username,
            stt_credentialSO.Password,
            stt_credentialSO.URL);

        credentials_TONE = new Credentials(
            ta_credentialSO.Username,
            ta_credentialSO.Password,
            ta_credentialSO.URL);

    }

    //------------------
    /// <summary>
    /// Start() method connects to the two Watson Serices and starts the recording
    /// real work is then done by a Runnable function
    /// </summary>
    void Start()
    {
        // Watson logging, interface to Speech to Text and Tone Analyzer Servises
        LogSystem.InstallDefaultReactors();

        //  Create credential and instantiate service
        _speechToText = new SpeechToText(credentials_STT);
        Active = true;

        StartRecording();

        // TONE ZONE
        _toneAnalyzer = new ToneAnalyzer(credentials_TONE);
        _toneAnalyzer.VersionDate = _toneAnalyzerVersionDate;

    }

    /// <summary>
    /// Starts the Speech-To-Text recording by starting a Runnable instance
    /// </summary>
    private void StartRecording()
    {
        if (_recordingRoutine == 0)
        {
            UnityObjectUtil.StartDestroyQueue();
            _recordingRoutine = Runnable.Run(RecordingHandler());
        }
    }

    /// <summary>
    /// Stops the recording by turning off microphone, stopping the Runnable
    /// </summary>
    private void StopRecording()
    {
        if (_recordingRoutine != 0)
        {
            Microphone.End(_microphoneID);
            Runnable.Stop(_recordingRoutine);
            _recordingRoutine = 0;
        }
    }

    /// <summary>
    /// invoked when an error happens, sets to inactive, logs erro
    /// </summary>
    /// <param name="error">Error.</param>
    private void OnError(string error)
    {
        Active = false;

        Log.Debug("WatsonServiceConnection.OnError()", "Error! {0}", error);
    }

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
    /// Primary Callback for Runnable - starts Microphone, captures segments, 
    ///     passes to Watson Service.  Details are IBM code
    /// </summary>
    /// <returns>The handler.</returns>
    private IEnumerator RecordingHandler()
    {
        Log.Debug("WatsonServiceConnection.RecordingHandler()", "devices: {0}", Microphone.devices);
        _recording = Microphone.Start(_microphoneID, true, _recordingBufferSize, _recordingHZ);
        yield return null;      // let _recordingRoutine get set..

        if (_recording == null)
        {
            StopRecording();
            yield break;
        }

        bool bFirstBlock = true;
        int midPoint = _recording.samples / 2;
        float[] samples = null;

        while (_recordingRoutine != 0 && _recording != null)
        {
            int writePos = Microphone.GetPosition(_microphoneID);
            if (writePos > _recording.samples || !Microphone.IsRecording(_microphoneID))
            {
                Log.Error("WatsonServiceConnection.RecordingHandler()", "Microphone disconnected.");

                StopRecording();
                yield break;
            }

            if ((bFirstBlock && writePos >= midPoint)
                || (!bFirstBlock && writePos < midPoint))
            {
                // front block is recorded, make a RecordClip and pass it onto our callback.
                samples = new float[midPoint];
                _recording.GetData(samples, bFirstBlock ? 0 : midPoint);

                AudioData record = new AudioData();
                record.MaxLevel = Mathf.Max(Mathf.Abs(Mathf.Min(samples)), Mathf.Max(samples));
                record.Clip = AudioClip.Create("Recording", midPoint, _recording.channels, _recordingHZ, false);
                record.Clip.SetData(samples, 0);

                // Send the audio to Watson for conversion
                _speechToText.OnListen(record);

                bFirstBlock = !bFirstBlock;
            }
            else
            {
                // calculate the number of samples remaining until we ready for a block of audio, 
                // and wait that amount of time it will take to record.
                int remaining = bFirstBlock ? (midPoint - writePos) : (_recording.samples - writePos);
                float timeRemaining = (float)remaining / (float)_recordingHZ;

                yield return new WaitForSeconds(timeRemaining);
            }
        }

        yield break;
    }

    // For Speech Recognition -- commands to UI/Experience
    /// <summary>
    /// Invoked by Watson SpeechToText to handle response
    /// </summary>
    /// <param name="result">Result.</param>
    private void OnRecognize(SpeechRecognitionEvent result)
    {
        if (result != null && result.results.Length > 0)
        {
            foreach (var res in result.results)
            {
                // Commands
                // at present we dont recognize any Commands, so this section is commented ot
                // and decimated the code, so if you want it back read Ryan's Example 4 Robot
                // How many alt usually come back? is updating a single SO correct 
                //    or should there be a collection/queue?
                foreach (var alt in res.alternatives)
                {
                    string text = string.Format("{0} ({1}, {2:0.00})\n", alt.transcript, res.final ? "Final" : "Interim", alt.confidence);
                    Log.Debug("WatsonServiceConnection.OnRecognize()", text);

                    // UPDATE THE ScriptableObject
                    // watchers need to refresh in their Update()
                    // or we need to implement EventVariables similar to Hipple GameEvent
                    // VariableEvent?
                    recognizedText.Value = text;

                    // SEND TO TONE ANALYSIS
                    // This might? be done as a watcher on recognizedText?
                    string GHI = alt.transcript;
                    if (!_toneAnalyzer.GetToneAnalyze(OnGetToneAnalyze, OnFail, GHI))
                        Log.Debug("WatsonServiceConnection.Examples()", "Failed to analyze!");

                    // Command Parsing
                    // This could be done better as a separate Watcher on recognizedText
                    if (alt.transcript.Contains("reset"))
                    {
                        //ResetAction();
                    }

                }


                // Log Keywords - should be done as a Watcher on recognizedText
                if (res.keywords_result != null && res.keywords_result.keyword != null)
                {
                    foreach (var keyword in res.keywords_result.keyword)
                    {
                        Log.Debug("WatsonServiceConnection.OnRecognize()", "keyword: {0}, confidence: {1}, start time: {2}, end time: {3}", keyword.normalized_text, keyword.confidence, keyword.start_time, keyword.end_time);
                        //ResultsField.text = "tone analyzed! 222";
                    }
                }

                // Log Alternative Words
                if (res.word_alternatives != null)
                {
                    foreach (var wordAlternative in res.word_alternatives)
                    {
                        Log.Debug("WatsonServiceConnection.OnRecognize()", "Word alternatives found. Start time: {0} | EndTime: {1}", wordAlternative.start_time, wordAlternative.end_time);
                        foreach (var alternative in wordAlternative.alternatives)
                            Log.Debug("WatsonServiceConnection.OnRecognize()", "\t word: {0} | confidence: {1}", alternative.word, alternative.confidence);
                    }
                }
            }
        }
    }

    private void OnRecognizeSpeaker(SpeakerRecognitionEvent result)
    {
        if (result != null)
        {
            foreach (SpeakerLabelsResult labelResult in result.speaker_labels)
            {
                Log.Debug("WatsonServiceConnection.OnRecognize()", string.Format("speaker result: {0} | confidence: {3} | from: {1} | to: {2}", labelResult.speaker, labelResult.from, labelResult.to, labelResult.confidence));
            }
        }
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
        /* before updateSOValue...
        if (TA_EmotionThreshold == null || resp.document_tone.tone_categories[0].tones[1].score > TA_EmotionThreshold.Value)
            TA_Disgust.Value = (float)resp.document_tone.tone_categories[0].tones[1].score;

        if (TA_EmotionThreshold == null || resp.document_tone.tone_categories[0].tones[2].score > TA_EmotionThreshold.Value)
            TA_Fear.Value = (float)resp.document_tone.tone_categories[0].tones[2].score;

        if (TA_EmotionThreshold == null || resp.document_tone.tone_categories[0].tones[3].score > TA_EmotionThreshold.Value)
            TA_Joy.Value = (float)resp.document_tone.tone_categories[0].tones[3].score;

        if (TA_EmotionThreshold == null || resp.document_tone.tone_categories[0].tones[4].score > TA_EmotionThreshold.Value)
            TA_Sadness.Value = (float)resp.document_tone.tone_categories[0].tones[4].score;
        */

        // Language tone - https://www.ibm.com/watson/developercloud/tone-analyzer/api/v3/
        /*        if (TA_EmotionThreshold == null || resp.document_tone.tone_categories[1].tones[0].score > TA_EmotionThreshold.Value)
					TA_Analytical.Value = (float)resp.document_tone.tone_categories[1].tones[0].score;

				if (TA_EmotionThreshold == null || resp.document_tone.tone_categories[1].tones[1].score > TA_EmotionThreshold.Value)
					TA_Confident.Value = (float)resp.document_tone.tone_categories[1].tones[0].score;

				if (TA_EmotionThreshold == null || resp.document_tone.tone_categories[1].tones[2].score > TA_EmotionThreshold.Value)
					TA_Tenatative.Value = (float)resp.document_tone.tone_categories[1].tones[2].score;
		*/
        // skip social tone for now
    }

    private void updateSOValue(ToneAnalyzerResponse resp, FloatVariable fv, int categoryId, int toneId)
    {
        if (fv == null) return; // no variable to update
        if (TA_EmotionThreshold == null || resp.document_tone.tone_categories[categoryId].tones[toneId].score > TA_EmotionThreshold.Value)
            fv.Value = (float)resp.document_tone.tone_categories[categoryId].tones[toneId].score;
    }
}
