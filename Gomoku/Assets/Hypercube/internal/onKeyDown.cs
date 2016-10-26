using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class onKeyDown : MonoBehaviour {

    public KeyCode key;
    public UnityEvent eventTriggered;
	
	// Update is called once per frame
	void Update () 
    {
        if ( Input.GetKeyDown(key))
            eventTriggered.Invoke();
	}
}
