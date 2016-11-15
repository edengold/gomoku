using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GomokuPion : MonoBehaviour
{

    public bool _isOnBoard = false;
    public int _player = -1;
    public int id;
    public Rules _Rules;
    public GomokuBoardManager Board;
    public GameObject notHere;
    void Start()
    {
        _player = -1;
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
            _player = player;
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
            _player = -1;
            GetComponent<MeshRenderer>().enabled = false;
            GetComponent<Renderer>().material.SetColor("_Color", Color.white);
            _isOnBoard = false;
        }
    }
    void OnMouseOver()
    {
        if (!_isOnBoard && !_Rules._isVictory)
        {
            bool one = false;
            GetComponent<MeshRenderer>().enabled = true;
            if (_Rules.Player == 0)
                GetComponent<Renderer>().material.SetColor("_Color", Color.white);
            else
                GetComponent<Renderer>().material.SetColor("_Color", Color.black);
            if (Input.GetMouseButtonDown(0))
            {
                if (_Rules.CanIPutHere(id))
                {
                    if (!one)
                    {
                        _Rules._isReversed = false;
                        Board.PionListTmp.Clear();
                        foreach (var val in Board.PionList)
                        {
                            if (val.GetComponent<GomokuPion>()._isOnBoard)
                                Board.PionListTmp.Add(val.GetComponent<GomokuPion>()._player);
                            else
                                Board.PionListTmp.Add(-1);
                        }
                        _Rules.NbTurs++;
                        one = true;
                    }
                    int tmp;
                    while ((tmp = _Rules.GetDeletedPion()) != -1)
                    {
                            Debug.Log(tmp);
                           _Rules.Board.DeletePion(tmp);
                            if (_Rules.Player == 0)
                                _Rules.NbBWhitePrise++;
                            else
                            {
                                _Rules.NbBlackPrise++;
                            }
                    }
                    InvokePion(_Rules.Player);
                }
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
