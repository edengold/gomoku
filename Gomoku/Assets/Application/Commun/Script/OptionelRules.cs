using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class OptionelRules : Singleton<MonoBehaviour>
{

    private bool togleBreak = false;
    private bool togle3 = false;
    // Use this for initialization
    void Start()
    {
        PlayerPrefs.SetInt("breakrule", 0);
        PlayerPrefs.SetInt("treerule", 0);
        //   DontDestroyOnLoad(transform.gameObject);
    }

    public void changeBreak()
    {
        togleBreak = !togleBreak;
        if (togleBreak)
            PlayerPrefs.SetInt("breakrule", 1);
        else
        {
            PlayerPrefs.SetInt("breakrule", 0);
        }
    }

    public void changeTree()
    {
        togle3 = !togle3;
        if (togle3)
            PlayerPrefs.SetInt("treerule", 1);
        else
        {
            PlayerPrefs.SetInt("treerule", 0);
        }

    }

    public bool getBreak()
    {
        return togleBreak;
    }
    public bool getTree()
    {
        return togle3;
    }
    // Update is called once per frame
    void Update()
    {

    }
}
