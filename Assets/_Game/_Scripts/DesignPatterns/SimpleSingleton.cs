using Unity.VisualScripting;

namespace _Game.DesignPattern
{
    public class SimpleSingleton<T> : HMonoBehaviour where T : HMonoBehaviour
    {
        protected static T _instance;

        public static T Ins
        {
            get
            {
                return _instance;
            }
        }
    }
}
