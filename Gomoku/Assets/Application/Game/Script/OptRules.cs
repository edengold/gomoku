using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class OptRules : MonoBehaviour
{
    public Rules _Rules;
    public Toggle ToggleBreak;
    public Toggle Toggle3;

    private bool togleBreak = false;
    private bool togle3 = false;

    // Use this for initialization
    void Start ()
    {
        if (PlayerPrefs.GetInt("breakrule") == 0)
        {
            togleBreak = false;
            ToggleBreak.isOn = false;
        }
        else
        {
            ToggleBreak.isOn = true;
        }
        if (PlayerPrefs.GetInt("treerule") == 0)
        {
            togle3 = false;
            Toggle3.isOn = false;
        }
        else
        {
            Toggle3.isOn = true;
        }
    }

    public void ChangeBreak()
    {
        togleBreak = !togleBreak;
        Debug.Log("break = " + togleBreak);
        _Rules.chBreak(togleBreak);
    }


    public void Change3rule()
    {
        togle3 = !togle3;
        Debug.Log("3rule = " + togleBreak);
        _Rules.ch3(togle3);
    }
    // Update is called once per frame
    void Update () {
	
	}
}
