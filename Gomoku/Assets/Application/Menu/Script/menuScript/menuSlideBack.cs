using UnityEngine;
using System.Collections;

public class menuSlideBack : MonoBehaviour {

    public float x;
    public float decal;
    bool sliderEnd = false;

    // Use this for initialization
    void Start () {
	
	}

    public void menuSliderBack()
    {
        sliderEnd = true;
    }
	
	// Update is called once per frame
	void Update () {
        if (sliderEnd)
        {
            if ((transform.position.x <= x + decal) && (transform.position.x >= x - decal))
            {
                sliderEnd = false;
            }
            else
            {
                if (transform.position.x > x)
                {
                    transform.position = new Vector3(transform.position.x - decal, transform.position.y, transform.position.x);
                }
                else
                {
                    transform.position = new Vector3(transform.position.x + decal, transform.position.y, transform.position.x);
                }
            }
        }
    }
}
