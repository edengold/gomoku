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
    static extern bool CanIPutHere(IntPtr api, int pos);

    [DllImport("GomokuDll", CharSet = CharSet.Unicode)]
    static extern int GetDeletedPion(IntPtr api);

    [DllImport("GomokuDll", CharSet = CharSet.Unicode)]
    static extern int GetNbWhitePrise(IntPtr api);

    [DllImport("GomokuDll", CharSet = CharSet.Unicode)]
    static extern int GetNbBlackPrise(IntPtr api);

    [DllImport("GomokuDll", CharSet = CharSet.Unicode)]
    static extern bool GetVictoryTeam(IntPtr api);

    [DllImport("GomokuDll", CharSet = CharSet.Unicode)]
    static extern bool GetVictory(IntPtr api);
    #endregion

    private IntPtr _gomokuAPI;
    public int Player;
    public GomokuBoardManager Board;
    public int NbBlackPrise;
    public int NbBWhitePrise;

    void Start()
    {
        _gomokuAPI = CreateGomokuAPI();
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
    public void GetNbBlackPrise()
    {
        NbBlackPrise = GetNbBlackPrise(_gomokuAPI);
    }
    public void GetNbWhitePrise()
    {
        NbBWhitePrise = GetNbWhitePrise(_gomokuAPI);
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
    #endregion

    void Update()
    {
        GetTurn();
        GetNbBlackPrise(_gomokuAPI);
        GetNbWhitePrise(_gomokuAPI);
        if (GetVictory(_gomokuAPI))
        {
            if (GetVictoryTeam(_gomokuAPI))
                Debug.Log("VictoryTeam = blanc");
            else
            {
                Debug.Log("VictoryTeam = noir");

            }
        }
    }

    public void OnDestroy()
    {
        Debug.Log("GomokuAPI Destroy");
        DeleteGomokuAPI(_gomokuAPI);
    }
}
