using UnityEngine;
using System.Collections;
using System.Collections.Generic;


  
    [ExecuteInEditMode]
    public class hypercubeCamera : MonoBehaviour
    {
         public const float version = 1.12f;

        public enum softSliceMode
        {
            HARD = 0,
            SOFT,
            SOFT_CUSTOM
        }
        [Tooltip("HARD = no slice blending (also no blackPoint modification)\nSOFT = autocalculate softness based on the overlap\nSOFT_CUSTOM = manage your own overlap and softness")]
        public softSliceMode slicing;
        [Tooltip("The percentage of overdraw a slice will include of its neighbor slices.\n\nEXAMPLE: an overlap of 1 will include its front and back neighbor slices (not including their own overlaps)  into itself.\nAn overlap of .5 will include half of its front neighbor and half of its back neighbor slice.")]
        public float overlap = 2f;
        [Tooltip("Softness is calculated for you to blend only overlapping areas. It can be set manually if Slicing is set to SOFT_CUSTOM.")]
        [Range(0.001f, .5f)]
        public float softness = .5f;

        public enum scaleConstraintType
        {
            NONE = 0,
            X_RELATIVE,
            Y_RELATIVE,
            Z_RELATIVE
        }
        [Tooltip("This will ensure your Hypercube scale always matches the aspect ratios inside Volume 1:1.\nChoose which axis to leave free. The others will be constrained to match the value of that axis.")]
        public scaleConstraintType scaleConstraint = scaleConstraintType.NONE;

        [Tooltip("If the hypercube_RTT camera is set to perspective, this will modify the FOV of each successive slice to create forced perspective effects.")]
        public float forcedPerspective = 0f; //0 is no forced perspective, other values force a perspective either towards or away from the front of the Volume.
        [Tooltip("Brightness is a final modifier on the output to Volume.\nCalculated value * Brightness = output")]
        public float brightness = 1f; //  a convenience way to set the brightness of the rendered textures. The proper way is to call 'setTone()' on the canvas
        [Tooltip("This can be used to differentiate between what is empty space, and what is 'black' in Volume.  This Color is ADDED to everything that has geometry.\n\nNOTE: Black Point can only be used if when soft slicing is being used.\nNOTE: The brighter the value here, the more color depth is effectively lost.")]
        public Color blackPoint;
        public bool autoHideMouse = true;
        public Shader softSliceShader;
        public Camera renderCam;
        public RenderTexture[] sliceTextures;
        public hypercube.castMesh castMeshPrefab;
        public hypercube.castMesh localCastMesh = null;
       
        //store our camera values here.
        float[] nearValues;
        float[] farValues;

        void Start()
        {
            if (!localCastMesh)
            {
                localCastMesh = GameObject.FindObjectOfType<hypercube.castMesh>();
                if (!localCastMesh)
                {
                    //if no canvas exists. we need to have one or the hypercube is useless.
#if UNITY_EDITOR
                    Cursor.visible = true;
                    localCastMesh = UnityEditor.PrefabUtility.InstantiatePrefab(castMeshPrefab) as hypercube.castMesh;  //try to keep the prefab connection, if possible
#else
                Cursor.visible = false;
                localCastMesh = Instantiate(castMeshPrefab); //normal instantiation, lost the prefab connection
#endif
                }
            }

            resetSettings();
        }



        void Update()
        {
            if (autoHideMouse)
            {
#if !UNITY_EDITOR

                Rect screenRect = new Rect(0, 0, Screen.width, Screen.height);
                if (screenRect.Contains(Input.mousePosition))
                    Cursor.visible = false;
                else
                    Cursor.visible = true;          
#endif
            }

            if (!localCastMesh)
                localCastMesh = GameObject.FindObjectOfType<hypercube.castMesh>();

            //maintain scale aspect ratio if desired.
            if (scaleConstraint == scaleConstraintType.NONE) 
            { }
            else if (scaleConstraint == scaleConstraintType.X_RELATIVE)
                transform.localScale = new Vector3(transform.localScale.x, transform.localScale.x * localCastMesh.aspectX.y, transform.localScale.x * localCastMesh.aspectX.z);
            else if (scaleConstraint == scaleConstraintType.Y_RELATIVE)
                transform.localScale = new Vector3(transform.localScale.y * localCastMesh.aspectY.x, transform.localScale.y, transform.localScale.y * localCastMesh.aspectY.z);
            else if (scaleConstraint == scaleConstraintType.Z_RELATIVE)
                transform.localScale = new Vector3(transform.localScale.z * localCastMesh.aspectZ.x, transform.localScale.z * localCastMesh.aspectZ.y, transform.localScale.z);

            if (transform.hasChanged)
            {
                resetSettings(); //comment this line out if you will not be scaling your cube during runtime
            }
            render();
        }

        void OnValidate()
        {
            if (sliceTextures.Length == 0)
                Debug.LogError("The Hypercube has no slice textures to render to.  Please assign them or reset the prefab.");


            if (slicing == softSliceMode.HARD)
                softness = 0f;

            if (!localCastMesh)
                localCastMesh = GameObject.FindObjectOfType<hypercube.castMesh>();

            if (localCastMesh)
            {
                localCastMesh.setTone(brightness);
                localCastMesh.updateMesh();
            }

            updateOverlap();
        }


        //let the slice image filter shader know how much 'softness' they should apply to the soft overlap
        void updateOverlap()
        {
            if (overlap < 0)
                overlap = 0;

            hypercube.softOverlap o = renderCam.GetComponent<hypercube.softOverlap>();
            if (slicing != softSliceMode.HARD)
            {
                if (slicing == softSliceMode.SOFT)
                    softness = overlap / ((overlap * 2f) + 1f); //this calculates exact interpolation between the end of a slice and the end of it's overlap area

                o.enabled = true;
                o.setShaderProperties(softness, blackPoint);
            }
            else
                o.enabled = false;
        }

        public void render()
        {
            if (overlap > 0f && slicing != softSliceMode.HARD)
                renderCam.gameObject.SetActive(true); //setting it active/inactive is only needed so that OnRenderImage() will be called on softOverlap.cs for the post process effect

            float baseViewAngle = renderCam.fieldOfView;

            if (localCastMesh.slices > sliceTextures.Length)
                localCastMesh.slices = sliceTextures.Length;

            for (int i = 0; i < localCastMesh.slices; i++)
            {
                renderCam.fieldOfView = baseViewAngle + (i * forcedPerspective); //allow forced perspective or perspective correction

                renderCam.nearClipPlane = nearValues[i];
                renderCam.farClipPlane = farValues[i];
                renderCam.targetTexture = sliceTextures[i];
                renderCam.Render();
            }

            renderCam.fieldOfView = baseViewAngle;

            if (overlap > 0f && slicing != softSliceMode.HARD)
                renderCam.gameObject.SetActive(false);
        }

        //prefs input
        public void softSliceToggle()
        {
            if (slicing == softSliceMode.HARD)
                slicing = softSliceMode.SOFT;
            else
                slicing = softSliceMode.HARD;
        }
        public void overlapUp()
        {
            overlap += .05f;
        }
        public void overlapDown()
        {
            overlap -= .05f;
        }


        //NOTE that if a parent of the cube is scaled, and the cube is arbitrarily rotated inside of it, it will return wrong lossy scale.
        // see: http://docs.unity3d.com/ScriptReference/Transform-lossyScale.html
        //TODO change this to use a proper matrix to handle local scale in a heirarchy
        public void resetSettings()
        {
            if (!localCastMesh)
                return;

            nearValues = new float[localCastMesh.getSliceCount()];
            farValues = new float[localCastMesh.getSliceCount()];

            float sliceDepth = transform.lossyScale.z / (float)localCastMesh.getSliceCount();

            renderCam.aspect = transform.lossyScale.x / transform.lossyScale.y;
            renderCam.orthographicSize = .5f * transform.lossyScale.y;

            for (int i = 0; i < localCastMesh.getSliceCount() && i < sliceTextures.Length; i++)
            {
                nearValues[i] = (i * sliceDepth) - (sliceDepth * overlap);
                farValues[i] = ((i + 1) * sliceDepth) + (sliceDepth * overlap);
            }


            updateOverlap();
        }

    }