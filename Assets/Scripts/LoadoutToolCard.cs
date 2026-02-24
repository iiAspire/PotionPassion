using UnityEngine;
using UnityEngine.UI;

public class LoadoutToolCard : MonoBehaviour
{
    [SerializeField] Image icon;
    [SerializeField] Image highlight;

    public SOToolItem ToolData { get; private set; }

    LoadoutToolSelector selector;

    void Start()
    {
        selector = GetComponentInParent<LoadoutToolSelector>();
        SetSelected(false);
    }

    public void Initialize(SOToolItem data)
    {
        ToolData = data;
        icon.sprite = data.icon;
    }

    public void OnClick()
    {
        selector.OnToolClicked(this);
    }

    public void SetSelected(bool selected)
    {
        if (highlight)
            highlight.enabled = selected;
    }
}