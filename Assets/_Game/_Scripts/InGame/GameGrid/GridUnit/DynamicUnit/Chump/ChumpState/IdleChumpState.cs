using _Game.DesignPattern.StateMachine;
using DG.Tweening;
using GameGridEnum;
using UnityEngine;

namespace _Game.GameGrid.Unit.DynamicUnit.Chump.ChumpState
{
    public class IdleChumpState : IState<Chump>
    {
        private const float MOVE_Y_VALUE = 0.06f;
        private const float MOVE_Y_TIME = 2f;
        private const float FLOATING_Y_OFFSET = 0.1f;
        private Tween floatingTween;

        private Vector3 originTransform;

        public StateEnum Id => StateEnum.Idle;

        public void OnEnter(Chump t)
        {

            #region ANIM

            //DEV: Refactor anim system
            if (t.IsInWater())
            {
                Vector3 position = t.Tf.position;
                originTransform = new Vector3(position.x,
                    (float)(Constants.DirFirstHeightOfSurface[GridSurfaceType.Water] + t.FloatingHeightOffset) / 2 *
                    Constants.CELL_SIZE - t.yOffsetOnDown - FLOATING_Y_OFFSET,
                    position.z);
                floatingTween = DOVirtual.Float(0, MOVE_Y_TIME, MOVE_Y_TIME, SetSinWavePosition).SetLoops(-1)
                    .SetEase(Ease.Linear);
            }
            else
            {
                if (floatingTween is null) return;
                floatingTween.Kill();
            }

            return;

            void SetSinWavePosition(float time)
            {
                float value = Mathf.Sin(2 * time * Mathf.PI / MOVE_Y_TIME) * MOVE_Y_VALUE;
                t.Tf.transform.position = originTransform + Vector3.up * value;
            }

            #endregion

        }

        public void OnExecute(Chump t)
        {

        }

        public void OnExit(Chump t)
        {
            floatingTween.Kill();
        }
    }
}
