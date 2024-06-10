using _Game.Managers;
using MoreMountains.NiceVibrations;

namespace _Game._Scripts.Utilities
{
    public static class HVibrate
    {
        public static bool IsHapticOn => DataManager.Ins.GameData.setting.haptic;
        
        public static void OnToggleHaptic(bool value)
        {
            DataManager.Ins.GameData.setting.haptic = value;
        }
        
        public static void Haptic(HapticTypes type)
        {
            if (!DataManager.Ins.GameData.setting.haptic)
                return;
            MMVibrationManager.Haptic(type, false, true);
        }
    }
}
