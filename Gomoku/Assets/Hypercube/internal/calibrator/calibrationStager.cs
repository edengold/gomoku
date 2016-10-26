using UnityEngine;
using System.Collections;

namespace hypercube
{
    [System.Serializable]
    public class calibrationStage
    {
        public string name;  //not functional, just for inspector convenience
		public Texture infoTexture;
        public GameObject[] activeObjects;
       
    }

    public class calibrationStager : MonoBehaviour
    {
        protected int stage = 0;

        public castMesh canvas;
		public UnityEngine.UI.InputField[] fullTextInputs; //these are used to prevent space bar from triggering help if we are actually typing something.
		public UnityEngine.UI.Image helpImage;
        public calibrationStage[] stages;


        
       // public GameObject infoScreen;

        float checkUSBtimer = 0f;

        void Start()
        {
            stage = 0;
            resetStage();
        }

        void Update()
        {
            //don't progress without the usb...
            if (stage == 0)
            {
                if (canvas.foundConfigFile)
                    nextStage();
                else
                {
                    checkUSBtimer += Time.deltaTime;
                    if (checkUSBtimer > 2f) //check every 2 seconds
                    {
                        canvas.loadSettings();
                        checkUSBtimer = 0;
                    }
                }
            }

            //normal path...

            if (Input.GetKeyDown(KeyCode.RightBracket))
                nextStage();
            else if (Input.GetKeyDown(KeyCode.LeftBracket))
                prevStage();

			if (Input.GetKeyDown (KeyCode.Space)) //toggle help
			{
				foreach (UnityEngine.UI.InputField f in fullTextInputs) //don't allow help to toggle if an input is focused.
				{
					if (f.isFocused)
						return;
				}
				helpImage.gameObject.SetActive(!helpImage.gameObject.activeSelf);
			}

            if (Input.GetKeyDown(KeyCode.Escape)) 
            {
                quit();
                return;
            }
        }

        public void nextStage()
        {
            stage++;

            if (stage >= stages.Length)
                stage = 0;

            resetStage();
        }
        public void prevStage()
        {
            stage--;

            if (stage < 0)
                stage = 0;  //don't loop

            resetStage();
        }
        void resetStage()
        {
            //disable all.
            foreach (calibrationStage s in stages)
            {
                foreach (GameObject o in s.activeObjects)
                {
                    o.SetActive(false);
                }
            }

            //enable the appropriate things.
            foreach (GameObject o in stages[stage].activeObjects)
            {
                o.SetActive(true);
            }

			helpImage.material.mainTexture = stages[stage].infoTexture;
			if (helpImage.gameObject.activeSelf) //if it is active, toggle it to update it.
			{
				helpImage.gameObject.SetActive (false);
				helpImage.gameObject.SetActive (true);
			}

        }

        void quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

    }

}


