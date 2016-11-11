using UnityEngine;
using System.Collections;

public class GomokuPion : MonoBehaviour
{

    private bool _isOnBoard = false;
    private int _player = 1;
    public int id;
    public Rules _Rules;
    public GameObject notHere;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    public void SetPionOnBoard(bool val)
    {
        _isOnBoard = val;
    }
    public void InvokePion(int player)
    {
        if (!_isOnBoard)
        {
            //Debug.Log("x = " + (int)(id % 20) + " y = " + (int)(id / 20));
            GetComponent<MeshRenderer>().enabled = true;
            if (player == 1)
                GetComponent<Renderer>().material.SetColor("_Color", Color.black);
            else
                GetComponent<Renderer>().material.SetColor("_Color", Color.white);
            _isOnBoard = true;
        }
    }
    public void KillPion()
    {
        if (_isOnBoard)
        {
            GetComponent<MeshRenderer>().enabled = false;
            GetComponent<Renderer>().material.SetColor("_Color", Color.white);
            _isOnBoard = false;
        }
    }
    void OnMouseOver()
    {
        if (!_isOnBoard)
        {
            GetComponent<MeshRenderer>().enabled = true;
            if (_Rules.Player == 0)
                GetComponent<Renderer>().material.SetColor("_Color", Color.white);
            else
                GetComponent<Renderer>().material.SetColor("_Color", Color.black);
            if (Input.GetMouseButtonDown(0))
            {
                if (_Rules.CanIPutHere(id))
                    InvokePion(_Rules.Player);
                else
                {
                    notHere.SetActive(true);
                }
            }
        }
    }
    void OnMouseExit()
    {
        if (!_isOnBoard)
        {
            GetComponent<MeshRenderer>().enabled = false;
            GetComponent<Renderer>().material.SetColor("_Color", Color.white);
        }
    }
}
