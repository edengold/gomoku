using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine.UI;

public class Rules : MonoBehaviour
{
    #region DllImport
#if UNITY_STANDALONE_WIN

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
    static extern bool Opt3Rule(IntPtr api, bool col);

    [DllImport("GomokuDll", CharSet = CharSet.Unicode)]
    static extern int OptBreakRule(IntPtr api, bool col);

    [DllImport("GomokuDll", CharSet = CharSet.Unicode)]
    static extern void ChangeMap(IntPtr api, int x, int y, int color);

    [DllImport("GomokuDll", CharSet = CharSet.Unicode)]
    static extern int GetError(IntPtr api);

    [DllImport("GomokuDll", CharSet = CharSet.Unicode)]
    static extern IntPtr CreateIAGomoku();

    [DllImport("GomokuDll", CharSet = CharSet.Unicode)]
    static extern void DeleteIAGomoku(IntPtr ia);

    [DllImport("GomokuDll", CharSet = CharSet.Unicode)]
    static extern void SetIa(IntPtr ia, IntPtr api);

    [DllImport("GomokuDll", CharSet = CharSet.Unicode)]
    static extern void RunIa(IntPtr ia, IntPtr api, int color, int pos);

    [DllImport("GomokuDll", CharSet = CharSet.Unicode)]
    static extern int GetPos(IntPtr api);

    [DllImport("GomokuDll", CharSet = CharSet.Unicode)]
    static extern int GetTime(IntPtr api);
#endif
#if UNITY_STANDALONE_LINUX

    [DllImport("GomokuAPI", CharSet = CharSet.Unicode)]
    static extern int Add(int a, int b);

    [DllImport("GomokuAPI", CharSet = CharSet.Unicode)]
    static extern IntPtr CreateGomokuAPI();

    [DllImport("GomokuAPI", CharSet = CharSet.Unicode)]
    static extern void DeleteGomokuAPI(IntPtr api);

    [DllImport("GomokuAPI", CharSet = CharSet.Unicode)]
    static extern bool GetTurn(IntPtr api);

    [DllImport("GomokuAPI", CharSet = CharSet.Unicode)]
    static extern bool SetTurn(IntPtr api);

    [DllImport("GomokuAPI", CharSet = CharSet.Unicode)]
    static extern bool CanIPutHere(IntPtr api, int pos);

    [DllImport("GomokuAPI", CharSet = CharSet.Unicode)]
    static extern int GetDeletedPion(IntPtr api);

    [DllImport("GomokuAPI", CharSet = CharSet.Unicode)]
    static extern bool GetVictoryTeam(IntPtr api);

    [DllImport("GomokuAPI", CharSet = CharSet.Unicode)]
    static extern bool GetVictory(IntPtr api);

    [DllImport("GomokuAPI", CharSet = CharSet.Unicode)]
    static extern bool Opt3Rule(IntPtr api, bool col);

    [DllImport("GomokuAPI", CharSet = CharSet.Unicode)]
    static extern int OptBreakRule(IntPtr api, bool col);

    [DllImport("GomokuAPI", CharSet = CharSet.Unicode)]
    static extern void ChangeMap(IntPtr api, int x, int y, int color);

    [DllImport("GomokuAPI", CharSet = CharSet.Unicode)]
    static extern int GetError(IntPtr api);

    [DllImport("GomokuAPI", CharSet = CharSet.Unicode)]
    static extern IntPtr CreateIAGomoku();

    [DllImport("GomokuAPI", CharSet = CharSet.Unicode)]
    static extern void DeleteIAGomoku(IntPtr ia);

    [DllImport("GomokuAPI", CharSet = CharSet.Unicode)]
    static extern void SetIa(IntPtr ia, IntPtr api);

    [DllImport("GomokuAPI", CharSet = CharSet.Unicode)]
    static extern void RunIa(IntPtr ia, IntPtr api, int color, int pos);

    [DllImport("GomokuAPI", CharSet = CharSet.Unicode)]
    static extern int GetPos(IntPtr api);

