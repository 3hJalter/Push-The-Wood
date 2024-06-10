using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Optimize
{
    public static Mesh CombineMeshes(List<MeshFilter> sourceMeshFilters)
    {
        var combine = new CombineInstance[sourceMeshFilters.Count];

        for (var i = 0; i < sourceMeshFilters.Count; i++)
        {
            combine[i].mesh = sourceMeshFilters[i].sharedMesh;
            combine[i].transform = sourceMeshFilters[i].transform.localToWorldMatrix;
        }

        var mesh = new Mesh();
        mesh.CombineMeshes(combine, true, true);
        return mesh;
    }
}
