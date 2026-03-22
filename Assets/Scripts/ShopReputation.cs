using UnityEngine;

public class ShopReputation : MonoBehaviour
{
    public static ShopReputation Instance;

    public float notoriety = 0f;

    void Awake()
    {
        Instance = this;
    }

    public void AddNotoriety(float amount)
    {
        notoriety += amount;
    }
}