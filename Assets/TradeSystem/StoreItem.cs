using UnityEngine;

public class StoreItem : MonoBehaviour, IMailComponent
{
    [SerializeField] private GameObject _product;
    [SerializeField] private int _cost;

    private int address;

    private void Start()
    {
        address = MailSystem.RegisterNewMail(this);
    }

    public void Buy() 
    {
        if (WalletSystem.TryBuyItem(_cost)) 
        {
            MailSystem.Request(address, GameManager.AltairAddress);
        }
    }

    public GameObject[] TakeRequest()
    {
        return new GameObject[1] { _product };
    }

    public void GetRequest(GameObject[] objects)
    {
        throw new System.NotImplementedException();
    }
}
