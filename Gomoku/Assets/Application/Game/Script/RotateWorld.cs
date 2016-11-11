using UnityEngine;
using System.Collections;

public class RotateWorld : MonoBehaviour {

    // Use this for initialization
    public bool iscam = false;
    public GameObject world;
    public float speed = 1.0f;
    private float timer = 4.0f;
    public float timeToRotate = 4.0f;
	void Start () {
        timer = timeToRotate;
	}
	
	// Update is called once per frame
	void Update () {
        if (iscam)
        {
            if (timer > 0)
                transform.RotateAround(world.transform.localPosition, Vector3.up, Time.deltaTime * speed);
            else
            {
                transform.RotateAround(world.transform.localPosition, -Vector3.up, Time.deltaTime * speed);
                if (timer < -timeToRotate)
                    timer = timeToRotate;
            }
            timer -= Time.deltaTime;
        }
        else
            transform.RotateAround(world.transform.localPosition, Vector3.up, Time.deltaTime *speed);
    }
}
