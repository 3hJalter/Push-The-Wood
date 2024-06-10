using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.UIs.Popup
{
    public class NotificationPopup : UICanvas
    {
        private const string NOTIFICATION = "NotificationP";
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI title;

        // TODO: Change param to a notification object data with icon, title, content, etc.
        public override void Setup(object param = null)
        {
            base.Setup(param);
            // convert param to string
            title.text = param != null ? param.ToString() : NOTIFICATION;
        }
    }
}
