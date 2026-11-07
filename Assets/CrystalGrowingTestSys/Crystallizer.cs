/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crystallizer : InteractiveObject, IPowderContainer
{
    // Текущее состояние прибора
    [SerializeField] private float minAmount;
    [SerializeField] private float maxAmount;

    public float currentTemperature = 20f;
    private ProcessType machineProcess = ProcessType.Crystallization;

    // Текущий состав внутри в виде структуры смеси
    private PowderComposition inputComposition = new PowderComposition(new Dictionary<MaterialType, float>());
    private float _totalAmount;
    private Material _currentCrystalMaterial;

    private RenderTexture _renderTexture;
    private Material _renderMaterial;
    [SerializeField] private Camera _viewCamera;
    [SerializeField] private SpriteRenderer _viewSpriteRenderer;
    [SerializeField] private GameObject _crystalItemPrefab;
    private GameObject _currentCrystal;
    private CrystalData _crystalToSpawn;

    private LayerMask _defaultLayer;

    public override void Interact(GameObject draggedObject)
    {
        if (!_canInteract) return;
        gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");

        if (draggedObject.TryGetComponent<Jar>(out var handJar))
        {
            // Банка сама рассчитает пропорции, уменьшит себя и передаст смесь через ReceivePowder
            float pouredAmount = handJar.InstantPourInto(this, minAmount, maxAmount);

            // Если что-то успешно пересыпалось — запускаем процесс
            if (pouredAmount > 0f)
            {
                StartProcess();
            }
            else
            {
                // Если пересыпание не удалось (например, не подошло по min/max), возвращаем слой назад
                gameObject.layer = _defaultLayer;
            }
        }
    }

    private void Awake()
    {
        _defaultLayer = gameObject.layer;
        _renderTexture = new RenderTexture(256, 256, 24);

        _renderTexture.format = RenderTextureFormat.ARGB32;
        _renderTexture.filterMode = FilterMode.Bilinear;
        _renderTexture.wrapMode = TextureWrapMode.Clamp;

        _renderTexture.Create();

        _viewCamera.targetTexture = _renderTexture;
        _renderMaterial = new Material(Shader.Find("Custom/SpriteRenderTexture"));
        _renderMaterial.SetTexture("_RTTex", _renderTexture);
        _viewSpriteRenderer.material = _renderMaterial;
    }

    private void OnDestroy()
    {
        if (_renderTexture != null)
        {
            _renderTexture.Release();
            Destroy(_renderTexture);
        }
    }

    // Кнопка "Старт" на приборе
    [ContextMenu("Start Crystallization")]
    public void StartProcess()
    {
        Debug.Log("Запуск процесса кристаллизации...");

        // Отправляем запрос в центральный реестр, получая фракции (компоненты от 0 до 1)
        _crystalToSpawn = RecipeRegistry.Instance.EvaluateMix(
            inputComposition.GetComponents(),
            currentTemperature,
            machineProcess
        );

        if (_crystalToSpawn != null)
        {
            Debug.Log($"Успех! Вырос кристалл: {_crystalToSpawn.crystalName}");
        }
        else
        {
            Debug.Log("Провал: Смесь превратилась в бесполезный шлак (рецепт не найден).");
            _crystalToSpawn = RecipeRegistry.Instance.GetSlag();
        }

        // Очищаем состав прибора для будущих циклов
        inputComposition = new PowderComposition(new Dictionary<MaterialType, float>());

        _currentCrystal = Instantiate(_crystalToSpawn.visualPrefab, transform.position, Quaternion.identity);
        _currentCrystal.layer = LayerMask.NameToLayer("GrowingCrystal");

        _currentCrystalMaterial = new Material(_crystalToSpawn.shader);
        _currentCrystal.GetComponent<MeshRenderer>().material = _currentCrystalMaterial;

        float growingTime = _totalAmount / 10f;
        _currentCrystal.GetComponent<CrystalNew>().GenerateCrystal(UnityEngine.Random.Range(0f, 1000f), _totalAmount);
        Debug.Log($"Total Amount {_totalAmount}");

        StartCoroutine(CrystalGrowingWaiting(growingTime));
    }


    private IEnumerator CrystalGrowingWaiting(float duration) 
    {
        float elapsed = 0f;

        while (elapsed < duration) 
        {
            _currentCrystalMaterial.SetFloat("_Progress", elapsed / duration);

            elapsed += Time.deltaTime;

            yield return null;
        }

        _currentCrystalMaterial.SetFloat("_Progress", 1f);

        GameObject crystalItem = Instantiate(_crystalItemPrefab, transform.position, Quaternion.identity);
        crystalItem.GetComponent<SpriteRenderer>().sprite = RenderTextureToSpriteConverter.ConvertRTToSprite(_renderTexture);
        crystalItem.GetComponent<CircleCollider2D>().radius = _crystalToSpawn.density != 0 ? Mathf.Sqrt(_totalAmount / _crystalToSpawn.density / Mathf.PI) : 1f;
        crystalItem.GetComponent<CrystalItem>().SetData(_crystalToSpawn, _totalAmount);
        Destroy(_currentCrystal);
        _totalAmount = 0f;
        yield return new WaitUntil(() => crystalItem.GetComponent<Draggable>().Dragged);
        gameObject.layer = _defaultLayer;
    }

    public float ReceivePowder(PowderComposition mix)
    {
        float spaceLeft = GetAvailableSpace();
        if (spaceLeft <= 0) return 0f;

        float mixAmount = mix.GetTotalAmount();
        float amountToAdd = Mathf.Min(mixAmount, spaceLeft);

        // Если пришло больше, чем может вместиться (на всякий случай, согласно контракту)
        if (amountToAdd < mixAmount)
        {
            mix = mix.Subtract(mixAmount - amountToAdd);
        }

        // Запоминаем состав смеси и её вес
        inputComposition = mix;
        _totalAmount = inputComposition.GetTotalAmount();

        return amountToAdd;
    }

    public float GetTotalAmount() => _totalAmount;

    public float GetAvailableSpace() => Mathf.Max(0f, maxAmount - _totalAmount);

    public PowderComposition GetComposition() => inputComposition;
}
*/