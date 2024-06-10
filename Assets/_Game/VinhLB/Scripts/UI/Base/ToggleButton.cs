using _Game.Managers;
using AudioEnum;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace VinhLB
{
    public class ToggleButton : HMonoBehaviour, IPointerClickHandler
    {
        [Header("General")]
        [SerializeField]
        protected bool _isOn = true;
        [SerializeField]
        protected SfxType _buttonSound = SfxType.ClickToggle;

        [Header("Events")]
        [SerializeField]
        protected UnityEvent<bool> _onValueChanged;

        public UnityEvent<bool> OnValueChanged => _onValueChanged;

        public bool IsOn
        {
            get => _isOn;
            set
            {
                _isOn = value;
                _onValueChanged.Invoke(value);
            }
        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            IsOn = !_isOn;
            
            AudioManager.Ins.PlaySfx(_buttonSound);
        }
    }
}