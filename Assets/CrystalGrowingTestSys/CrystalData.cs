using UnityEngine;

// 2. Класс-паспорт самого кристалла
[CreateAssetMenu(fileName = "NewCrystal", menuName = "Crystals/Crystal Data")]
public class CrystalData : ScriptableObject
{
    public string crystalName;
    public GameObject visualPrefab; // Как кристалл выглядит в мире
    public Shader shader;
    public float density;
    public int quality;

    public CrystalType type;
    public CrystalSize size;
}
