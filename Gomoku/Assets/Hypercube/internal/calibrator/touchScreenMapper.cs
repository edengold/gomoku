using UnityEngine;
using System.Collections;

namespace hypercube
{
    public class touchScreenMapper : hypercube.touchScreenTarget
    {

        public TextMesh outputText;
        public GameObject arrow;
        public GameObject circle;

        public hypercubeCamera cam;

        enum calibrationStage
        {
            STEP_INVALID = -1,
            STEP_touchCorner1 = 0,
            STEP_touchCorner2,
            STEP_touchCorner3,
            STEP_touchCorner4,
            STEP_save
        }

        calibrationStage stage;

        int ULx = 0;
        int ULy = 0;
        int URx = 0;
        int URy = 0;
        int LRx = 0;
        int LRy = 0;
        int LLx = 0;
        int LLy = 0;


        protected override void OnEnable()
        {
            stage = calibrationStage.STEP_touchCorner1;
            resetStage();

 	        base.OnEnable();
        }

        protected override void OnDisable()
        {
            if (arrow)
                arrow.SetActive(false);
            if (circle)
                circle.SetActive(false);

            base.OnDisable();
        }


        // Update is called once per frame
        void Update()
        {

            if (Input.GetKeyDown(KeyCode.S) && Input.GetKey(KeyCode.LeftShift))
            {
                if (stage == calibrationStage.STEP_save)
                    save();
                return;
            }

            if (Input.GetKeyDown(KeyCode.Return))
            {
                goToNextStage();
                return;
            }

            if (stage == calibrationStage.STEP_save)
            {
                if (hypercube.input.frontScreen.touchCount > 0)
                {
                    //debug info
                    //hypercube.touchInterface i = new hypercube.touchInterface();
                    //hypercube.input.frontScreen.touches[0]._getInterface(ref i);
                    //outputText.text = hypercube.input.frontScreen.touches[0].id + ":  " + hypercube.input.frontScreen.touches[0].posX + " - " + hypercube.input.frontScreen.touches[0].posY + "\n" + i.rawTouchScreenX + "  " + i.rawTouchScreenY;
                    circle.transform.position = hypercube.input.frontScreen.touches[0].getWorldPos(cam);
                }
            }
        }

        void goToNextStage()
        {

            stage++;

            if (stage > calibrationStage.STEP_save)
                stage = calibrationStage.STEP_touchCorner1;

            resetStage();
        }

        void resetStage()
        {
            arrow.SetActive(true);
            circle.SetActive(false);
            outputText.text = "Align finger to corner.\nThen lift.";

            if (stage == calibrationStage.STEP_touchCorner1)
            {
                arrow.transform.localRotation = Quaternion.identity;
                arrow.transform.localPosition = cam.transform.TransformPoint(-.5f, .5f, -.5f);
            }
            else if (stage == calibrationStage.STEP_touchCorner2)
            {
                arrow.transform.localRotation = Quaternion.Euler(0f, 0f, 270f);
                arrow.transform.localPosition = cam.transform.TransformPoint(.5f, .5f, -.5f);
            }
            else if (stage == calibrationStage.STEP_touchCorner3)
            {
                arrow.transform.localRotation = Quaternion.Euler(0f, 0f, 180f);
                arrow.transform.localPosition = cam.transform.TransformPoint(.5f, -.5f, -.5f);
            }
            else if (stage == calibrationStage.STEP_touchCorner4)
            {
                arrow.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                arrow.transform.localPosition = cam.transform.TransformPoint(-.5f, -.5f, -.5f);
            }
            else if (stage == calibrationStage.STEP_save)
            {
                arrow.SetActive(false);
                circle.SetActive(true);

                outputText.text = "<color=#00ffff>Is the circle aligned?</color>\nIf <color=#00ff00>YES</color>,  Lshift + S to save.\nIf <color=#ff0000>NO</color> press ENTER\n to try again.";
            }
        }

        public override void onTouchUp(hypercube.touch touch)
        {
            hypercube.touchInterface i = new hypercube.touchInterface();
            touch._getInterface(ref i);
            if (stage == calibrationStage.STEP_touchCorner1)
            {
                ULx = i.rawTouchScreenX;
                ULy = i.rawTouchScreenY;
                goToNextStage();
            }
            else if (stage == calibrationStage.STEP_touchCorner2)
            {
                URx = i.rawTouchScreenX;
                URy = i.rawTouchScreenY;
                goToNextStage();
            }
            else if (stage == calibrationStage.STEP_touchCorner3)
            {
                LRx = i.rawTouchScreenX;
                LRy = i.rawTouchScreenY;
                goToNextStage();
            }
            else if (stage == calibrationStage.STEP_touchCorner4)
            {
                LLx = i.rawTouchScreenX;
                LLy = i.rawTouchScreenY;
                set();
                goToNextStage();
            }
        }


        void set()
        {
            //save the settings...
            dataFileDict d = cam.localCastMesh.gameObject.GetComponent<dataFileDict>();

            //gather normalized limits
            float resX = d.getValueAsFloat("touchScreenResX", 800f);
            float resY = d.getValueAsFloat("touchScreenResY", 480f);

            float top = (float)(ULy + URy) / 2f;//use averages.
            float bottom = (float)(LLy + LRy) / 2f;
            float left = (float)(ULx + LLx) / 2f;
            float right = (float)(URx + LRx) / 2f;

            top /= resY; //normalize the raw averages
            bottom /= resY;
            left /= resX;
            right /= resX;

            d.setValue("touchScreenMapTop", top);
            d.setValue("touchScreenMapBottom", bottom);
            d.setValue("touchScreenMapLeft", left);
            d.setValue("touchScreenMapRight", right);

#if HYPERCUBE_INPUT
            hypercube.input.frontScreen.setTouchScreenDims(d);
#endif
        }

        void save()
        {
            cam.localCastMesh.saveConfigSettings();
            outputText.text = "SAVED!\n\nPress ESCAPE to exit.";
        }


    }

}