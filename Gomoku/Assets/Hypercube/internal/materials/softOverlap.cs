using UnityEngine;
using System.Collections;
using System;

namespace hypercube
{


    [ImageEffectOpaque]
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    [AddComponentMenu("")]
    public class softOverlap : MonoBehaviour
    {
        /// Provides a shader property that is set in the inspector
        /// and a material instantiated from the shader
        public hypercubeCamera cam;

        private Material m_Material;


        protected virtual void Start()
        {
            // Disable if we don't support image effects
            if (!SystemInfo.supportsImageEffects)
            {
                enabled = false;
                return;
            }

            // Disable the image effect if the shader can't
            // run on the users graphics card
            if (!cam.softSliceShader || !cam.softSliceShader.isSupported)
                enabled = false;
            else
                GetComponent<Camera>().depthTextureMode = DepthTextureMode.Depth;
        }


        protected Material material
        {
            get
            {
                if (m_Material == null)
                {
                    m_Material = new Material(cam.softSliceShader);
                    m_Material.hideFlags = HideFlags.HideAndDontSave;
                }
                return m_Material;
            }
        }

        //used to set how much 'fade' should be applied to each end of the slice
        //for example a value of 5 will fade in 5 percent from the near and 5 from the far of the full slice leaving 90% as the unfaded 'base' of the slice
        public void setShaderProperties(float p, Color blackPoint)
        {
            p = Mathf.Clamp(p, 0f, .5f);
            material.SetFloat("_softPercent", p);
            material.SetColor("_blackPoint", blackPoint);
        }

        protected virtual void OnDestroy()
        {
            if (m_Material)
            {
                DestroyImmediate(m_Material);
            }
        }

        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {

            if (cam.overlap > 0f)
                Graphics.Blit(source, destination, material);
        }
    }

}