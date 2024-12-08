using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MultiButton : Button
{
    [SerializeField]
    private List<Graphic> targetGraphics = new List<Graphic>();

    protected override void DoStateTransition(SelectionState state, bool instant)
    {
        Color color;
        switch (state)
        {
            case SelectionState.Normal:
                color = colors.normalColor;
                break;
            case SelectionState.Highlighted:
                color = colors.highlightedColor;
                break;
            case SelectionState.Pressed:
                color = colors.pressedColor;
                break;
            case SelectionState.Selected:
                color = colors.selectedColor;
                break;
            case SelectionState.Disabled:
                color = colors.disabledColor;
                break;
            default:
                color = Color.black;
                break;
        }

        foreach (var graphic in targetGraphics)
        {
            if (graphic != null) graphic.CrossFadeColor(color, instant ? 0f : colors.fadeDuration, true, true);
        }
    }
}