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
    public GameObject notTurn;
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
                if (_Rules.ia && _Rules.Player != 0)
                {
                    notTurn.SetActive(true);
                    return;
                }
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
                    int b = 0;
                    int w = 0;
                    while ((tmp = _Rules.GetDeletedPion()) > 0 && tmp < 19*19)
                    {
                            Debug.Log(tmp);
                           _Rules.Board.DeletePion(tmp);
                        if (_Rules.Player == 0)
                        {
                            w++;
                            _Rules.NbBWhitePrise++;
                        }
                            else
                        {
                            b++;
                                _Rules.NbBlackPrise++;
                            }
                    }
                    _Rules.NbBWhitePriseTMp = _Rules.NbBWhitePrise - w;
                    _Rules.NbBlackPriseTmp = _Rules.NbBlackPrise - b;
                    InvokePion(_Rules.Player);
                 if (_Rules.ia)
                    _Rules.IaPlay(1, id);
                }
                else
                    notHere.SetActive(true);
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
