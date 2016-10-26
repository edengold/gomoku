using UnityEngine;
using System.Collections;

public class inputTest : MonoBehaviour 
{
    public Transform[] fingerItems;

    public hypercubeCamera cam;

    public Transform someThing;

	void Update () 
    {
        if (hypercube.input.frontScreen == null) //Volume not connected via USB
            return;  

        for (uint i = 0; i < fingerItems.Length; i++)
        {
            if (i < hypercube.input.frontScreen.touches.Length)
                fingerItems[i].position = hypercube.input.frontScreen.touches[i].getWorldPos(cam);
            else
                fingerItems[i].position = new Vector3(50f, 50f, 50f); //not used atm, just put them aside.
        }

        someThing.transform.Translate(hypercube.input.frontScreen.averageDiff.x, hypercube.input.frontScreen.averageDiff.y, 0f, Space.World);

        someThing.Rotate(0f, hypercube.input.frontScreen.twist, 0f); 

        someThing.localScale *= hypercube.input.frontScreen.pinch;
        
	}
}
