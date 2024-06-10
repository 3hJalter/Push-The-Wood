using _Game.DesignPattern;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace VinhLB
{
    using UnityEngine.UI;
    public class UIUnit : GameUnit
    {
        [Title("References")]
        [SerializeField]
        protected RectTransform _rectTF;
        [SerializeField]
        protected Image _icon;
        [SerializeField]
        protected TMP_Text _text;
        // [SerializeField]
        // private Animator _animator;

        public RectTransform RectTf => _rectTF;
        public Image Icon => _icon;
        public TMP_Text Text => _text;
    }
}