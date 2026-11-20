using System;
using UnityEngine;

[CreateAssetMenu(fileName = "new QuestBody", menuName = "Custom/QuestBody")]
public class QuestBody : ScriptableObject
{
    public string Text;
    public Vector2Int DeadlineRange;
    public Vector2Int RewardRange;
    public CrystalType[] Types;
    public CrystalSize[] Sizes;
    [NonSerialized] public Quest Request;
}
