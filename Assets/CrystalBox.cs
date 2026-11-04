/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrystalBox : InteractiveObject, IPowderContainer
{
    [SerializeField] private float _maxAmount;
    [SerializeField] private float _currentAmount;

    private bool _canInteract = true;
    private float _growingTime = 0f;
    private float _timer = 0f;

    private GameObject _crystal;
    private Material _crystalMaterial;

    [SerializeField] private GameObject _garbageCrystal;

    public void ReceivePowder(float amount)
    {
        throw new System.NotImplementedException();
    }

    public override void Interact(GameObject draggedObject)
    {
        base.Interact(draggedObject);
        if (_canInteract && draggedObject.TryGetComponent<IPowderContainer>(out var powderContainer))
        {
            float amount = powderContainer.GetTotalAmount();
            Dictionary<MaterialType, float> composition = powderContainer.GetPowders();
            foreach (var p in composition.Keys)
            {
                if (_currentAmount + composition[p] > _maxAmount)
                {
                    _currentAmount = _maxAmount;
                }
                else
                {
                    _currentAmount += composition[p];
                    powderContainer.ReceivePowder(p, composition[p]);
                    //Debug.Log($"Текущий amount в кристаллизаторе {_currentAmount}");
                }
            }

            GameObject crystal = null;
            Dictionary<MaterialType, CrystalPropertyData> database = CrystalDatabaseManager.Instance.GetDatabase();
            foreach (var d in database.Keys)
            {
                List<CrystalComponent> conditions = CrystalDatabaseManager.Instance.GetCrystal(d).composition;
                bool match = true;
                foreach (var c in conditions)
                {
                    composition.TryGetValue(c.materialId, out float value);
                    if (value == 0)
                    {
                        continue;

                    }
                    if (!(c.minShare <= value / amount && value / amount <= c.maxShare))
                    {
                        match = false; break;
                    }
                }
                if (match)
                {
                    crystal = CrystalDatabaseManager.Instance.GetCrystal(d).crystalObject;
                    break;
                }
            }
            if (crystal == null) crystal = _garbageCrystal;
            StartGrowing(crystal);
            _canInteract = false;
        }
    }

    private void StartGrowing(GameObject crystal)
    {
        _crystal = Instantiate(crystal, transform.position, Quaternion.identity);
        _crystalMaterial = new Material(Shader.Find("Custom/CrystalCrystalGrow"));
        _crystal.GetComponent<MeshRenderer>().material = _crystalMaterial;
        _growingTime = _currentAmount / 10f;
        _crystal.GetComponent<CrystalNew>().GenerateCrystal(UnityEngine.Random.Range(0f, 1000f), _currentAmount);
        //float area = _crystal.GetComponent<BlueCrystal>().GenerateCrystal(UnityEngine.Random.Range(0f, 1000f));
        //_crystalMaterial.SetFloat("_Size", _growingTime / Mathf.Sqrt(area) / 2f);
    }

    private void Update()
    {
        if (_timer < _growingTime)
        {
            _crystalMaterial.SetFloat("_Progress", _timer / _growingTime);
            _timer += Time.deltaTime;
        }
        else if (_growingTime != 0)
        {
            _growingTime = 0f;
            _currentAmount = 0f;
            _timer = 0f;
            _canInteract = true;
            _crystal.gameObject.layer = LayerMask.NameToLayer("Default");
        }

    }

    public float GetAmount()
    {
        return _currentAmount;
    }

    public float ReceivePowder(MaterialType type, float amount)
    {
        throw new System.NotImplementedException();
    }

    public float GetAmount(MaterialType type)
    {
        throw new System.NotImplementedException();
    }

    public float GetAmountFraction(MaterialType type)
    {
        throw new System.NotImplementedException();
    }

    public float GetTotalAmount()
    {
        throw new System.NotImplementedException();
    }

    public float GetAvailableSpace()
    {
        throw new System.NotImplementedException();
    }

    public Dictionary<MaterialType, float> GetPowders()
    {
        throw new System.NotImplementedException();
    }
}*/