using TMPro;
using UnityEngine;

public class WalletSystem : MonoBehaviour
{
    [SerializeField] private int _startingSum;
    [SerializeField] private TextMeshProUGUI _walletVisual;
    private static TextMeshProUGUI s_walletVisual;

    public static int Wallet { get; private set; }

    private void Awake()
    {
        s_walletVisual = _walletVisual;
        Wallet = _startingSum;
    }

    public static void GetReward(int count) 
    {
        Wallet += count;
        UpdateVisual();
    }

    public static bool TryBuyItem(int cost) 
    {
        if (Wallet < cost) 
        {
            return false;
        }
        Wallet -= cost;
        UpdateVisual();
        return true;
    }

    private static void UpdateVisual() 
    {
        s_walletVisual.text = Wallet.ToString();
    }
}
