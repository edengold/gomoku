using UnityEngine;
using System.Collections;

public class RotateCube : MonoBehaviour {

    // Use this for initialization
    [SerializeField]
    private float speed = 0.1f;
    public GameObject pivot;
      void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetMouseButton(1))
        {
            float rotX = Input.GetAxis("Mouse X") * speed * Mathf.Deg2Rad;
            float rotY = Input.GetAxis("Mouse Y") * speed * Mathf.Deg2Rad;
            transform.RotateAround(pivot.transform.position, Vector3.up, -rotX);
            transform.RotateAround(pivot.transform.position, Vector3.right, rotY);
        }
        }
}
