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

public class WatsonSTT_TAService : MonoBehaviour 
{

    [SerializeField]
    WatsonCredentialsSO stt_credentialSO;
    [SerializeField]
    WatsonTACredentialSO ta_credentialSO;

    private Credentials credentials_STT;
    private Credentials credentials_TA;

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
        
        credentials_TA = new Credentials(
            ta_credentialSO.Username,
            ta_credentialSO.Password,
            ta_credentialSO.URL);

	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