    [DllImport("GomokuAPI", CharSet = CharSet.Unicode)]
    static extern int GetTime(IntPtr api);
#endif
    #endregion

    private IntPtr _gomokuAPI;
    private IntPtr _gomokuIA;
    public int Player;
    public bool ia = true;
    public bool iaTurn = false;
    public GomokuBoardManager Board;
    public int NbBlackPrise;
    public int NbBWhitePrise;
    public int NbBlackPriseTmp;
    public int NbBWhitePriseTMp;
    public int NbTurs = 0;
    public bool _isVictory = false;
    public GameObject VicScreen;
    public bool _isReversed = false;
    public Text VictoriTest;
    public GameObject Timer;
    public Text TimerText;

    void Awake()
    {
        ia = false;
        if (PlayerPrefs.GetInt("IA") == 1)
        {
            Timer.SetActive(true);
            ia = true;
        }
        _gomokuAPI = CreateGomokuAPI();
        _gomokuIA = CreateIAGomoku();
    }

    void Start()
    {
        if (PlayerPrefs.GetInt("breakrule") == 0)
            OptBreakRule(_gomokuAPI, false);
        else
            OptBreakRule(_gomokuAPI, true);
        if (PlayerPrefs.GetInt("treerule") == 0)
            Opt3Rule(_gomokuAPI, false);
        else
            Opt3Rule(_gomokuAPI, true);
    }

    #region FuncFromDll

    public void chBreak(bool val)
    {
        int tmp;
        if ((tmp = OptBreakRule(_gomokuAPI, val)) != -1)
        {
            if (tmp == 0)
            {
                _isVictory = true;
                VictoriTest.text = "WHITE WIN";
            }
            else
            {
                VictoriTest.text = "BLACK WIN";
                VictoriTest.color = Color.black;
                _isVictory = true;
            }
        }
    }

    public void IaPlay(int color, int pos)
    {
        SetIa(_gomokuIA, _gomokuAPI);
        RunIa(_gomokuIA, _gomokuAPI, color, pos);
    }

    public void ch3(bool val)
    {
        Opt3Rule(_gomokuAPI, val);
    }

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
        if (NbTurs > 0 && !_isReversed)
        {
            NbTurs--;
            _isReversed = true;
            Board.ReturnToTmp();
            foreach (var val in Board.PionList)
            {
                GomokuPion pion = val.GetComponent<GomokuPion>();
                int x = (int) (pion.id%20);
                int y = (int) (pion.id/20);
                ChangeMap(_gomokuAPI, x, y, pion._player);
            }
            SetTurn(_gomokuAPI);
            NbBWhitePrise = NbBWhitePriseTMp;
            NbBlackPrise = NbBlackPriseTmp;
        }
    }

    #endregion

    void Update()
    {
        GetTurn();
        if (ia)
        {
            int pos;
            if ((pos = GetPos(_gomokuIA)) != -1)
            {
                TimerText.text = GetTime(_gomokuIA) + " ms";
                Board.CreatePion(pos, 1);
            }
        }
        Debug.Log("ICI LERROR => " + GetError(_gomokuAPI));
        if (GetVictory(_gomokuAPI))
        {
            _isVictory = true;
            if (GetVictoryTeam(_gomokuAPI))
                VictoriTest.text = "WHITE WIN";
            else
            {
                VictoriTest.text = "BLACK WIN";
                VictoriTest.color = Color.black;
            }
        }
        if (NbBWhitePrise >= 10 || NbBlackPrise >= 10)
        {
            if (NbBWhitePrise >= 10)
                VictoriTest.text = "WHITE WIN";
            else
            {
                VictoriTest.text = "BLACK WIN";
                VictoriTest.color = Color.black;
            }
            _isVictory = true;
        }
        if (_isVictory && !VicScreen.activeSelf)
        {
            VicScreen.SetActive(true);
        }
    }

    public void OnDestroy()
    {
        Debug.Log("GomokuAPI Destroy");
        DeleteGomokuAPI(_gomokuAPI);
        DeleteIAGomoku(_gomokuIA);
    }
}