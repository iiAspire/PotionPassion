using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    [SerializeField] private ShopShelfManager shelfManager;

    public bool IsShopOpen { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public void OpenShop()
    {
        IsShopOpen = true;
        shelfManager.PopulateShelves();
    }

    public void CloseShop()
    {
        IsShopOpen = false;
        shelfManager.ClearShelves();
    }

    public void Sell(ShelfItemView shelfItem)
    {
        CardComponent sourceCard = shelfItem.GetSourceCard();
        if (sourceCard == null)
            return;

        // Remove the actual item
        Destroy(sourceCard.gameObject);

        // TODO: Grant reward cards here

        // Refresh shelves to stay in sync
        shelfManager.PopulateShelves();
    }
}