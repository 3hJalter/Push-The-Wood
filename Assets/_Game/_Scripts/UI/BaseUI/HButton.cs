using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VinhLB;

public class HButton : Button, IClickable
{
    public event Action OnClicked;

    public ButtonAnim buttonAnim;

    public override void OnPointerClick(PointerEventData eventData)
    {
        base.OnPointerClick(eventData);
        
        OnClicked?.Invoke();
    }

    public void Show(bool instant = false)
    {
        gameObject.SetActive(true);

        if (!instant && buttonAnim)
            buttonAnim.ShowAnim();
    }

    public void Hide(bool instant = false)
    {
        if (instant || !buttonAnim)
            gameObject.SetActive(false);
        else
            buttonAnim.HideAnim(() => gameObject.SetActive(false));
    }

    [ContextMenu("Setup")]
    protected virtual void Setup()
    {
        buttonAnim = GetComponent<ButtonAnim>();
    }
}
