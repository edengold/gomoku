using UnityEngine;
using System.Collections;

public class NotHere : MonoBehaviour {

	// Use this for initialization
    public float Timer;
    private float time;
	void Start ()
	{
	    time = Timer;
	}
	
	// Update is called once per frame
	void Update () {
	    if (time <= 0.0f)
	    {
	        time = Timer;
	        gameObject.SetActive(false);
	    }
	    time -= Time.deltaTime;
	}
}
