using UnityEngine;
using UnityEditor;
using System.Collections;

//manages a full screen display of the render texture of the Volume

//due to differences in the way the windows function on different OS
//on OSX the caster must be launched with the mouse over the desired display, so there is no 'toggle window' menuItem (there is only a 'close window')  and the ⌘e hotkey to toggle it.

//on Windows 
//Ctrl + E can toggle the window, and also you have a toggle option in the menu.

namespace hypercube
{
    public class casterWindow : EditorWindow
    {
        Camera canvasCam;
        castMesh canvas;
        RenderTexture renderTexture;

        static casterWindow caster;

#if UNITY_STANDALONE_OSX 
	[MenuItem("Hypercube/Toggle Hotkey: _%e", false, 10)] //emphasize the hotkey in osx, since that is the only way to get it to function properly
#else
        [MenuItem("Hypercube/Toggle Caster Window _%e", false, 10)] //see  https://docs.unity3d.com/ScriptReference/MenuItem.html)
#endif
        public static void toggleWindow()
        {
            if (caster)
                closeWindow();
            else
                openWindow();
        }

        public static void openWindow()
        {
            //allow only 1 window
            closeWindow();

            int posX = EditorPrefs.GetInt("V_windowOffsetX", 0);
            int posY = EditorPrefs.GetInt("V_windowOffsetY", 0);
            int w = EditorPrefs.GetInt("V_windowWidth", Display.main.renderingWidth);
            int h = EditorPrefs.GetInt("V_windowHeight", Display.main.renderingHeight);

            //create a new window
            caster = ScriptableObject.CreateInstance<casterWindow>();
            caster.position = new Rect(posX, posY, w, h);
            caster.autoRepaintOnSceneChange = true;  //this lets it update any changes.  see also: http://docs.unity3d.com/ScriptReference/EditorWindow-autoRepaintOnSceneChange.html
            caster.ShowPopup();

            //give a warning if Playmode tint color is not white.
            string tintVal = EditorPrefs.GetString("Playmode tint", "");
            if (tintVal != "Playmode tint;1;1;1;1")
#if UNITY_EDITOR_WIN
                Debug.LogWarning("In some versions of Unity, caster window may appear incorrectly dark during PLAY mode.\nThis can be fixed by setting \"Playmode tint\" to white in: Edit > Preferences > Colors");
#elif UNITY_EDITOR_OSX
                Debug.LogWarning("In some versions of Unity, caster window may appear incorrectly dark during PLAY mode.\nThis can be fixed by setting \"Playmode tint\" to white in: Unity > Preferences > Colors");
#endif

        }



        public static void closeWindow()
        {
            if (caster != null)
                caster.Close();


            //it is possible to have stray windows if the user messes with the monitor setup in the OS while the caster window is open
            casterWindow strayWindow = EditorWindow.GetWindow<casterWindow>();
            if (strayWindow != null && strayWindow != caster) //remember the caster is not really destroyed here... editorWindow does not destroyImmediate
                strayWindow.Close();

            //stop deforming the output view
            hypercube.castMesh canvas = GameObject.FindObjectOfType<hypercube.castMesh>();
            if (canvas)
                canvas.usingCustomDimensions = false;

            caster = null;
        }



        public void Awake()
        {

            if (EditorApplication.isPlaying) 

            //close the game window, if it's up.
            //EditorWindow[] allWindows = Resources.FindObjectsOfTypeAll(typeof(EditorWindow)) as EditorWindow[];
            //foreach (EditorWindow w in allWindows)
            //{
            //    if (w.GetType().ToString() == "UnityEditor.GameView")
            //        w.Close();
            //}

            //force things to reset. set it all up in update so that it will be dynamic
            canvas = null;
            canvasCam = null;

            ensureTextureIntegrity();
        }

        public void Update()
        {
            if (canvas == null || canvasCam == null)
            {
                canvas = GameObject.FindObjectOfType<hypercube.castMesh>();
                if (canvas)
                {
                    canvasCam = canvas.GetComponent<Camera>();
                    canvas.setCustomWidthHeight(position.width, position.height);
                }
                else
                {
                    Debug.LogWarning("No Hypercube Canvas found, closing window.");
                    closeWindow();
                    return;
                }
            }

            ensureTextureIntegrity(); //this call is for during Editor
        }

        void OnGUI()
        {
            if (canvasCam != null && Application.isEditor) //calling this in OnGUI instead of update allows manual renaming changes during edit mode, but not during play mode
            {
                ensureTextureIntegrity();//this call is for during Play

                canvasCam.targetTexture = renderTexture;
                canvasCam.Render();
                canvasCam.targetTexture = null;


                GUI.contentColor = Color.white; //ensure this isn't being messed with, or our caster will appear dark.
                GUI.color = Color.white;
                EditorGUI.DrawPreviewTexture(new Rect(0.0f, 0.0f, position.width, position.height), renderTexture, canvas.casterMaterial, ScaleMode.StretchToFill);
            }
        }


        void ensureTextureIntegrity()
        {

            if (!renderTexture || renderTexture.width != position.width || renderTexture.height != position.height)
                renderTexture = new RenderTexture((int)position.width, (int)position.height, 0, RenderTextureFormat.ARGB32);

            //let the hypercube know that it is not using the gameWindow for rendering, and to rely only on the given settings.
            if (canvas)
                canvas.setCustomWidthHeight(position.width, position.height); //try to set it to proper dims
        }

    }

}
