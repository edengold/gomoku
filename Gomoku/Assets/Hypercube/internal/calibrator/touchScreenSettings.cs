using UnityEngine;
using System.Collections;

namespace hypercube
{
    public class touchScreenSettings : touchScreenTarget
    {
        public UnityEngine.UI.InputField resXInput;
        public UnityEngine.UI.InputField resYInput;
        public UnityEngine.UI.InputField sizeWInput;
        public UnityEngine.UI.InputField sizeHInput;
        public UnityEngine.UI.InputField sizeDInput;

        public UnityEngine.UI.Text display;

        public hypercubeCamera cam;

		Vector3 originalScale;  //this is used so the aspect ratios can be tested in situ

        protected override void OnEnable()
        {
			originalScale = cam.transform.localScale;
			cam.scaleConstraint = hypercubeCamera.scaleConstraintType.X_RELATIVE;
			cam.slicing = hypercubeCamera.softSliceMode.SOFT;
			cam.overlap = 1f;

            dataFileDict d = cam.localCastMesh.gameObject.GetComponent<dataFileDict>();

            resXInput.text = d.getValue("touchScreenResX", "800");
            resYInput.text = d.getValue("touchScreenResY", "480");
            sizeWInput.text = d.getValue("projectionCentimeterWidth", "20");
            sizeHInput.text = d.getValue("projectionCentimeterHeight", "12");
            sizeDInput.text = d.getValue("projectionCentimeterDepth", "20");

            base.OnEnable();
        }


        protected override void OnDisable()
        {
			base.OnDisable();

			if (!cam)
				return;
			
			cam.scaleConstraint = hypercubeCamera.scaleConstraintType.NONE;
			cam.transform.localScale = originalScale;
			cam.slicing = hypercubeCamera.softSliceMode.HARD;
			cam.overlap = 0f;

			if (!cam.localCastMesh)
				return;
			
			dataFileDict d = cam.localCastMesh.gameObject.GetComponent<dataFileDict> ();

			d.setValue ("touchScreenResX", resXInput.text);
			d.setValue ("touchScreenResY", resYInput.text);
			d.setValue ("projectionCentimeterWidth", sizeWInput.text);
			d.setValue ("projectionCentimeterHeight", sizeHInput.text);
			d.setValue ("projectionCentimeterDepth", sizeDInput.text);          
        }

        public override void onTouchMoved(touch touch)
        {
            touchInterface i = new touchInterface();
            touch._getInterface(ref i);

            display.text = "Latest Values:\nx: " + i.rawTouchScreenX + "\ny: " + i.rawTouchScreenY;
        }

		public void onTextUpdate()
		{
			#if HYPERCUBE_DEV
			float x = dataFileDict.stringToFloat(sizeWInput.text, -1f);
			float y = dataFileDict.stringToFloat(sizeHInput.text, -1f);
			float z = dataFileDict.stringToFloat(sizeDInput.text, -1f);

			if (x <= 0f || y <= 0f || z <= 0f)
				return;
			
			cam.localCastMesh.setProjectionAspectRatios(x,y,z);
			#endif
		}
    }
}