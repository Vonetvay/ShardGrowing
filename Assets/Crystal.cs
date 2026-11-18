using NaughtyAttributes;
using System;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using System.Collections.Generic;
using static UnityEngine.Rendering.DebugUI;

public record ShardParams(float3 Position, float4x4 BaseMatrix, float4x4 TransformMatrix, float ZOffset, float Delay, float Duration);

[RequireComponent(typeof(CapsuleCollider2D))]
[RequireComponent(typeof(Draggable))]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Crystal : MonoBehaviour, IInteractiveObject, IInteractWithTerrain, IPowderContainer
{
    [Header("Настройка анимации")]
    [SerializeField][Range(0f, 1f)] protected float _animationProgress;
    public float AnimationProgress => _animationProgress;
    [SerializeField] private float _animationStep = 0.1f;

    [SerializeField] private int _growthDuration = 5;
    [SerializeField] private float _shardAnimationProgress = 0.1f;

    [Header("Настройка кристалла")]
    [SerializeField] protected int vertexCount = 4;
    [SerializeField] private float _seed;
    [SerializeField] private float _size;
    [SerializeField] private float _density;

    [SerializeField] private GrowthRecipe _growthRecipe;
    [SerializeField] private float _growthUpAmount;
    private float _growthUpAmountCurrent = 0f;
    [SerializeField] private Image _amountBar;

    [Header("Шейдеры")]
    [SerializeField] private Shader _shader;
    [SerializeField] private Material _material;
    [SerializeField] private Texture2D _crackedTex;
    private MeshRenderer _meshRenderer;

    private CapsuleCollider2D _collider;
    private Draggable _draggable;

    private bool _imNeedToFeed = false;

    private bool _growing = false;
    public bool Growing => _growing;

    private bool _canInteract = true;

    [SerializeField] private GameObject _growthIndicator;

    public void Interact(GameObject draggedObject)
    {
        if (!_canInteract) return;
        /*if (draggedObject.TryGetComponent<IPowderContainerDynamic>(out IPowderContainerDynamic handContainer))
        {
            PowderComposition givenComposition = handContainer.GivePowderPortion(_growthUpAmount);
            if (givenComposition.GetTotalAmount() <= 0f) return;

            bool match = _growthRecipe.Matches(givenComposition.GetComponents(), 20f, ProcessType.Crystallization);
            _canInteract = false;

            _imNeedToFeed = !match;

            string matchString = match ? "подошла" : "не подошла";
            Debug.Log($"Кристалл покормлен, смесь {matchString}");
        }*/
        else if (draggedObject.TryGetComponent<Shovel>(out Shovel shovel))
        {
            OnGrowthEnd();
            
            Debug.Log("Кристалл посажен");
        }
    }

    public void OnValidate()
    {
        _animationProgress = Mathf.Round(_animationProgress / _animationStep) * _animationStep;
        GenerateCrystal(_seed);
    }

    private void Awake()
    {
        Init();
    }

    private void InitMaterial() 
    {
        _material = new Material(_shader);
        _meshRenderer.material = _material;
        _material.SetTexture("_BaseMap", _crackedTex);
    }

    private void SetNormalState() 
    {
        _material.SetFloat("_TextureAlpha", 1f);
    }

    public void SetCrackedState() 
    {
        _material.SetFloat("_TextureAlpha", 0f);
        _canInteract = false;
        _draggable.SetState(new DraggableUnlockedState());
        OnGrowthEnd();
    }

    public void TryGrowthUp() 
    {
        if (!_growing) return;
        if (!_imNeedToFeed)
        {
            SetNormalState();
            GrowthUp();
        }
        else 
        {
            SetCrackedState();
            Debug.Log("Кристалл треснул");
        }
    }

    [Button("Вырасти")]
    private void GrowthUp() 
    {
        if (_animationProgress == 1f) return;

        _animationProgress += (1f - _shardAnimationProgress) / _growthDuration;
        
        if (_animationProgress >= 1f) _animationProgress = 1f;

        _imNeedToFeed = true;
        _canInteract = true;
        _growthUpAmountCurrent = 0f;
        UpdateFeedAmountBar();

        GenerateCrystal(_seed);

        OverlappingNearCrystal();
    }

    private void OverlappingNearCrystal() 
    {
        List<Collider2D> _overlappedColliders = new List<Collider2D>();

        ContactFilter2D filter = new ContactFilter2D();
        //filter.SetLayerMask(LayerMask.NameToLayer("Crystal"));
        //filter.useTriggers = false;

        Debug.Log(_collider.Overlap(filter, _overlappedColliders));

        foreach (var item in _overlappedColliders)
        {
            if (item.TryGetComponent<Crystal>(out Crystal crystal)) 
            {
                if (crystal.Growing) 
                {
                    if (crystal.AnimationProgress > _animationProgress)
                    {
                        SetCrackedState();
                    }
                    else 
                    {
                        crystal.SetCrackedState();
                    }
                }
            }
        }
    }

    protected virtual void OnGrowthStart() 
    {
        _growing = true;
        _imNeedToFeed = true;
        _growthIndicator.SetActive(true);
        _amountBar.gameObject.SetActive(true);
        _draggable.SetState(new DraggableLockedState());
    }

    protected virtual void OnGrowthEnd() 
    {
        _growing = false;
        _growthIndicator.SetActive(false);
        _amountBar.gameObject.SetActive(false);
        _draggable.SetState(new DraggableUnlockedState());
    }

    [Button("Вернуться к состоянию осколка")]
    private void ReturnToShard() 
    {
        _animationProgress = _shardAnimationProgress;
        GenerateCrystal(_seed);
    }

    [ContextMenu("Инициализировать")]
    public void Init() 
    {
        _meshRenderer = GetComponent<MeshRenderer>();
        _collider = GetComponent<CapsuleCollider2D>();
        _draggable = GetComponent<Draggable>();
        InitMaterial();
    }

    public float GenerateCrystal(float seed)
    {
        _collider = GetComponent<CapsuleCollider2D>();

        int triangleCount = vertexCount * 3 - 6;
        MeshFilter meshFilter = GetComponent<MeshFilter>();

        Mesh mesh = new Mesh { name = "ProceduralCrystal" };

        float currentSeed = seed;
        SeedOperations.RandomNumberFromSeed(ref currentSeed, 0f, 1f);

        ShardParams[] shardsParams = CrystalCalculator(ref currentSeed);
        int count = shardsParams.Length;

        if (count == 0) return 0f;

        int totalVertices = vertexCount * count;
        int totalTriangles = triangleCount * count;

        NativeArray<float3> vertices = new NativeArray<float3>(totalVertices, Allocator.Temp);
        NativeArray<int> triangles = new NativeArray<int>(totalTriangles, Allocator.Temp);
        NativeArray<float2> uvs = new NativeArray<float2>(totalVertices, Allocator.Temp);

        for (int i = 0; i < count; i++)
        {
            NativeSlice<float3> vertexSlice = new NativeSlice<float3>(vertices, i * vertexCount, vertexCount);
            NativeSlice<int> triangleSlice = new NativeSlice<int>(triangles, i * triangleCount, triangleCount);
            NativeSlice<float2> uvSlice = new NativeSlice<float2>(uvs, i * vertexCount, vertexCount);

            ShardCalculator(i, vertexCount, vertexSlice, triangleSlice, uvSlice, shardsParams[i]);
        }

        if (_size > 1)
        {
            for (int i = 0; i < vertices.Length; i++) 
            {
                vertices[i] *= _size;
            }
        }

        mesh.SetVertices(vertices);

        mesh.SetIndexBufferParams(totalTriangles, IndexFormat.UInt32);
        mesh.SetIndexBufferData(triangles, 0, 0, totalTriangles, MeshUpdateFlags.DontValidateIndices);

        // Субмеш обязателен при использовании SetIndexBufferData
        mesh.subMeshCount = 1;
        mesh.SetSubMesh(0, new SubMeshDescriptor(0, totalTriangles));

        mesh.SetUVs(0, uvs);

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        meshFilter.mesh = mesh;

        Bounds bounds = mesh.bounds;
        Vector3 size = bounds.size;
        Vector3 offset = bounds.center;

        _collider.offset = new Vector2(offset.x, offset.y);
        _collider.direction = size.x / size.y >= 1 ? CapsuleDirection2D.Horizontal : CapsuleDirection2D.Vertical;
        _collider.size = new Vector2(size.x, size.y);        

        vertices.Dispose();
        triangles.Dispose();
        uvs.Dispose();

        return 0f;
    }

    public struct SortVerticesByX : System.Collections.Generic.IComparer<float3>
    {
        public int Compare(float3 a, float3 b)
        {
            return a.x.CompareTo(b.x);
        }
    }

    public struct SortVerticesByY : System.Collections.Generic.IComparer<float3>
    {
        public int Compare(float3 a, float3 b)
        {
            return a.x.CompareTo(b.x);
        }
    }

    protected virtual ShardParams[] CrystalCalculator(ref float seed) => Array.Empty<ShardParams>();

    protected virtual void ShardCalculator(int index, int vertexCount, NativeSlice<float3> vertices, NativeSlice<int> triangles, NativeSlice<float2> uvs, ShardParams parameters)
    {
        
    }

    protected virtual void LocalShardAnimate(float progress, NativeSlice<float3> localVertices, float3 animationPivot, ShardParams parameters) 
    {

    }

    public void TerrainInteract(TerrainManager terrain, int cx, int cy)
    {
        if (!_canInteract) return;

        TerrainType type = TerrainManager.Instance.GetTerrainFromCoord(cx, cy);
        Debug.Log(type.ToString());
        if (type == TerrainType.Field) 
        {
            OnGrowthStart();
        }
    }

    public float ReceivePowder(PowderComposition mix)
    {
        float addingAmount = mix.GetTotalAmount();
        _growthUpAmountCurrent += addingAmount;
        if (_growthUpAmountCurrent >= _growthUpAmount - .01f) 
        {
            _imNeedToFeed = false;
            _canInteract = false;
        }
        UpdateFeedAmountBar();
        return addingAmount;
    }

    private void UpdateFeedAmountBar() 
    {
        _amountBar.fillAmount = _growthUpAmountCurrent / _growthUpAmount;
    }

    public float GetAvailableSpace()
    {
        return _growthUpAmount - _growthUpAmountCurrent;
    }

    public bool CanIGetMixType(PowderComposition mix)
    {
        if (!_canInteract || !_imNeedToFeed) return false;
        return _growthRecipe.Matches(mix.GetComponentsInShares(), 20f, ProcessType.Crystallization);
    }
}
