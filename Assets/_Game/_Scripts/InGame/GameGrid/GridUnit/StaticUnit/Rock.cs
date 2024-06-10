using _Game.Managers;
using AudioEnum;
using DG.Tweening;
using UnityEngine;

namespace _Game.GameGrid.Unit.StaticUnit
{
    public class Rock : GridUnitStatic
    {
        private const int DEGREE = 6;
        private const float DECAY_VALUE = 0.9f;
        public override void OnBePushed(Direction direction, GridUnit unit)
        {
            //NOTE: Refactor
            //NOTE: Play Shake Animation
            //DevLog.Log(DevId.Hung, "Rock Block");
            Vector3 axis = Vector3.Cross(Vector3.up, Constants.DirVector3[direction]);
            float lastAngle = 0;
            AudioManager.Ins.PlaySfx(SfxType.PushStone);
            DOVirtual.Float(0, DEGREE * 4 * DECAY_VALUE * DECAY_VALUE, Constants.MOVING_TIME * 1.2f, i =>
            {
                float rotateAngle;
                //NOTE: Calculate Angle
                if (i <= DEGREE)
                {
                    rotateAngle = i;
                }
                else if (i <= 3 * DEGREE * DECAY_VALUE)
                {
                    rotateAngle = 2 * DEGREE - i;
                }
                else
                {
                    rotateAngle = i - 4 * DEGREE * DECAY_VALUE * DECAY_VALUE;
                }
                transform.RotateAround(anchor.Tf.position, axis, rotateAngle - lastAngle);
                lastAngle = rotateAngle;
            }).SetUpdate(UpdateType.Fixed)
                    .SetEase(Ease.OutQuad);
        }
    }
}
