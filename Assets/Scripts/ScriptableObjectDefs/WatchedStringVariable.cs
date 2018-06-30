// ----------------------------------------------------------------------------
// WatchedStringVariable - a ScriptableObject holding a String with Watchers
//    Watchers are notified by a callback whenever string is Set
//    a mashup of Ryan Hipple's StringVariable and GameEventListener
//
// see Ryan's Unite 2017 - Game Architecture with Scriptable Objects
// 
// ----------------------------------------------------------------------------
using System;
using UnityEngine;

[CreateAssetMenu]
public class WatchedStringVariable : ScriptableObject
{
    [SerializeField]
    private string value = "";

    public event Action OnChanged = delegate { };

    public string Value
    {
        get { return value; }
        set { 
            this.value = value;
            OnChanged();
        }
    }
}
