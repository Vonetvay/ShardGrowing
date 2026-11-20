using UnityEngine;

public static class GameManager
{
    public static int AltairAddress = 0;

    private static string[] s_personNames = {
        "William", "Nikita", "Ebub", "Gray magle", "Edward",
        "Bro", "54 person", "Stranger person", "Magica magle"
    };

    public static string GetRandomPersonName() 
    {
        return s_personNames[Random.Range(0, s_personNames.Length)];
    }
}
