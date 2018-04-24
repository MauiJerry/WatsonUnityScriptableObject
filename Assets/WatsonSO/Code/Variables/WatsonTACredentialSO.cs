using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoboRyanTron.Unite2017.Variables;

[CreateAssetMenu]
public class WatsonTACredentialSO: ScriptableObject 
{
    [SerializeField]
    private string _username = "NotAssigned" ;
    [SerializeField]
    private string _password = "NONE" ;
    [SerializeField]
    private string _url = "https://gateway.watsonplatform.net/tone-analyzer/api";

    public string Username
    {
        get { return _username; }
        set { _username = value; }
    }
    public string Password
    {
        get { return _password; }
        set { _password = value; }
    }
    public string URL
    {
        get { return _url; }
        set { _url = value; }
    }

}
