using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CustomMeshCombiner : MonoBehaviour
{
    const int MAX_VERT_COUNT = 65535;

    MeshFilter[] meshFilters;
    List<List<CombineInstance>> allCombine = new List<List<CombineInstance>>();
    Material mat;

    int vertCount = 0;

    void Start()
    {
        //meshFilters = GetComponentsInChildren<MeshFilter>();
        //Debug.Log("mesh filters "+meshFilters.Length);
        //mat = meshFilters[0].gameObject.GetComponent<Renderer>().material;

        //var c = new List<CombineInstance>();
        //allCombine.Add(c);

        //for (int i = 0; i < meshFilters.Length; i++)
        //{
        //    var ci = new CombineInstance();
        //    ci.mesh = meshFilters[i].mesh;
        //    ci.transform = meshFilters[i].transform.localToWorldMatrix;
        //    ci.lightmapScaleOffset = meshFilters[i].gameObject.GetComponent<Renderer>().lightmapScaleOffset;

        //    //Check if new new added combine instance exceed the limit
        //    if (vertCount + ci.mesh.vertexCount > MAX_VERT_COUNT)
        //    {
        //        //Create a new List
        //        var c2 = new List<CombineInstance>();
        //        allCombine.Add(c2); vertCount = 0;
        //    }

        //    //Add the mesh combine to the latest array
        //    allCombine[allCombine.Count - 1].Add(ci);
        //    vertCount += ci.mesh.vertexCount;
        //    meshFilters[i].gameObject.SetActive(false);
        //}

        //MakeGroups();
    }

    public void MergeObjects(GameObject objects) {
        meshFilters = objects.GetComponentsInChildren<MeshFilter>();
        mat = meshFilters[0].gameObject.GetComponent<Renderer>().material;

        var c = new List<CombineInstance>();
        allCombine.Add(c);

        for (int i = 0; i < meshFilters.Length; i++)
        {
            var ci = new CombineInstance();
            ci.mesh = meshFilters[i].mesh;
            ci.transform = meshFilters[i].transform.localToWorldMatrix;
            ci.lightmapScaleOffset = meshFilters[i].gameObject.GetComponent<Renderer>().lightmapScaleOffset;

            //Check if new new added combine instance exceed the limit
            if (vertCount + ci.mesh.vertexCount > MAX_VERT_COUNT)
            {
                //Create a new List
                var c2 = new List<CombineInstance>();
                allCombine.Add(c2); vertCount = 0;
            }

            //Add the mesh combine to the latest array
            allCombine[allCombine.Count - 1].Add(ci);
            vertCount += ci.mesh.vertexCount;
            meshFilters[i].gameObject.SetActive(false);
        }

        MakeGroups();
    }

    void MakeGroups()
    {
        for (int i = 0; i < allCombine.Count; i++)
        {
            GameObject go = null;

            if (i == 0)
            {
                go = gameObject;
            }
            else
            {
                go = new GameObject(gameObject.name + "_" + i);
                go.transform.parent = transform.parent;
            }

            var mr = go.AddComponent<MeshRenderer>();
            var mf = go.AddComponent<MeshFilter>();

            mr.material = mat;
            mf.mesh = new Mesh();
            mf.mesh.CombineMeshes(allCombine[i].ToArray(), true, true, true);
            mr.lightmapIndex = 0;
            gameObject.SetActive(true);
            transform.localEulerAngles = Vector3.zero;
        }
    }
}




