using UnityEngine;
using System.Collections;

namespace hypercube
{
    public class basicSettingsAdjustor : MonoBehaviour 
    {
        public UnityEngine.UI.InputField modelName;
        public UnityEngine.UI.InputField versionNumber;

        public UnityEngine.UI.InputField sliceCount;

        public castMesh canvas;

         void OnEnable()
        {
            if (!canvas)
                return;

            dataFileDict d = canvas.GetComponent<dataFileDict>();

            modelName.text = d.getValue("volumeModelName", "UNKNOWN!");
            versionNumber.text = d.getValue("volumeHardwareVersion", "-9999");

            sliceCount.text = canvas.slices.ToString();
        }

        void OnDisable()
        {
            if (!canvas)
                return;

            dataFileDict d = canvas.GetComponent<dataFileDict>();

            d.setValue("volumeModelName", modelName.text);
            d.setValue("volumeHardwareVersion", dataFileDict.stringToFloat(versionNumber.text, -9999f));

            canvas.slices = dataFileDict.stringToInt(sliceCount.text, 10);

        }
    }
}


