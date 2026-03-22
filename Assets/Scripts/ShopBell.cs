using UnityEngine;

public class ShopBell : MonoBehaviour
{
    public static ShopBell Instance { get; private set; }

    [SerializeField] AudioSource bellSource;

    void Awake()
    {
        Instance = this;
    }

    public void Ring()
    {
        bellSource?.Play();
    }
}