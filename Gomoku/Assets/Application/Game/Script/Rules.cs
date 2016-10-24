using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Rules : MonoBehaviour {

    // Use this for initialization
    public enum FaceSide {
        FRONT = 0,
        BACK = 1,
        TOP = 2,
        BOT = 3,
        LEFT = 4,
        RIGHT = 5
    };
    public Dictionary<FaceSide, List<GameObject>> PionTab = new Dictionary<FaceSide, List<GameObject>>();

    void Awake()
    {
        PionTab.Add(FaceSide.FRONT, new List<GameObject>());
        PionTab.Add(FaceSide.BACK, new List<GameObject>());
        PionTab.Add(FaceSide.TOP, new List<GameObject>());
        PionTab.Add(FaceSide.BOT, new List<GameObject>());
        PionTab.Add(FaceSide.LEFT, new List<GameObject>());
        PionTab.Add(FaceSide.RIGHT, new List<GameObject>());
    }
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
