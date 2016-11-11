using UnityEngine;
using System.Collections;
using JetBrains.Annotations;
using UnityEngine.UI;

public class Stats : MonoBehaviour
{
    public Rules _Rules;
    public Text NbW;
    public Text NbB;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update ()
	{
	    NbB.text = "X " + _Rules.NbBlackPrise;
	    NbW.text = "X " + _Rules.NbBWhitePrise;
    }
}
