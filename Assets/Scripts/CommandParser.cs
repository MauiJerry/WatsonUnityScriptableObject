/* CommandParser - parses a text string and dispatches actions
 *  real simple at present to demonstrate use with WatchedStringVariable
 * 
 */
using System;
using UnityEngine;

public class CommandParser : MonoBehaviour {
    // Text to be analyized - from speech to text perhaps
    [SerializeField]
    WatchedStringVariable watchedTextToParse;

	// Use this for initialization
	void Awake () 
    {
        if (watchedTextToParse != null) 
            watchedTextToParse.OnChanged += ParseWatchedText;
	}
    private void OnDestroy()
    {
        if (watchedTextToParse != null) 
            watchedTextToParse.OnChanged -= ParseWatchedText;
    }

    void ParseWatchedText()
    {
        if (watchedTextToParse != null) 
            ParseText(watchedTextToParse.Value);
    }

    void ParseText(string text)
    {
        // we arent really doing anything yet
        if (text.Contains("reset"))
        {
            print("Command 'reset' recognized, and ignored");
        }
    }
}
