using UnityEngine;
using System.Collections;

public class hypercubeAnimLauncher : MonoBehaviour {

    public Animator anim;
    public float freq = 3f;
    public float minFreq = .3f;

    float timer;
	

	void Update () 
    {
        timer -= Time.deltaTime;
        if (timer < 0)
        {
            anim.SetTrigger("playIt");
            timer = Random.Range(minFreq, freq + minFreq);
        }
	}
}
