using UnityEngine;
using System.Collections;

public class calibrationRotator : MonoBehaviour {

	public float xSpeed = .01f;
	public float ySpeed = .01f;
	public float zSpeed = .01f;
	
	// Update is called once per frame
	void Update () 
	{
		transform.Rotate (xSpeed * Time.deltaTime, ySpeed * Time.deltaTime, zSpeed * Time.deltaTime);
	}
}
