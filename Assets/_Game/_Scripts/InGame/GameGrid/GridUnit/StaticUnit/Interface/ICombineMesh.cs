using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Game.DesignPattern
{
    public interface ICombineMesh
    {
        public List<MeshFilter> CombineMeshs(bool isMeshActive);
    }
}