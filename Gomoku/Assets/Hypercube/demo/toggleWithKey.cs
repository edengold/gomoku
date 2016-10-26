using UnityEngine;
using System.Collections;

public class toggleWithKey : MonoBehaviour 
{

    public KeyCode key;
    public GameObject toggle;
    public GameObject toggleUp;

	// Update is called once per frame
	void Update () 
    {

        if (toggle && Input.GetKeyDown(key))
        {
            if (toggle.activeSelf)
                toggle.SetActive(false);
            else
                toggle.SetActive(true);
        }

        if (toggleUp && Input.GetKeyUp(key))
        {
            if (toggleUp.activeSelf)
                toggleUp.SetActive(false);
            else
                toggleUp.SetActive(true);
        }
	
	}
}
