using _Game._Scripts.Managers;
using _Game.Utilities;
using UnityEngine;

namespace HControls
{
    public class HInputManager
    {
        private static bool _isLockInput;
        private static HInputManager _instance;
        private Direction _direction = Direction.None;
        private float _horizontalInput;
        private float _verticalInput;
        
        private Direction Direction
        {
            get => _isLockInput ? Direction.None : _direction;
            set => _direction = _isLockInput ? Direction.None : value;
        }
        
        private float HorizontalInput {
            get => _isLockInput ? 0f : _horizontalInput;
            set => _horizontalInput = _isLockInput ? 0f : value;
        }
        
        private float VerticalInput {
            get => _isLockInput ? 0f : _verticalInput;
            set => _verticalInput = _isLockInput ? 0f : value;
        }
        
        private static HInputManager Instance => _instance ??= new HInputManager();

        public static void SetDefault()
        {
            _isLockInput = false;
            Instance._horizontalInput = 0f;
            Instance._verticalInput = 0f;
            Instance._direction = Direction.None;
        }

        public static void LockInput(bool isLock = true)
        {
            _isLockInput = isLock;
        }
        
        public static void SetHorizontalInput(float value)
        {
            Instance.HorizontalInput = value;
        }

        public static void SetVerticalInput(float value)
        {
            Instance.VerticalInput = value;
        }

        public static void SetDirectionInput(Direction direction)
        {   
            Instance.Direction = direction;
        }

        public static void SetDirectionInput(float thresholdP2)
        {
            Instance.Direction = GetDirectionInput(thresholdP2);
        }

        public static Direction GetDirectionInput()
        {
            return Instance.Direction;
        }

        private static Direction GetDirectionInput(float thresholdP2)
        {
            Vector2 moveInput = new(Instance.HorizontalInput, Instance.VerticalInput);
            if (moveInput.sqrMagnitude < thresholdP2) return Direction.None;
            float angle = Mathf.Atan2(moveInput.y, -moveInput.x);
            moveInput = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            return Mathf.Abs(moveInput.x) > Mathf.Abs(moveInput.y)
                ? moveInput.x > 0 ? Direction.Left : Direction.Right
                : moveInput.y > 0
                    ? Direction.Forward
                    : Direction.Back;
        }
    }
}
