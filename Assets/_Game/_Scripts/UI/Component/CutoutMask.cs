using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace _Game._Scripts.UIs.Component
{
    public sealed class CutoutMask : Image
    {
        private static readonly int StencilComp = Shader.PropertyToID("_StencilComp");

        public override Material materialForRendering
        {
            get
            {
                Material rendering = new(base.materialForRendering);
                rendering.SetInt(StencilComp, (int)CompareFunction.NotEqual);
                return rendering;
            }
        }
    }
}
