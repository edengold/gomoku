using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace hypercube
{

    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    public class hypercubePreview : MonoBehaviour
    {

        [HideInInspector]
        public int sliceCount = 12;
        public float sliceDistance = .1f;

        public Material[] previewMaterials;


        void Start()
        {
            //try in a lazy way to connect ourselves
            castMesh c = GameObject.FindObjectOfType<castMesh>();
            if (c && !c.preview)
                c.preview = this;
        }

        void OnValidate()
        {
            if (previewMaterials.Length < 1)
                Debug.LogError("Preview has no items in it's material list.  It needs to be able to choose from materials to apply to it's slices.");

            if (sliceCount < 1)
                sliceCount = 1;

            if (sliceCount > previewMaterials.Length)
                sliceCount = previewMaterials.Length;

            updateMesh();
        }

        public void updateMesh()
        {
            if (previewMaterials.Length == 0)
            {
                Debug.LogError("Canvas materials have not been set!  Please define what materials you want to apply to each slice in the hypercubeCanvas component.");
                return;
            }

            if (sliceCount < 1)
            {
                sliceCount = 1;
                return;
            }

            if (sliceCount > previewMaterials.Length)
            {
                Debug.LogWarning("Can't add more than " + previewMaterials.Length + " slices, because only " + previewMaterials.Length + " canvas materials are defined.");
                sliceCount = previewMaterials.Length;
                return;
            }

            Vector3[] verts = new Vector3[4 * sliceCount]; //4 verts in a quad * slices * dimensions  
            Vector2[] uvs = new Vector2[4 * sliceCount];
            Vector3[] normals = new Vector3[4 * sliceCount]; //normals are necessary for the transparency shader to work (since it uses it to calculate camera facing)
            List<int[]> submeshes = new List<int[]>(); //the triangle list(s)
            Material[] faceMaterials = new Material[sliceCount];

            //create the mesh
            for (int z = 0; z < sliceCount; z++)
            {
                int v = z * 4;
                float zPos = (float)z * sliceDistance;

                verts[v + 0] = new Vector3(-.5f, .5f, zPos); //top left
                verts[v + 1] = new Vector3(.5f, .5f, zPos); //top right
                verts[v + 2] = new Vector3(.5f, -.5f, zPos); //bottom right
                verts[v + 3] = new Vector3(-.5f, -.5f, zPos); //bottom left
                normals[v + 0] = new Vector3(0, 0, 1);
                normals[v + 1] = new Vector3(0, 0, 1);
                normals[v + 2] = new Vector3(0, 0, 1);
                normals[v + 3] = new Vector3(0, 0, 1);

                uvs[v + 0] = new Vector2(0, 1);
                uvs[v + 1] = new Vector2(1, 1);
                uvs[v + 2] = new Vector2(1, 0);
                uvs[v + 3] = new Vector2(0, 0);


                int[] tris = new int[6];
                tris[0] = v + 0; //1st tri starts at top left
                tris[1] = v + 1;
                tris[2] = v + 2;
                tris[3] = v + 2; //2nd triangle begins here
                tris[4] = v + 3;
                tris[5] = v + 0; //ends at bottom right       
                submeshes.Add(tris);

                //every face has a separate material/texture     
                faceMaterials[z] = previewMaterials[z];
            }


            MeshRenderer r = GetComponent<MeshRenderer>();
            MeshFilter mf = GetComponent<MeshFilter>();

            Mesh m = mf.sharedMesh;
            if (!m)
                m = new Mesh(); //probably some in-editor state where things aren't init.
            m.Clear();
            m.vertices = verts;
            m.uv = uvs;
            m.normals = normals;

            m.subMeshCount = sliceCount;
            for (int s = 0; s < sliceCount; s++)
            {
                m.SetTriangles(submeshes[s], s);
            }

            r.sharedMaterials = faceMaterials;

            m.RecalculateBounds();
        }
    }

}
