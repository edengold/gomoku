using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class Rules : MonoBehaviour
{
    #region DllImport
    [DllImport("GomokuDll", CharSet = CharSet.Unicode)]
    static extern int Add(int a, int b);

    [DllImport("GomokuDll", CharSet = CharSet.Unicode)]
    static extern IntPtr CreateGomokuAPI();

    [DllImport("GomokuDll", CharSet = CharSet.Unicode)]
    static extern void DeleteGomokuAPI(IntPtr api);

    [DllImport("GomokuDll", CharSet = CharSet.Unicode)]
    static extern bool GetTurn(IntPtr api);

    [DllImport("GomokuDll", CharSet = CharSet.Unicode)]
    static extern bool SetTurn(IntPtr api);

    [DllImport("GomokuDll", CharSet = CharSet.Unicode)]
    static extern bool CanIPutHere(IntPtr api, int pos);

    [DllImport("GomokuDll", CharSet = CharSet.Unicode)]
    static extern int GetDeletedPion(IntPtr api);

    [DllImport("GomokuDll", CharSet = CharSet.Unicode)]
    static extern bool GetVictoryTeam(IntPtr api);

    [DllImport("GomokuDll", CharSet = CharSet.Unicode)]
    static extern bool GetVictory(IntPtr api);

    [DllImport("GomokuDll", CharSet = CharSet.Unicode)]
    static extern bool Opt3Rule(IntPtr api);

    [DllImport("GomokuDll", CharSet = CharSet.Unicode)]
    static extern bool OptBreakRule(IntPtr api);

    [DllImport("GomokuDll", CharSet = CharSet.Unicode)]
    static extern void ChangeMap(IntPtr api, int x, int y, int color);
    #endregion

    private IntPtr _gomokuAPI;
    public int Player;
    public GomokuBoardManager Board;
    public int NbBlackPrise;
    public int NbBWhitePrise;
    public int NbTurs = 0;
    public bool _isVictory = false;
    public GameObject VicScreen;

    void Start()
    {
        _gomokuAPI = CreateGomokuAPI();
        if (PlayerPrefs.GetInt("breakrule") == 0)
            OptBreakRule(_gomokuAPI);
        if (PlayerPrefs.GetInt("treerule") == 0)
            Opt3Rule(_gomokuAPI);

    }

    #region FuncFromDll

    public void GetTurn()
    {
        if (GetTurn(_gomokuAPI))
            Player = 0;
        else
        {
            Player = 1;
        }
    }

    public bool GetVictory()
    {
        return GetVictory(_gomokuAPI);
    }
    public int GetVictoryTeam()
    {
        if (GetVictoryTeam(_gomokuAPI))
            return 0;
        return 1;
    }
    public int GetDeletedPion()
    {
        int tmp;
        if ((tmp = GetDeletedPion(_gomokuAPI)) != -1)
        {
            return tmp;
            if (tmp >= 0 && tmp < 20*20)
            {
                Debug.Log(tmp);
                Board.DeletePion(tmp);
            }
        }
        return -1;
    }
    public bool CanIPutHere(int pos)
    {
        return CanIPutHere(_gomokuAPI, pos);
    }

    public void Reverse()
    {
         NbTurs--;
         Board.ReturnToTmp();
        foreach (var val in Board.PionList)
        {
            GomokuPion pion = val.GetComponent<GomokuPion>();
            int x = (int) (pion.id%20);
            int y = (int) (pion.id/20);
            ChangeMap(_gomokuAPI, x, y, pion._player);
        }
         SetTurn(_gomokuAPI);
    }
    #endregion

    void Update()
    {
        GetTurn();
        if (GetVictory(_gomokuAPI))
        {
            _isVictory = true;
            if (GetVictoryTeam(_gomokuAPI))
                Debug.Log("VictoryTeam = blanc");
            else
            {
                Debug.Log("VictoryTeam = noir");
            }
        }
        if (NbBWhitePrise >= 10 || NbBlackPrise >= 10)
            _isVictory= true;
        if (_isVictory && !VicScreen.activeSelf)
        {
            VicScreen.SetActive(true);
        }

    }

    public void OnDestroy()
    {
        Debug.Log("GomokuAPI Destroy");
        DeleteGomokuAPI(_gomokuAPI);
    }
}
