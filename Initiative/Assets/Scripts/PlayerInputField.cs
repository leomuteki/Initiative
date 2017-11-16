using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerInputField : MonoBehaviour {

    // References
    public InputField Name;
    public InputField Modifier;
    
    public bool IsTapped(Vector2 touch)
    {
        RectTransform my_rect = GetComponent<RectTransform>();
        return RectTransformUtility.RectangleContainsScreenPoint(my_rect, touch);
    }

    public bool NameTapped(Vector2 touch)
    {
        RectTransform my_rect = Name.GetComponent<RectTransform>();
        return RectTransformUtility.RectangleContainsScreenPoint(my_rect, touch);
    }

    public bool ModifierTapped(Vector2 touch)
    {
        RectTransform my_rect = Modifier.GetComponent<RectTransform>();
        return RectTransformUtility.RectangleContainsScreenPoint(my_rect, touch);
    }

}
