using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI; // Добавлено для работы с Layout-компонентами
using System.Collections.Generic;

public class InfoBox : MonoBehaviour
{
    [SerializeField] private GameObject _body;
    [SerializeField] private GameObject _infoTextPrefab;
    private List<TextMeshProUGUI> _infoList = new List<TextMeshProUGUI>();

    public static Action<string[]> SetInfoBox;
    public static Action RemoveInfoBox;

    private void OnEnable()
    {
        SetInfoBox += SetInfo;
        RemoveInfoBox += RemoveInfo;
    }

    private void OnDisable()
    {
        SetInfoBox -= SetInfo;
        RemoveInfoBox -= RemoveInfo;
    }

    private void SetInfo(string[] data)
    {
        if (data.Length == _infoList.Count)
        {
            // Если количество строк совпадает, просто обновляем текст (это самый быстрый вариант)
            for (int i = 0; i < data.Length; i++)
            {
                _infoList[i].text = data[i];
            }
        }
        else
        {
            RemoveInfo();
            _body.SetActive(true);

            foreach (var d in data)
            {
                var textInstance = Instantiate(_infoTextPrefab, _body.transform).GetComponent<TextMeshProUGUI>();
                textInstance.text = d;
                _infoList.Add(textInstance); // Обязательно сохраняем ссылку в список!
            }
        }

        // Принудительно заставляем Unity UI пересчитать размеры плашки прямо в этом кадре
        ForceUpdateLayout();
    }

    private void RemoveInfo()
    {
        // Удаляем объекты мгновенно, чтобы Layout Group не учитывал их при последующем спавне
        for (int i = _body.transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(_body.transform.GetChild(i).gameObject);
        }

        _infoList.Clear(); // Очищаем список ссылок
        _body.SetActive(false);
    }

    private void ForceUpdateLayout()
    {
        // Заставляем обновиться все холсты
        Canvas.ForceUpdateCanvases();

        // Просим Layout Group и Content Size Fitter на объекте _body мгновенно пересчитать свои размеры
        if (_body.TryGetComponent<LayoutGroup>(out var layout))
        {
            layout.CalculateLayoutInputHorizontal();
            layout.CalculateLayoutInputVertical();
        }
        if (_body.TryGetComponent<ContentSizeFitter>(out var fitter))
        {
            fitter.SetLayoutHorizontal();
            fitter.SetLayoutVertical();
        }
    }
}
