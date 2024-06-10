using UnityEngine;
using UnityEngine.UI;

namespace HControls
{
    [RequireComponent(typeof(GameObject))]
    public class HDpadButton : HMonoBehaviour
    {
        [SerializeField] private Image mainImg;
        [SerializeField] private Direction direction;
        [SerializeField] private GameObject pointerDownImg;
        public Direction Direction => direction;

        public GameObject PointerDownImg => pointerDownImg;

        private void Awake()
        {
            pointerDownImg.SetActive(false);
        }

        public void LockInput(bool isLock)
        {
            mainImg.raycastTarget = !isLock;
        }
    }
}
