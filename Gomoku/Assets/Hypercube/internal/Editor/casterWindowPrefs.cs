using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class casterWindowPrefs : EditorWindow
{

    int posX = EditorPrefs.GetInt("V_windowOffsetX", 0);
    int posY = EditorPrefs.GetInt("V_windowOffsetY", 0);
    int width = EditorPrefs.GetInt("V_windowWidth", 1920);  //Display.main.renderingWidth
    int height = EditorPrefs.GetInt("V_windowHeight", 1080);


    [MenuItem("Hypercube/Caster Window Prefs", false, 1)]  //1 is prio
    public static void openCubeWindowPrefs()
    {
        EditorWindow.GetWindow(typeof(casterWindowPrefs), false, "Caster Prefs");
    }



    void OnGUI()
    {

        GUILayout.Label("Caster Window Prefs", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Use this tool to align a Volume Caster Window to the Volume display.\n\n" +
        	
		#if UNITY_STANDALONE_OSX 
            "TO OPEN THE WINDOW:\nmouse over Volume's display, then ⌘E", MessageType.Info);
#else
            "Toggle the Caster window with Ctrl+E", MessageType.Info);
		#endif


        posX = EditorGUILayout.IntField("X Position:", posX);
        posY = EditorGUILayout.IntField("Y Position:", posY);
        width = EditorGUILayout.IntField("Width:", width);
        height = EditorGUILayout.IntField("Height:", height);


        if (GUILayout.Button("Move Right +" + Screen.currentResolution.width))
            posX += Screen.currentResolution.width;


        if (GUILayout.Button("Set to current: " + Screen.currentResolution.width + " x " + Screen.currentResolution.height))
        {
            posX = 0;
            posY = 0;
            width = Screen.currentResolution.width;
            height = Screen.currentResolution.height;
        }


        GUILayout.FlexibleSpace();



#if UNITY_EDITOR_WIN
		EditorGUILayout.HelpBox("TIPS:\nUnity prefers if the cube monitor is left of the main monitor (don't ask me why). \n\nIf any changes are made to the monitor setup, Unity must be off or restarted for this tool to work properly.", MessageType.Info);

#endif

        //if (GUILayout.Button("- SAVE -"))
        if (GUI.changed)
        {
            EditorPrefs.SetInt("V_windowOffsetX", posX);
            EditorPrefs.SetInt("V_windowOffsetY", posY);
            EditorPrefs.SetInt("V_windowWidth", width);
            EditorPrefs.SetInt("V_windowHeight", height);

         //   hypercube.casterWindow.closeWindow();
        }
    }


}
