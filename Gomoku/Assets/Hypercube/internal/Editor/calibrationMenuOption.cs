using UnityEngine;
using System.Collections;
using UnityEditor;

namespace hypercube
{
    public class calibrationMenuOption : MonoBehaviour
    {
#if HYPERCUBE_DEV
        [MenuItem("Hypercube/Save Settings", false, 51)]
        public static void saveCubeSettings()
        {
            hypercube.castMesh c = GameObject.FindObjectOfType<hypercube.castMesh>();
            if (c)
                c.saveConfigSettings();
            else
                Debug.LogWarning("No castMesh was found, and therefore no saving could occur.");
        }
#endif

        [MenuItem("Hypercube/Load Settings", false, 52)]
        public static void loadHardwareCalibrationSettings()
        {
            hypercube.castMesh c = GameObject.FindObjectOfType<hypercube.castMesh>();
            if (c)
            {
                if (c.loadSettings()) //to prevent spamming, this does not provide feedback when settings are loaded
                    Debug.Log("Hypercube settings loaded.");
            }
            else
                Debug.LogWarning("No castMesh was found, and therefore no loading occurred.");
        }

#if HYPERCUBE_DEV
        [MenuItem("Hypercube/Copy current slice calibration", false, 300)]  //# is prio
        public static void openCubeWindowPrefs()
        {
            calibrator c = GameObject.FindObjectOfType<calibrator>();

            if (c)
                c.copyCurrentSliceCalibration();
            else
                Debug.LogWarning("No calibrator was found, and therefore no copying occurred.");
        }
#endif

        [MenuItem("Hypercube/Load Volume friendly Unity Prefs", false, 600)]
        public static void setVolumeFriendlyPrefs()
        {

            Debug.Log("Removing skybox...");
            Debug.Log("Removing ambient light...");
            //turn off ambient stuff, they can cause lighting anomalies of not specifically set or handled
            RenderSettings.skybox = null; 
            RenderSettings.ambientLight = Color.black;

            Debug.Log("Ensuring editor set to 3D mode...");
            EditorSettings.defaultBehaviorMode = EditorBehaviorMode.Mode3D;

            Debug.Log("Ensuring Scene views set to 3D mode...");
            foreach (SceneView s in SceneView.sceneViews)
            {
                s.in2DMode = false;
            }

            Debug.Log("Setting compatibility level to .Net 2.0 (necessary for receiving input from Volume)...");
            Debug.Log("Setting HYPERCUBE_INPUT preprocessor macro (necessary for receiving input from Volume)...");

            //these 2 lines below MUST stay together!!  If the preprocessor is added without changing the ApiCompatibilityLevel, then a weird error will appear where the editor won't know what IO.ports is
            //this will be very tough for novice programmers to figure out what is going on.
            PlayerSettings.apiCompatibilityLevel = ApiCompatibilityLevel.NET_2_0;
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, "HYPERCUBE_INPUT"); //add HYPERCUBE_INPUT to prerprocessor defines   

            Debug.Log("Setting our standalone build target...");

#if UNITY_EDITOR_WIN
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.StandaloneWindows)
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.StandaloneWindows);
#elif UNITY_EDITOR_OSX
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.StandaloneOSXUniversal)
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.StandaloneOSXUniversal);
#endif
        }


        [MenuItem("Hypercube/About Hypercube", false, 601)]
        public static void aboutHypercube()
        {
            Debug.Log("Hypercube: Volume Plugin  -  Version: " + hypercubeCamera.version + "  -  by Looking Glass Factory, Inc.  Visit lookingglassfactory.com to learn more!");
        }
    }
}
