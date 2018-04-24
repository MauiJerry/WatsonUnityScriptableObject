/// <summary>
/// WatsonSTT_TAService: Watson SpeechToText and ToneAnalysis Service Connector
/// built as MonoBehavior but data passed in and out as Scriptable Objects
/// based on the various ExampleService.cs in SDK and RustyOldDrake GitHub
///   Note the IBM code uses its own Logging system, not Unity's Debug
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
    [SerializeField]
    ToneAnalysis_ResponseSO toneAnalysisResponse;

    // Tone Analysis Reponses
    [SerializeField]
    private FloatVariable TA_Joy;
    [SerializeField]
    private FloatVariable TA_Sadness;
    [SerializeField]
    private FloatVariable TA_Disgust;
    [SerializeField]
    private FloatVariable TA_Anger;

    [SerializeField]
    private FloatVariable TA_Analytical;
    [SerializeField]
    private FloatVariable TA_Confident;
    [SerializeField]
    private FloatVariable TA_Tenatative;

    [SerializeField]
    private FloatVariable TA_Consientious;
    [SerializeField]
    private FloatVariable TA_Extraversion;
    [SerializeField]
    private FloatVariable TA_Agreeable;
    [SerializeField]
    private FloatVariable TA_EmotionalRange;

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
	void Awake () 
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
                //and decimated the code, so if you want it back read Ryan's Example 4 Robot
                foreach (var alt in res.alternatives)
                {
                    string text = string.Format("{0} ({1}, {2:0.00})\n", alt.transcript, res.final ? "Final" : "Interim", alt.confidence);
                    Log.Debug("WatsonServiceConnection.OnRecognize()", text);

                    // UPDATE THE ScriptableObject
                    // watchers need to refresh in their Update()
                    // or we need to implement EventVariables
                    // as Hipple mentions
                    recognizedText.Value = text;

                    // SEND TO TONE ANALYSIS
                    // This might? be done as a watcher on recognizedText?
                    string GHI = alt.transcript;
                    if (!_toneAnalyzer.GetToneAnalyze(OnGetToneAnalyze, OnFail, GHI))
                        Log.Debug("WatsonServiceConnection.Examples()", "Failed to analyze!");

                    // Command Parsing
                    // This should be done as a Watcher on recognizedText
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
    }

}
