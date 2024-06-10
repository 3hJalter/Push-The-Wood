using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Game.Content
{
    public class SkinController : MonoBehaviour
    {
        [SerializeField]
        GameObject[] SkinObjects;
        [SerializeField]
        Animator[] SkinAnimators;

        private int currentSkinIndex = 0;
        public Animator CurrentAnimator => SkinAnimators[currentSkinIndex];

        private void Awake()
        {
            
        }
        public Animator ChangeSkin(int index)
        {
            foreach(GameObject skin in SkinObjects)
            {
                skin.SetActive(false);
            }
            currentSkinIndex = index;
            SkinObjects[currentSkinIndex].SetActive(true);
            if(SkinAnimators != null)
                return SkinAnimators[currentSkinIndex];
            return null;
        }
    }
}