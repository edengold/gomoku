using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GomokuBoardManager : MonoBehaviour {

    [SerializeField]
    private GameObject pionPrefab;
    [SerializeField]
    private GameObject parent;
    public List<GameObject> PionList = new List<GameObject>();
    // Use this for initialization
    void Start () {
        CreateBoard();


    }
    public void CreateBoard()
    {
        int id = 0;
        for (float i = 0.5f; i >= -0.5f; i -= 0.0525f)
        {
            for (float j = -0.5f; j <= 0.5f; j += 0.0525f)
            {
                GameObject tmp;
                tmp = (GameObject)GameObject.Instantiate(pionPrefab, new Vector3(0, 0, 0), Quaternion.Euler(0,0,0));
                tmp.transform.parent = parent.transform;
                tmp.transform.localPosition = new Vector3(j, i, -0.6f);
                tmp.transform.localScale = pionPrefab.transform.localScale;
                tmp.transform.localRotation = Quaternion.Euler(90, 0, 0);
                PionList.Add(tmp);
                tmp.GetComponent<GomokuPion>().id = id++;
            }
        }
    }

    public void CreatePion(int x, int y,int player)
    {
        PionList[(x * 20) + y].GetComponent<GomokuPion>().InvokePion(player);
    }
    public void CreatePion(int pos, int player)
    {
        PionList[pos].GetComponent<GomokuPion>().InvokePion(player);
    }
    public void DeletePion(int x, int y)
    {
        PionList[(x * 20) + y].GetComponent<GomokuPion>().KillPion();
    }
    public void DeletePion(int pos)
    {
        PionList[pos].GetComponent<GomokuPion>().KillPion();
    }
    // Update is called once per frame
    void Update () {

	}
}
