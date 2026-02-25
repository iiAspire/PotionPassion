using UnityEngine;
using UnityEngine.UI;

public class LoadoutToolCard : MonoBehaviour
{
    [SerializeField] Image icon;
    [SerializeField] Image highlight;

    public SOToolItem ToolData { get; private set; }
    public int OriginalIndex { get; set; }

    LoadoutToolSelector selector;

    public void Initialize(SOToolItem data, LoadoutToolSelector owner)
    {
        ToolData = data;
        icon.sprite = data.icon;

        selector = owner;

        SetSelected(false);
    }

    public void OnClick()
    {
        if (selector != null)
            selector.OnToolClicked(this);
    }

    public void SetSelected(bool selected)
    {
        if (highlight)
            highlight.enabled = selected;
    }
}