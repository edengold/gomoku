using UnityEngine;
using System.Collections;

public class ConnectionPanelScript : MonoBehaviour {

    // Use this for initialization
    private bool moveCenter = false;
    private bool moveRight = false;
    private bool moveLeft = false;
    public bool startCenter = false;
    public float speed = 0.3f;
    void Start () {
        if (!startCenter)
            transform.localPosition = new Vector3(-Screen.width, 0, 0);
        else
            transform.localPosition = new Vector3(0, 0, 0);
    }

    public void Tocenter()
    {
        moveCenter = true;
    }
    public void ToRight()
    {
        moveRight = true;
    }
    public void ToLeft()
    {
        moveLeft = true;
    }
    // Update is called once per frame
    void Update () {
	    if (moveCenter)
        {
            if (transform.localPosition.x >= -speed && transform.localPosition.x <= speed)
                moveCenter = false;
            if (transform.localPosition.x < speed)
            {
                transform.localPosition = new Vector3(transform.localPosition.x + speed, 0, 0);
            }
            else if (transform.localPosition.x > speed)
            {
                transform.localPosition = new Vector3(transform.localPosition.x - speed, 0, 0);
            }
        }
        if (moveRight)
        {
            if (transform.localPosition.x < Screen.width)
                transform.localPosition = new Vector3(transform.localPosition.x + speed, 0, 0);
            else
                moveRight = false;
        }
        if (moveLeft)
        {
            if (transform.localPosition.x > -Screen.width)
                transform.localPosition = new Vector3(transform.localPosition.x - speed, 0, 0);
            else
                moveLeft = false;
        }
    }
}
