using UnityEngine;

namespace VinhLB
{
    public class Flower : EnvironmentUnit
    {
        [SerializeField] private MeshFilter meshFilter;

        public MeshFilter MeshFilter => meshFilter;
    }
}