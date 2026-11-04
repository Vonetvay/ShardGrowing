using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

[RequireComponent (typeof(ObjectWithInfo))]
public class CrystalItem : MonoBehaviour
{
    private CrystalData _data;

    private ObjectWithInfo _owi;

    [SerializeField] Image[] _qualityStarImages;
    [SerializeField] Sprite _disableStar;
    [SerializeField] Sprite _enableStar;

    private void Awake()
    {
        _owi = GetComponent<ObjectWithInfo>();
    }

    public void SetData(CrystalData data, float amount) 
    {
        _data = data;

        _owi.Info = new List<string>();
        _owi.Info.Add($"{data.crystalName}");
        _owi.Info.Add($"Масса: {(int)(amount * 100f) / 100f}г");
        _owi.Info.Add($"Качество: {_data.quality}/4");
        /*UpdateVisuals();*/
    }

    /*public void UpdateVisuals() 
    {
        for (int i = 0; i < _qualityStarImages.Length; i++) 
        {
            _qualityStarImages[i].sprite = i < _data.quality ? _enableStar : _disableStar;
        }
    }*/
}
