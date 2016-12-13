using UnityEngine;
using System.Collections;

public class ConnectionPanelScript : MonoBehaviour {

    // Use this for initialization

    void Start () {

    }

    public void IaOn()
    {
        PlayerPrefs.SetInt("IA", 1);
    }
    public void IaOff()
    {
        PlayerPrefs.SetInt("IA", 0);
    }
    // Update is called once per frame
    void Update () {
	    
    }
}
