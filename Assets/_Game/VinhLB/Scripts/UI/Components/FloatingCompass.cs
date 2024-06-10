using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VinhLB
{
    public class FloatingCompass : UIUnit
    {
        [SerializeField]
        private CanvasGroup _canvasGroup;

        public CanvasGroup CanvasGroup => _canvasGroup;
    }
}