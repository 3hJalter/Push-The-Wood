using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Assign this script to the indicator prefabs.
/// </summary>
public class Indicator : MonoBehaviour
{
    [SerializeField] private IndicatorType indicatorType;
    private Image indicatorImage;
    private Text distanceText;

    [SerializeField] private Image customizeArrowImage;
    [SerializeField] private List<Sprite> arrowSprites;
    
    /// <summary>
    /// Gets if the game object is active in hierarchy.
    /// </summary>
    public bool Active => gameObject.activeInHierarchy;

    /// <summary>
    /// Gets the indicator type
    /// </summary>
    public IndicatorType Type => indicatorType;

    void Awake()
    {
        indicatorImage = GetComponent<Image>();
        distanceText = GetComponentInChildren<Text>();
    }

    public void SetArrowImage(ArrowIndicatorType arrowIndicatorType)
    {
        if (indicatorType is not IndicatorType.ARROW) {
                customizeArrowImage.enabled = false;
                return;
        }
        
        if (arrowIndicatorType is not ArrowIndicatorType.None)
        {
            customizeArrowImage.enabled = true;
            customizeArrowImage.sprite = arrowSprites[(int)arrowIndicatorType];
            customizeArrowImage.rectTransform.rotation = Quaternion.identity;
        }
        else
        {
            customizeArrowImage.enabled = false;
        }
    }
    
    /// <summary>
    /// Sets the image color for the indicator.
    /// </summary>
    /// <param name="color"></param>
    public void SetImageColor(Color color)
    {
        indicatorImage.color = color;
    }

    /// <summary>
    /// Sets the distance text for the indicator.
    /// </summary>
    /// <param name="value"></param>
    public void SetDistanceText(float value)
    {
        distanceText.text = value >= 0 ? Mathf.Floor(value) + " m" : "";
    }

    /// <summary>
    /// Sets the distance text rotation of the indicator.
    /// </summary>
    /// <param name="rotation"></param>
    public void SetTextRotation(Quaternion rotation)
    {
        distanceText.rectTransform.rotation = rotation;
    }

    /// <summary>
    /// Sets the indicator as active or inactive.
    /// </summary>
    /// <param name="value"></param>
    public void Activate(bool value)
    {
        gameObject.SetActive(value);
        
    }
}

public enum IndicatorType
{
    BOX,
    ARROW
}

public enum ArrowIndicatorType
{
    None = -1,
    Fruit = 0,
    Chicken = 1,
    BonusChest = 2,
    CompassChest = 3,
}