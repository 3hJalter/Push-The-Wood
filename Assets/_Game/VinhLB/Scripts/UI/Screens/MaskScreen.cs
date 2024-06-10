using System.Collections.Generic;
using Coffee.UIExtensions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace VinhLB
{
    public class MaskScreen : UICanvas
    {
        [SerializeField]
        private UIMask _uiMaskPrefab;
        [SerializeField]
        private RectTransform _unmaskedPanelRectTF;
        [SerializeField]
        private RectTransform _uiMaskParentRectTF;
        [SerializeField]
        private RectTransform _simpleMaskableRectTF;
        [SerializeField]
        private MaskDatabase _maskDatabase;

        private List<UIMask> _createdUIMaskList = new List<UIMask>();
        private Stack<UIMask> _availableUIMaskList = new Stack<UIMask>();
        
        public override void Open(object param = null)
        {
            base.Open(param);

            SetupMask(param);
        }

        public override void Close()
        {
            base.Close();
            
            _availableUIMaskList.Clear();
            for (int i = 0; i < _createdUIMaskList.Count; i++)
            {
                _createdUIMaskList[i].Close();
                _availableUIMaskList.Push(_createdUIMaskList[i]);
            }
        }

        public void SetupMask(object param)
        {
            if (param == null)
            {
                _unmaskedPanelRectTF.gameObject.SetActive(false);
                _simpleMaskableRectTF.gameObject.SetActive(true);
                
                return;
            }
            
            _simpleMaskableRectTF.gameObject.SetActive(false);
            _unmaskedPanelRectTF.gameObject.SetActive(true);
            
            if (param is MaskData)
            {
                MaskData data = (MaskData)param;
                
                InitializeUIMask(data);
            }
            else if (param is MaskData[])
            {
                MaskData[] data = (MaskData[])param;

                for (int i = 0; i < data.Length; i++)
                {
                    InitializeUIMask(data[i]);
                }
            }
        }

        private void InitializeUIMask(MaskData data)
        {
            UIMask mask;
            UnmaskRaycastFilter unmaskRaycastFilter;
            if (_availableUIMaskList.Count > 0)
            {
                mask = _availableUIMaskList.Pop();
                mask.Open();
                unmaskRaycastFilter = mask.UnmaskRaycastFilter;
            }
            else
            {
                mask = Instantiate(_uiMaskPrefab, _uiMaskParentRectTF);
                _createdUIMaskList.Add(mask);
                unmaskRaycastFilter = _unmaskedPanelRectTF.gameObject.AddComponent<UnmaskRaycastFilter>();
            }
            
            mask.Initialize(data.Position, data.Size, _maskDatabase.MaskSpriteDict[data.MaskType], 
                data.ClickableItem, data.OnClickedCallback, data.TargetRectTF, unmaskRaycastFilter);
        }
    }

    public struct MaskData
    {
        public Vector3 Position;
        public Vector2 Size;
        public MaskType MaskType;
        public IClickable ClickableItem;
        public System.Action OnClickedCallback;
        public RectTransform TargetRectTF;
    }

    public enum MaskType
    {
        None = -1,
        RoundedCornerRectangle = 0,
        Eclipse = 1
    }
}