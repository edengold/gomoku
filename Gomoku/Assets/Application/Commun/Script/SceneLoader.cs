using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
public class SceneLoader : MonoBehaviour {
    public void LoadLevel(string lvl)
    {
        SceneManager.LoadScene(lvl);
    }  

    public void exit()
    {
        Application.Quit();
    }
}
