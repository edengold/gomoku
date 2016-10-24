using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BoardManager : MonoBehaviour
{
    #region declaration
    public Rules rules;


    [SerializeField]
    private GameObject prefab;
    [SerializeField]
    private GameObject faceFront;
    [SerializeField]
    private GameObject faceBack;
    [SerializeField]
    private GameObject faceTop;
    [SerializeField]
    private GameObject faceBot;
    [SerializeField]
    private GameObject faceLeft;
    [SerializeField]
    private GameObject faceRight;
    [SerializeField]
    private GameObject Cube;
    [SerializeField]
    private GameObject point;

    [SerializeField]
    private int x = 19;
    [SerializeField]
    private int z = 19;
    #endregion
    void Start()
    {
        CreateWorld();
    }
    public void CreateWorld()
    {
        Debug.Log("World Create");
        int multiplicateur = (int)prefab.transform.localScale.x;
        x *= multiplicateur;
        z *= multiplicateur;
        for (int i = 0; i < 6; i++)
        {
            CreateFace(multiplicateur, x, z, i);
        }
 
        CreatePoint(faceFront.transform, new Vector3(0,0,0), Rules.FaceSide.FRONT);
        CreatePoint(faceBack.transform, new Vector3(0, 0, 0), Rules.FaceSide.BACK);
        CreatePoint(faceTop.transform, new Vector3(90, 0, 0), Rules.FaceSide.TOP);
        CreatePoint(faceBot.transform, new Vector3(90, 0, 0), Rules.FaceSide.BOT);
        CreatePoint(faceLeft.transform, new Vector3(0, 90, 0), Rules.FaceSide.LEFT);
        CreatePoint(faceRight.transform, new Vector3(0, 90, 0), Rules.FaceSide.RIGHT);
        Cube.transform.localPosition = new Vector3(426.4887f, 1.9501f, -88.93186f);
        Cube.transform.Rotate(new Vector3(-7.169f, 11.506f, 0.571f));

    }
    private void CreateFace(int multiplicateur, int x, int y, int face)
    {
        for (int i = 0; i < x; i += multiplicateur)
        {
            for (int j = 0; j < z; j += multiplicateur)
            {
                GameObject tmp = (GameObject)GameObject.Instantiate(prefab, new Vector3(i, 0, j), Quaternion.identity);
                tmp.transform.Rotate(new Vector3(90, 0, 0));
                switch (face)
                {
                    case 0:
                        tmp.transform.parent = faceFront.transform;
                        break;
                    case 1:
                        tmp.transform.parent = faceBack.transform;
                        break;
                    case 2:
                        tmp.transform.parent = faceTop.transform;
                        break;
                    case 3:
                        tmp.transform.parent = faceBot.transform;
                        break;
                    case 4:
                        tmp.transform.parent = faceLeft.transform;
                        break;
                    case 5:
                        tmp.transform.parent = faceRight.transform;
                        break;
                }
            }
        }
        switch (face)
        {
            case 0:
                faceFront.transform.Rotate(new Vector3(0, 90, -90));
                faceFront.transform.localPosition = new Vector3(-457.5f, -439.5f, 0);
                break;
            case 1:
                faceBack.transform.Rotate(new Vector3(0, -90, -90));
                faceBack.transform.localPosition = new Vector3(-439.5f, -439.5f, 11);
                break;
            case 2:
                faceTop.transform.Rotate(new Vector3(0, 90, 0));
                faceTop.transform.localPosition = new Vector3(-457.5f, 14.5f, -443f);
                break;
            case 3:
                faceBot.transform.Rotate(new Vector3(-180, -90, 0));
                faceBot.transform.localPosition = new Vector3(-457.5f, 3.5f, 454);
                break;
            case 4:
                faceLeft.transform.Rotate(new Vector3(-180, 0, 90));
                faceLeft.transform.localPosition = new Vector3(-454f, -439.5f, 14.5f);
                break;
            case 5:
                faceRight.transform.Rotate(new Vector3(0, 0, -90));
                faceRight.transform.localPosition = new Vector3(-443, -439.5f, -3.5f);
                break;
        }
    }
    private void CreatePoint(Transform face, Vector3 rot, Rules.FaceSide faceSide)
    {
        int i = 1;
        int j = 1;
        List<GameObject> piontmp = new List<GameObject>();
        foreach (Transform child in face)
        {
            GameObject tmp = (GameObject)GameObject.Instantiate(point, child.transform.position, Quaternion.identity);
            tmp.transform.parent = child.transform;
            tmp.transform.localPosition = new Vector3(-0.5f, -0.5f, 0);
            tmp.transform.Rotate(rot);
            rules.PionTab[faceSide].Add(tmp);
            if (j == 19)
            {
                GameObject tmp3 = (GameObject)GameObject.Instantiate(point, child.transform.position, Quaternion.identity);
                tmp3.transform.parent = child.transform;
                tmp3.transform.localPosition = new Vector3(0.5f, -0.5f, 0);
                tmp3.transform.Rotate(rot);
                piontmp.Add(tmp3);

            }
            if (i == x)
            {
                GameObject tmp2 = (GameObject)GameObject.Instantiate(point, child.transform.position, Quaternion.identity);
                tmp2.transform.parent = child.transform;
                tmp2.transform.localPosition = new Vector3(-0.5f, 0.5f, 0);
                tmp2.transform.Rotate(rot);
                rules.PionTab[faceSide].Add(tmp2);
                if (j == 19)
                {
                    GameObject tmp4 = (GameObject)GameObject.Instantiate(point, child.transform.position, Quaternion.identity);
                    tmp4.transform.parent = child.transform;
                    tmp4.transform.localPosition = new Vector3(0.5f, 0.5f, 0);
                    tmp4.transform.Rotate(rot);
                    piontmp.Add(tmp4);

                }
                i = 0;
                j++;
            }
            i++;

        }
        foreach (GameObject pion in piontmp)
        {
            rules.PionTab[faceSide].Add(pion);
        }
    }
    void Update()
    {

    }

}