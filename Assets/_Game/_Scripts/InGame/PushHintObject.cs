using _Game.GameGrid;
using DG.Tweening;
using UnityEngine;

namespace _Game._Scripts.InGame
{
    public class PushHintObject : HMonoBehaviour
    {
        [SerializeField] private Transform arrow;
        [SerializeField] private Transform arrowImage;
        private Tween _arrowTween;
        private const float CAMERA_X_ROTATION = 50f;
        private void OnDestroy()
        {
            _arrowTween?.Kill();
        }

        public void SetActive(bool active)
        {
            if (!active && gameObject.activeSelf)
            {
                _arrowTween?.Kill();
                arrow.localPosition = Vector3.zero;
                arrowImage.localRotation = Quaternion.Euler(CAMERA_X_ROTATION, 0, 0);
            }
            gameObject.SetActive(active);
        }
        
        public void MoveTo(int x, int y, int direction, bool isShow = true)
        {
            SetActive(isShow);
            _arrowTween?.Kill();
            // Get cell with x, y
            GameGridCell cell = LevelManager.Ins.CurrentLevel.GetCell(new Vector2Int(x, y));
            // Move this Tf to the position of the grid cell
            Tf.position = new Vector3(cell.WorldX, 0, cell.WorldY);
            // Rotate the arrow to the direction
            Direction dir = (Direction) direction;
            arrow.localPosition = Vector3.zero;
            switch (dir)
            {
                case Direction.Left:
                    arrowImage.localRotation = Quaternion.Euler(CAMERA_X_ROTATION, 0, 180);
                    // Left: Tween 0 0 0 to -1 0 0
                    _arrowTween = arrow.DOLocalMove(new Vector3(-1, 0, 0), 0.5f).SetLoops(-1, LoopType.Yoyo);
                    break;
                case Direction.Right:
                    arrowImage.localRotation = Quaternion.Euler(CAMERA_X_ROTATION, 0, 0);
                    //  Right: Tween 0 0 0 to 1 0 0 
                    _arrowTween = arrow.DOLocalMove(new Vector3(1, 0, 0), 0.5f).SetLoops(-1, LoopType.Yoyo);
                    break;
                case Direction.Forward:
                    arrowImage.localRotation = Quaternion.Euler(CAMERA_X_ROTATION, 0, 90);
                     // Forward: Tween 0 0 0 to 0 1 0 
                    _arrowTween = arrow.DOLocalMove(new Vector3(0, 1, 0), 0.5f).SetLoops(-1, LoopType.Yoyo);   
                    break;
                case Direction.Back:
                    arrowImage.localRotation = Quaternion.Euler(CAMERA_X_ROTATION, 0, -90);
                    // Back: Tween 0 0 0 to 0 -1 0
                    _arrowTween = arrow.DOLocalMove(new Vector3(0, -1, 0), 0.5f).SetLoops(-1, LoopType.Yoyo);
                    break;
                case Direction.None:
                default:
                    return;
            }
        }
    }
}
