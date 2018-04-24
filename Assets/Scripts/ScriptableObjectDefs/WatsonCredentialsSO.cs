using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoboRyanTron.Unite2017.Variables;

[CreateAssetMenu]
public class WatsonCredentialsSO : ScriptableObject 
{
    [SerializeField]
    private StringVariable _username  ;
    [SerializeField]
    private StringVariable _password;
    [SerializeField]
    private StringVariable _url ;

    public string Username
    {
        get { return _username.Value; }
        set { _username.Value = value; }
    }
    public string Password
    {
        get { return _password.Value; }
        set { _password.Value = value; }
    }
    public string URL
    {
        get { return _url.Value; }
        set { _url.Value = value; }
    }

}
