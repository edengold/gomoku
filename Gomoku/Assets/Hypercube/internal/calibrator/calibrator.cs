using UnityEngine;
using System.Collections;

//this is a tool to set calibrations on individual corners of the hypercubeCanvas
//TO USE:
//add this component to an empty gameObject
//connect the canvas to this component
//connect the hypercube camera to this component
//use TAB to cycle through the slices
//use Q E Z C S  to highlight a particular vertex on the slice
//use WADX to make adjustments
//use ENTER to load settings from the file


namespace hypercube
{

#if HYPERCUBE_DEV
    public enum distortionCompensationType
    {
        PIXEL,
        SPATIAL
    }

    public enum canvasEditMode
    {
        UL = 0,
        UR,
        LL,
        LR,
        M
    }
#endif

    public class calibrator : MonoBehaviour
    {
        
#if !HYPERCUBE_DEV
        [Header("-Requires HYPERCUBE_DEV define-")]

        //don't lose these references, but they are useless without the define so hide them.
        
        public castMesh canvas;
        [HideInInspector]
        public Texture2D calibrationCorner;
        [HideInInspector]
        public Texture2D calibrationCenter;
        [HideInInspector]
        public Material selectedMat;
        [HideInInspector]
        public Material offMat;
#else

        public string current;
        public castMesh canvas;
        public bool pauseInput = false;
        public float brightness = 3f;      

        [Tooltip("How sensitive do you want your calibrations to be.")]
        public float interval = 1f;
        [Tooltip("Pixel movement will cause the interval to cause interval * pixel movements. Spatial will feel more intuitive if you are working directly on the volume.")]
        public distortionCompensationType relativeTo = distortionCompensationType.SPATIAL;

        //these default to upside down because the 'default' orientation of Volume is upside down on the castMesh if viewed in a normal monitor
        //this may sound strange, but it keeps the most difficult code inside castMesh more readable and corresponding to intuition.
        public KeyCode nextSlice = KeyCode.R;
        public KeyCode prevSlice = KeyCode.F;
        public KeyCode highlightUL = KeyCode.Q;
        public KeyCode highlightUR = KeyCode.E;
        public KeyCode highlightLL = KeyCode.Z;
        public KeyCode highlightLR = KeyCode.C;
        public KeyCode highlightM = KeyCode.S;
        public KeyCode up = KeyCode.X;
        public KeyCode down = KeyCode.W;
        public KeyCode left = KeyCode.A;
        public KeyCode right = KeyCode.D;
        public KeyCode skewXUp = KeyCode.LeftArrow;
        public KeyCode skewXDn = KeyCode.RightArrow;
        public KeyCode skewYUp = KeyCode.UpArrow;
        public KeyCode skewYDn = KeyCode.DownArrow;
        public KeyCode bowTopUp = KeyCode.Alpha9;
        public KeyCode bowTopDn = KeyCode.O;
        public KeyCode bowBotUp = KeyCode.L;
        public KeyCode bowBotDn = KeyCode.Period;
        public KeyCode bowLLeft = KeyCode.J;
        public KeyCode bowLRight = KeyCode.K;
        public KeyCode bowRLeft = KeyCode.Semicolon;
        public KeyCode bowRRight = KeyCode.Quote;

        public Texture2D calibrationCorner;
        public Texture2D calibrationCenter;
        public Material selectedMat;
        public Material offMat;

		enum testingMode
		{
			OFF = 0,
			TEST1,
			TEST1_ALL,
			TEST2,
			TEST2_ALL
		}

		testingMode testState = testingMode.OFF; //show the test material instead of the normal calibrating images
        public Material blackMat;
        public Material testMat1; //the TEST words
		public Material testMat2; //checkerboard test

        canvasEditMode m;
        int currentSlice;

        void OnEnable()
        {
            canvas.updateMesh();
        }
        void OnDisable()
        {
            canvas.updateMesh();
        }

