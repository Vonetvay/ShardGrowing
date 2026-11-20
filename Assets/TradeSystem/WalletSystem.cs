public static class WalletSystem
{
    public static int Wallet { get; private set; }

    public static void GetReward(int count) 
    {
        Wallet += count;
    }

    public static bool TryBuyItem(int cost) 
    {
        if (Wallet < cost) 
        {
            return false;
        }
        Wallet -= cost;
        return true;
    }
}
