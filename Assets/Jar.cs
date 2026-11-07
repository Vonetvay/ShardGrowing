using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Draggable))]
[RequireComponent(typeof(ObjectWithInfo))]
public class Jar : InteractiveObject, IPowderContainer
{
    [Serializable]
    public struct PowderAmount
    {
        public MaterialType Type;
        public float Amount;
    }

    [Header("Settings")]
    [SerializeField] private float _maxAmount = 100f;
    [SerializeField] private float _pourSpeed = 20f;

    [Header("References")]
    [SerializeField] private Image _contentImage;

    [Header("Setup")]
    [SerializeField] private List<PowderAmount> _startingPowders;

    // Теперь храним состояние через единую структуру состава смеси
    private PowderComposition _composition = new PowderComposition(new Dictionary<MaterialType, float>());

    private ObjectWithInfo _owi;

    private void Awake()
    {
        _owi = GetComponent<ObjectWithInfo>();

        // Инициализируем начальную смесь
        var initialDictionary = new Dictionary<MaterialType, float>();
        foreach (var powder in _startingPowders)
        {
            if (initialDictionary.ContainsKey(powder.Type))
                initialDictionary[powder.Type] += powder.Amount;
            else
                initialDictionary.Add(powder.Type, powder.Amount);
        }
        _composition = new PowderComposition(initialDictionary);
    }

    private void Start()
    {
        UpdateVisuals();
        UpdateImageColor(_composition.GetComponents());
    }

    public override void Interact(GameObject draggedObject)
    {
        base.Interact(draggedObject);
        if (draggedObject.TryGetComponent<Jar>(out var handJar))
        {
            handJar.PourValueInto(this);
        }
    }

    public void PourValueInto(IPowderContainer targetContainer)
    {
        float currentTotal = _composition.GetTotalAmount();
        if (currentTotal <= 0) return;

        // 1. Считаем, сколько МАКСИМУМ мы можем отдать в этом кадре
        float desiredPourAmount = Time.deltaTime * _pourSpeed;
        desiredPourAmount = Mathf.Min(desiredPourAmount, currentTotal);

        // 2. Узнаем, сколько МАКСИМУМ может принять целевой контейнер
        float targetSpace = targetContainer.GetAvailableSpace();
        if (targetSpace <= 0) return;

        // Корректируем итоговый объем
        float actualTotalPour = Mathf.Min(desiredPourAmount, targetSpace);
        if (actualTotalPour <= 0) return;

        // 3. Выделяем порцию для пересыпания (изменяя пропорционально текущий состав)
        float remainingRatio = (currentTotal - actualTotalPour) / currentTotal;

        // Создаем смесь для передачи (берем разницу через вычитание остатка)
        PowderComposition portion = _composition.Subtract(actualTotalPour);

        // Передаем порцию в цель
        targetContainer.ReceivePowder(portion);

        // У себя оставляем только то, что должно остаться
        //_composition = _composition.Subtract(actualTotalPour);

        UpdateVisuals();
    }

    public float InstantPourInto(IPowderContainer targetContainer, float minAmount, float maxAmount)
    {
        float currentTotal = _composition.GetTotalAmount();
        if (currentTotal <= 0f) return 0f;
        if (maxAmount <= 0f || minAmount > maxAmount) return 0f;

        // 1. Определяем желаемый объем
        float desiredPour = Mathf.Min(maxAmount, currentTotal);

        // 2. Узнаем доступное место в цели
        float targetSpace = targetContainer.GetAvailableSpace();

        // 3. Физический объем пересыпания
        float actualTotalPour = Mathf.Min(desiredPour, targetSpace);

        // 4. Валидация ограничений минимума
        if (actualTotalPour < minAmount || actualTotalPour <= 0f) return 0f;

        // 5. Отделяем порцию состава для передачи
        PowderComposition portion = _composition.Subtract(actualTotalPour);

        // Передаем в целевой контейнер
        float receivedAmount = targetContainer.ReceivePowder(portion);

        // Уменьшаем свой объем ровно на столько, сколько цель смогла принять
        //_composition = _composition.Subtract(receivedAmount);

        UpdateVisuals();
        return receivedAmount;
    }

    public float ReceivePowder(PowderComposition mix)
    {
        float spaceLeft = GetAvailableSpace();
        if (spaceLeft <= 0) return 0f;

        float mixAmount = mix.GetTotalAmount();
        float amountToAdd = Mathf.Min(mixAmount, spaceLeft);

        // Если пересыпается лишь часть смеси (не влезает полностью), уменьшаем её перед добавлением
        if (amountToAdd < mixAmount)
        {
            mix.Subtract(amountToAdd);
        }

        // Объединяем текущую смесь с входящей порцией
        _composition = _composition.Add(mix);
        UpdateVisuals();
        UpdateImageColor(_composition.GetComponents());

        return amountToAdd;
    }

    private void UpdateVisuals()
    {
        if (_contentImage == null) return;

        float currentTotal = _composition.GetTotalAmount();

        if (_maxAmount > 0)
        {
            _contentImage.fillAmount = currentTotal / _maxAmount;
        }

        _owi.Info = new List<string>();
        _owi.Info.Add($"{(int)(currentTotal * 100f) / 100f}/{_maxAmount}г");

        // Процентное соотношение получаем напрямую через GetComponents() структуры
        var components = _composition.GetComponents();
        foreach (var kvp in components)
        {
            _owi.Info.Add($"{MaterialManager.Instance.GetMaterial(kvp.Key).materialName}  {(int)(kvp.Value * 10000f) / 100f}%");
        }
    }

    private void UpdateImageColor(Dictionary<MaterialType, float> components)
    {
        if (_composition.GetTotalAmount() <= 0)
        {
            _contentImage.color = Color.white;
            return;
        }

        Color finalColor = Color.clear;

        // Используем уже посчитанные фракции компонентов для смешивания цветов
        foreach (var kvp in components)
        {
            if (kvp.Value > 0)
            {
                finalColor += MaterialManager.Instance.GetMaterial(kvp.Key).color * kvp.Value;
            }
        }

        _contentImage.color = finalColor == Color.clear ? Color.white : finalColor;
    }

    // Реализация интерфейса и геттеры
    public float GetTotalAmount() => _composition.GetTotalAmount();
    public float GetAvailableSpace() => Mathf.Max(0f, _maxAmount - _composition.GetTotalAmount());
    public PowderComposition GetComposition() => _composition;

    public PowderComposition GivePowderPortion(float amount)
    {
        if (amount > GetTotalAmount()) return new PowderComposition();

        PowderComposition _passedComposition = _composition.Subtract(amount);
        UpdateVisuals();

        return _passedComposition;
    }
}
