using System;
using UnityEngine;

[Serializable]
public struct PowderComponentShare 
{
    public MaterialType Type;
    public Vector2 Share;
}

[CreateAssetMenu(fileName = "new FeedComposition", menuName = "Crystals/FeedComposition")]
public class FeedComposition : ScriptableObject
{
    public PowderComponentShare[] Components;
}
