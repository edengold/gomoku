using UnityEngine;
using System.Collections;
using JetBrains.Annotations;
using UnityEngine.UI;

public class Stats : MonoBehaviour
{
    public Rules _Rules;
    public Text NbW;
    public Text NbB;
    public Text nbTurns;
    public GameObject OpRules;
	// Use this for initialization
	void Start ()
	{
//	    if (OptionelRules._instance == null)

	}
	
	// Update is called once per frame
	void Update ()
	{
	    NbB.text = "X " + _Rules.NbBlackPrise;
	    NbW.text = "X " + _Rules.NbBWhitePrise;
	    nbTurns.text = "" + _Rules.NbTurs;
	}
}
