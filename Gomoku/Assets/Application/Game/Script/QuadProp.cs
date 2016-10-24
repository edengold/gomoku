using UnityEngine;
using System.Collections;

public class QuadProp : MonoBehaviour {

    public GameObject Herb1;
	// Use this for initialization
	void Start () {
        float rdX = Random.Range(-0.8f, 0.8f);
        float rdZ = Random.Range(-0.8f, 0.8f);
        int herbRd = Random.Range(0, 2);
        GameObject tmp;
        switch (herbRd)
        {
            case 0:
                tmp = (GameObject)GameObject.Instantiate(Herb1, new Vector3(transform.position.x + rdX, Herb1.transform.position.y, transform.position.z + rdZ), Quaternion.identity);
                tmp.transform.parent = transform;
                break;
        }
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