        public void copyCurrentSliceCalibration()
        {
            canvas.copyCurrentSliceCalibration(currentSlice);
        }

        void sliceUp()
        {
            currentSlice++;
            if (currentSlice >= canvas.getSliceCount())
                currentSlice = 0;
        }
        void sliceDn()
        {
            currentSlice--;
            if (currentSlice < 0)
                currentSlice = canvas.getSliceCount() - 1;
        }

        // Update is called once per frame
        void Update()
        {
            if (!canvas)
                return;

            if (pauseInput)
                return;

            //copy slice calibration
            if (Input.GetKeyDown(KeyCode.C) && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
            {
                copyCurrentSliceCalibration();
                return;
            }

            canvasEditMode oldMode = m;
            int oldSelection = currentSlice;

            if (Input.GetKeyDown(nextSlice))
            {
                if (canvas._flipZ)
                    sliceDn();
                else
                    sliceUp();
            }
            if (Input.GetKeyDown(prevSlice))
            {
                if (canvas._flipZ)
                    sliceUp();
                else
                    sliceDn();
            }
            else if (Input.GetKeyDown(skewXDn))
            {
                float xPixel = 2f / canvas.sliceWidth;  //here it is 2 instead of 1 because x raw positions correspond from -1 to 1, while y raw positions correspond from 0 to 1
                if (relativeTo == distortionCompensationType.SPATIAL)
					xPixel *= (canvas.sliceWidth/ canvas.sliceHeight) * canvas.aspectX.y;
                canvas.makeSkewAdjustment(currentSlice, true, interval * xPixel);
            }
            else if (Input.GetKeyDown(skewXUp))
            {
                float xPixel = 2f / canvas.sliceWidth;  //here it is 2 instead of 1 because x raw positions correspond from -1 to 1, while y raw positions correspond from 0 to 1
                if (relativeTo == distortionCompensationType.SPATIAL)
					xPixel *= (canvas.sliceWidth/ canvas.sliceHeight) * canvas.aspectX.y;
                canvas.makeSkewAdjustment(currentSlice, true, -interval * xPixel);
            }
            else if (Input.GetKeyDown(skewYUp))
            {
                float yPixel = 1f / ((float)canvas.sliceHeight * canvas.getSliceCount());
                canvas.makeSkewAdjustment(currentSlice, false, interval * yPixel);
            }
            else if (Input.GetKeyDown(skewYDn))
            {
                float yPixel = 1f / ((float)canvas.sliceHeight * canvas.getSliceCount());
                canvas.makeSkewAdjustment(currentSlice, false, -interval * yPixel);
            }


            else if (Input.GetKeyDown(bowTopUp))
            {
                float yPixel = 1f / ((float)canvas.sliceHeight * canvas.getSliceCount());
                canvas.makeBowAdjustment(currentSlice, castMesh.bowEdge.top, -interval * yPixel);
            }
            else if (Input.GetKeyDown(bowTopDn))
            {
                float yPixel = 1f / ((float)canvas.sliceHeight * canvas.getSliceCount());
                canvas.makeBowAdjustment(currentSlice, castMesh.bowEdge.top, interval * yPixel);
            }
            else if (Input.GetKeyDown(bowBotUp))
            {
                float yPixel = 1f / ((float)canvas.sliceHeight * canvas.getSliceCount());
                canvas.makeBowAdjustment(currentSlice, castMesh.bowEdge.bottom, -interval * yPixel);
            }
            else if (Input.GetKeyDown(bowBotDn))
            {
                float yPixel = 1f / ((float)canvas.sliceHeight * canvas.getSliceCount());
                canvas.makeBowAdjustment(currentSlice, castMesh.bowEdge.bottom, interval * yPixel);
            }
            else if (Input.GetKeyDown(bowLLeft))
            {
                float xPixel = 2f / canvas.sliceWidth; //the xpixel makes the movement distance between x/y equivalent (instead of just a local transform)
                if (relativeTo == distortionCompensationType.SPATIAL)
					xPixel *= (canvas.sliceWidth/ canvas.sliceHeight) * canvas.aspectX.y;
                canvas.makeBowAdjustment(currentSlice, castMesh.bowEdge.left, interval * xPixel);
            }
            else if (Input.GetKeyDown(bowLRight))
            {
                float xPixel = 2f / canvas.sliceWidth; //the xpixel makes the movement distance between x/y equivalent (instead of just a local transform)
                if (relativeTo == distortionCompensationType.SPATIAL)
					xPixel *= (canvas.sliceWidth/ canvas.sliceHeight) * canvas.aspectX.y;
                canvas.makeBowAdjustment(currentSlice, castMesh.bowEdge.left, -interval * xPixel);
            }
            else if (Input.GetKeyDown(bowRLeft))
            {
                float xPixel = 2f / canvas.sliceWidth; //the xpixel makes the movement distance between x/y equivalent (instead of just a local transform)
                if (relativeTo == distortionCompensationType.SPATIAL)
					xPixel *= (canvas.sliceWidth/ canvas.sliceHeight) * canvas.aspectX.y;
                canvas.makeBowAdjustment(currentSlice, castMesh.bowEdge.right, interval * xPixel);
            }
            else if (Input.GetKeyDown(bowRRight))
            {
                float xPixel = 2f / canvas.sliceWidth; //the xpixel makes the movement distance between x/y equivalent (instead of just a local transform)
                if (relativeTo == distortionCompensationType.SPATIAL)
					xPixel *= (canvas.sliceWidth/ canvas.sliceHeight) * canvas.aspectX.y;
                canvas.makeBowAdjustment(currentSlice, castMesh.bowEdge.right, -interval * xPixel);
            }


            else if (Input.GetKeyDown(highlightUL))
            {
                m = canvasEditMode.UL;
            }
            else if (Input.GetKeyDown(highlightUR))
            {
                m = canvasEditMode.UR;
            }
            else if (Input.GetKeyDown(highlightLL))
            {
                m = canvasEditMode.LL;
            }
            else if (Input.GetKeyDown(highlightLR))
            {
                m = canvasEditMode.LR;
            }
            else if (Input.GetKeyDown(highlightM))
            {
                m = canvasEditMode.M;
            }
            else if (Input.GetKeyDown(left))
            {
                float xPixel = 2f / canvas.sliceWidth; //the xpixel makes the movement distance between x/y equivalent (instead of just a local transform)
                if (relativeTo == distortionCompensationType.SPATIAL)
					xPixel *= (canvas.sliceWidth/ canvas.sliceHeight) * canvas.aspectX.y;
                canvas.makeAdjustment(currentSlice, m, true, -interval * xPixel);
            }
            else if (Input.GetKeyDown(right))
            {
                float xPixel = 2f / canvas.sliceWidth;  //here it is 2 instead of 1 because x raw positions correspond from -1 to 1, while y raw positions correspond from 0 to 1
                if (relativeTo == distortionCompensationType.SPATIAL)
					xPixel *=  (canvas.sliceWidth/ canvas.sliceHeight) * canvas.aspectX.y;
                canvas.makeAdjustment(currentSlice, m, true, interval * xPixel);
            }
            else if (Input.GetKeyDown(down))
            {
                float yPixel = 1f / ((float)canvas.sliceHeight * canvas.getSliceCount());
                canvas.makeAdjustment(currentSlice, m, false, interval * yPixel);
            }
            else if (Input.GetKeyDown(up))
            {
                float yPixel = 1f / ((float)canvas.sliceHeight * canvas.getSliceCount());
                canvas.makeAdjustment(currentSlice, m, false, -interval * yPixel);
            }

            if (currentSlice != oldSelection || m != oldMode)
            {
                current = "s" + currentSlice + "  " + m.ToString();
                canvas.updateMesh();
            }

            //testing
			if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && Input.GetKeyDown(KeyCode.Alpha1))
            {
				testState = testingMode.OFF;
                canvas.updateMesh();
            }
			else if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && Input.GetKeyDown(KeyCode.Alpha2))
			{
				testState = testingMode.TEST1;
				canvas.updateMesh();
			}
			else if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && Input.GetKeyDown(KeyCode.Alpha3))
			{
				testState = testingMode.TEST1_ALL;
				canvas.updateMesh();
			}
			else if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && Input.GetKeyDown(KeyCode.Alpha4))
			{
				testState = testingMode.TEST2;
				canvas.updateMesh();
			}
			else if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && Input.GetKeyDown(KeyCode.Alpha5))
			{
				testState = testingMode.TEST2_ALL;
				canvas.updateMesh();
			}

			if (Input.GetKeyDown (KeyCode.Alpha1)) 
			{
				interval = .1f;
				Debug.Log ("Set sensitivity to: " + interval);
			}
			else if (Input.GetKeyDown (KeyCode.Alpha2)) 
			{
				interval = .5f;
				Debug.Log ("Set sensitivity to: " + interval);
			}
			else if (Input.GetKeyDown (KeyCode.Alpha3)) 
			{
				interval = 1f;
				Debug.Log ("Set sensitivity to: " + interval);
			}
			else if (Input.GetKeyDown (KeyCode.Alpha4)) 
			{
				interval = 2f;
				Debug.Log ("Set sensitivity to: " + interval);
			}
			else if (Input.GetKeyDown (KeyCode.Alpha5)) 
			{
				interval = 4f;
				Debug.Log ("Set sensitivity to: " + interval);
			}

          

        }

        void OnValidate()
        {

            if (!canvas)
            {
                //thats weird... this should already be set in the prefab, try to automagically fix...
                canvas = GetComponent<castMesh>();
                if (!canvas)
                {
                 //   Debug.LogWarning("The calibration tool has no hypercubeCanvas to calibrate!");
                    return;
                }
            }

            selectedMat.SetFloat("_Mod", brightness);
            offMat.SetFloat("_Mod", brightness);

            canvas.updateMesh();

        }


        public Material[] getMaterials()
        {
            
            if (m == canvasEditMode.M)
            {
                selectedMat.SetTexture("_MainTex", calibrationCenter);
                selectedMat.SetTextureScale("_MainTex", new Vector2(1f, 1f));
            }
            else
            {
                selectedMat.SetTexture("_MainTex", calibrationCorner);
                if (m == canvasEditMode.UL)
                    selectedMat.SetTextureScale("_MainTex", new Vector2(1f, 1f));
                else if (m == canvasEditMode.UR)
                    selectedMat.SetTextureScale("_MainTex", new Vector2(-1f, 1f));
                else if (m == canvasEditMode.LL)
                    selectedMat.SetTextureScale("_MainTex", new Vector2(1f, -1f));
                else if (m == canvasEditMode.LR)
                    selectedMat.SetTextureScale("_MainTex", new Vector2(-1f, -1f));
            }

            Material[] outMats = new Material[canvas.getSliceCount()];

            for (int i = 0; i < canvas.getSliceCount(); i++)
            {
				if (testState == testingMode.OFF) //normal path
				{
					if (i == currentSlice)
						outMats[i] = selectedMat;
					else
						outMats[i] = offMat;
				}
				else if (testState == testingMode.TEST1)
                {
                    if (i == currentSlice)
                        outMats[i] = testMat1;
                    else
                        outMats[i] = blackMat;
                }
				else if (testState == testingMode.TEST1_ALL)
                {
					outMats[i] = testMat1;
                }
				else if (testState == testingMode.TEST2)
				{
					if (i == currentSlice)
						outMats[i] = testMat2;
					else
						outMats[i] = blackMat;
				}
				else if (testState == testingMode.TEST2_ALL)
				{
					outMats[i] = testMat2;
				}
            }


            return outMats;
        }
#endif

    }

}