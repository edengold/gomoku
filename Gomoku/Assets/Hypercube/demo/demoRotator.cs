using UnityEngine;
using System.Collections;

public class demoRotator : MonoBehaviour {

    public float pauseTime = 4f;
    float paused = -1f;

    public GameObject rotated;
    public float rotationSpeed;
    public float scaleSpeed;
    public float scaleMod;
    public float verticalSwingSpeed;
    public float verticalSwing;   

    Vector3 startScale;
    Vector3 startRot;
    Vector3 currentRot;
    float rotationTime;
    void Start()
    {
        startScale = rotated.transform.localScale;
        startRot = rotated.transform.localRotation.eulerAngles;
        rotationTime = 0;
    }
	
	// Update is called once per frame
	void Update () 
    {

        if ( 
            Input.GetKey(KeyCode.W) ||
            Input.GetKey(KeyCode.A) ||
            Input.GetKey(KeyCode.S) ||
            Input.GetKey(KeyCode.D) ||
            Input.GetKey(KeyCode.E) ||
            Input.GetKey(KeyCode.Q) ||
            Input.GetKey(KeyCode.R) ||
            Input.GetKey(KeyCode.LeftArrow) ||
            Input.GetKey(KeyCode.RightArrow) ||
            Input.GetKey(KeyCode.UpArrow) ||
            Input.GetKey(KeyCode.DownArrow) ||
            Input.GetKey(KeyCode.Tab) ||
            Input.GetKey(KeyCode.LeftControl) ||
            Input.GetKey(KeyCode.LeftShift) ||
            Input.GetAxis("Mouse X") != 0 ||
            Input.GetAxis("Mouse Y") != 0
            )
        {
            paused = pauseTime;
            return;
        }

        if (paused > 0)
        {
            paused -= Time.deltaTime;
            if (paused > 0)
                return;
        }

        rotationTime += Time.deltaTime;

        //auto rotation
        currentRot = startRot;
        currentRot.y += rotationSpeed * rotationTime;
        currentRot.x += Mathf.Sin(rotationTime * verticalSwingSpeed) * verticalSwing;
        rotated.transform.localRotation = Quaternion.Euler(currentRot);

        //scale
        Vector3 temp = startScale;
        float mod = Mathf.Sin(rotationTime * scaleSpeed) * scaleMod;
        mod += .5f;  //the mod is based around the original scale, not 0.
        if (mod < .01f)
            mod = .01f;
        temp.x += mod;
        temp.y += mod;
        temp.z += mod;
        rotated.transform.localScale = temp;      
	}

    public void reset()
    {
        rotated.transform.localRotation = Quaternion.Euler(startRot);
        rotated.transform.localScale = startScale;
        rotationTime = 0;
    }


    public void quitApp()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
         Application.Quit();
#endif
    }

}
