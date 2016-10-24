using UnityEngine;
using System.Collections;
using System;

public class Pion : MonoBehaviour {

    // Use this for initialization
    private bool isOnBoard = false;
    public int id;
    public Rules rules;
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
    public void SetPionOnBoard(bool val)
    {
        isOnBoard = val;
    }
    public void InvkePion()
    {
        if (!isOnBoard)
        {
            GetComponent<MeshRenderer>().enabled = true;
            isOnBoard = true;
        }
    }
    void OnMouseOver()
    {
        int index = 0;
        if (!isOnBoard)
        {
            GetComponent<MeshRenderer>().enabled = true;
            if (Input.GetMouseButtonDown(0))
            {
                foreach (Rules.FaceSide faceEnum in Enum.GetValues(typeof(Rules.FaceSide)))
                {
                    if (rules.PionTab[faceEnum].IndexOf(gameObject) != -1)
                    {
                        index = rules.PionTab[faceEnum].IndexOf(gameObject);
                    }
                }
                foreach (Rules.FaceSide faceEnum in Enum.GetValues(typeof(Rules.FaceSide)))
                {
                    rules.PionTab[faceEnum][index].GetComponent<Pion>().InvkePion();
                }
            }
        }
    }
    void OnMouseExit()
    {
        if (!isOnBoard)
        {
            GetComponent<MeshRenderer>().enabled = false;
        }
    }
}
