using _Game.DesignPattern;
using _Game.Managers;
using UnityEngine;

namespace VinhLB
{
    public class WorldUIUnitSpawner : HMonoBehaviour
    {
        [SerializeField]
        private Canvas _worldCanvas;
        [SerializeField]
        private Camera _worldCamera;
        [SerializeField]
        private Camera _uiCamera;

        public void SpawnFloatingWorldUI(Transform objectTF)
        {
            FloatingWorldUI floatingWorldUI =
                SimplePool.Spawn<FloatingWorldUI>(DataManager.Ins.GetWorldUIUnit(PoolType.FloatingRewardKey));

            SetupWorldUIUnit(floatingWorldUI.Tf, objectTF);

            floatingWorldUI.Initialize(null, "x1");
        }

        private void SetupWorldUIUnit(Transform worldUITF, Transform objectTF)
        {
            // worldUITF.SetParent(_worldCanvas.transform, false);
            //
            // Vector3 screenPosition = _worldCamera.WorldToScreenPoint(objectTF.position);
            // screenPosition.z = (_worldCanvas.transform.position - _uiCamera.transform.position).magnitude;
            // worldUITF.position = _uiCamera.ScreenToWorldPoint(screenPosition);

            worldUITF.position = objectTF.position;
        }
    }
}