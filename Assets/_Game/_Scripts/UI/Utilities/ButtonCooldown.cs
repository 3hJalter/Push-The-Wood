using _Game.Utilities.Timer;
using Newtonsoft.Json.Bson;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.Utilities
{
    public class ButtonCooldown : MonoBehaviour
    {
        [SerializeField]
        float cooldownTime;
        [SerializeField]
        Button button;
        Coroutine coroutine;
        private void Awake()
        {
            button.onClick.AddListener(WaitCooldown);
        }

        IEnumerator ActiveCooldownTime()
        {
            yield return new WaitForSeconds(cooldownTime);
            button.enabled = true;
        }

        public void WaitCooldown()
        {
            button.enabled = false;
            coroutine = StartCoroutine(ActiveCooldownTime());
        }

        public void SetActive(bool active)
        {
            if(active) 
            {
                StopCoroutine(coroutine);
                button.enabled = true;
            }
        }
    }
}