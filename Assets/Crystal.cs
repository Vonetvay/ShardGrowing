using NaughtyAttributes;
using System;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using System.Collections.Generic;
using static UnityEngine.Rendering.DebugUI;

/*public enum CrystalState 
{
    Normal,
    Growing,
    Cracked
}*/

public record ShardParams(float3 Position, float4x4 BaseMatrix, float4x4 TransformMatrix, float ZOffset, float Delay, float Duration);

[RequireComponent(typeof(CapsuleCollider2D))]
[RequireComponent(typeof(Draggable))]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(ObjectWithInfo))]
public class Crystal : MonoBehaviour, IInteractiveObject, IInteractWithTerrain, IPowderContainer, ISaveable
{
    [Header("Настройка роста")]
    [SerializeField][Range(0f, 1f)] protected float _animationProgress;
    public float AnimationProgress => _animationProgress;
    [SerializeField] private float _shardAnimationProgress = 0.1f;
    [SerializeField] private int _growthDuration = 5;
    [SerializeField] private float _growthUpAmount;
    [HideInInspector] public float GrowthUpAmountCurrent = 0f;

    [SerializeField] private FeedComposition _feedComposition;

    [SerializeField] private bool _debugMode;
    [ShowIf(nameof(_debugMode))]
    [SerializeField] private float _animationStep = 0.1f;
    
    [Header("Настройка кристалла")]
    [SerializeField] protected int vertexCount = 4;
    [SerializeField] private float _seed;
    [SerializeField] private float _visualScale;

    //[SerializeField] private GrowthRecipe _growthRecipe;

    [Header("Шейдеры")]
    [SerializeField] private Shader _shader;
    [HideInInspector] public Material Material;
    [SerializeField] private Texture2D _crackedTex;
    private Mesh mesh;

    [Header("UI")]

    [SerializeField] public GameObject GrowthIndicator;
    [SerializeField] public Image AmountBar;
    [SerializeField] public GameObject Particles;

    private MeshRenderer _meshRenderer;
    private CapsuleCollider2D _collider;
    [HideInInspector] public Draggable Draggable;
    protected ObjectWithInfo _owi;

    private CrystalState _state = new CrystalStateNormal();
    [HideInInspector] public CrystalState State => _state;

    [HideInInspector] public bool CanInteract = true;

    [HideInInspector] public CrystalType Type { get; protected set; }
    [HideInInspector] public CrystalSize Size { get; private set; }

    public void Interact(GameObject draggedObject)
    {
        if (!CanInteract) return;
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
            SetState(new CrystalStateNormal());
            
            Debug.Log("Кристалл посажен");
        }
    }

    protected virtual void Awake()
    {
        Init();
    }

    private void UpdateObjectInfo() 
    {
        _owi.Info = new List<string>
        {
            $"{Type}",
            $"{Size}",
            _state.GetType().Name
        };
    }

    private CrystalSize ConvertAnimationProgressToSize(float prog) 
    {
        if (prog < 0.3f) return CrystalSize.Small;
        else if (prog < 0.6f) return CrystalSize.Middle;
        else if (prog < 0.95f) return CrystalSize.Big;
        return CrystalSize.Large;
    }

    private void InitMaterial() 
    {
        Destroy(Material);
        Material = new Material(_shader);
        
        _meshRenderer.material = Material;
        Material.SetTexture("_BaseMap", _crackedTex);
    }

    public void TryGrowthUp() 
    {
        switch (_state) 
        {
            case CrystalStateFed:
                SetState(new CrystalStateGrowing());
                GrowthUp();
                break;
            case CrystalStateGrowing:
                SetState(new CrystalStateCracked());
                Debug.Log("Кристалл треснул");
                break;
            default:
                break;
        }
    }

    [Button("Вырасти")]
    private void GrowthUp() 
    {
        float _deltaProgress = (1f - _shardAnimationProgress) / _growthDuration;

        if (_animationProgress + _deltaProgress >= 1f)
        {
            _animationProgress = 1f;
            SetState(new CrystalStateCompleted());
        }
        else 
        {
            _animationProgress += _deltaProgress;
            SetState(new CrystalStateGrowing());
        }
        
        Size = ConvertAnimationProgressToSize(_animationProgress);
        UpdateFeedAmountBar();

        GenerateCrystal(_seed);

        OverlappingNearCrystal();
    }

    private void OverlappingNearCrystal() 
    {
        List<Collider2D> _overlappedColliders = new();

        ContactFilter2D filter = new();
        //filter.SetLayerMask(LayerMask.NameToLayer("Crystal"));
        //filter.useTriggers = false;

        Debug.Log(_collider.Overlap(filter, _overlappedColliders));

        foreach (var item in _overlappedColliders)
        {
            if (item.TryGetComponent<Crystal>(out Crystal crystal)) 
            {
                if (crystal == this) continue;
                if (crystal.State is CrystalStateGrowing) 
                {
                    if (crystal.AnimationProgress > _animationProgress)
                    {
                        SetState(new CrystalStateCracked());
                    }
                    else 
                    {
                        crystal.SetState(new CrystalStateCracked());
                    }
                }
            }
        }
    }

    [Button("Вернуться к состоянию осколка")]
    private void ReturnToShard() 
    {
        _animationProgress = _shardAnimationProgress;
        Size = ConvertAnimationProgressToSize(_animationProgress);
        GenerateCrystal(_seed);
    }

    [ContextMenu("Инициализировать")]
    public virtual void Init() 
    {
        _seed = UnityEngine.Random.Range(0f, 1000f);
        Type = GetMyType();
        Size = CrystalSize.Shard;
        _meshRenderer = GetComponent<MeshRenderer>();
        _collider = GetComponent<CapsuleCollider2D>();
        Draggable = GetComponent<Draggable>();
        _owi = GetComponent<ObjectWithInfo>();
        InitMaterial();
        GenerateCrystal(_seed);
    }

    protected virtual CrystalType GetMyType() => CrystalType.None;

    public float GenerateCrystal(float seed)
    {
        int triangleCount = vertexCount * 3 - 6;
        MeshFilter meshFilter = GetComponent<MeshFilter>();

        if (mesh != null) mesh.Clear();
        mesh = new() { name = "ProceduralCrystal" };

        float currentSeed = seed;
        SeedOperations.RandomNumberFromSeed(ref currentSeed, 0f, 1f);

        ShardParams[] shardsParams = CrystalCalculator(ref currentSeed);
        int count = shardsParams.Length;

        if (count == 0) return 0f;

        int totalVertices = vertexCount * count;
        int totalTriangles = triangleCount * count;

        NativeArray<float3> vertices = new(totalVertices, Allocator.Temp);
        NativeArray<int> triangles = new(totalTriangles, Allocator.Temp);
        NativeArray<float2> uvs = new(totalVertices, Allocator.Temp);

        for (int i = 0; i < count; i++)
        {
            NativeSlice<float3> vertexSlice = new(vertices, i * vertexCount, vertexCount);
            NativeSlice<int> triangleSlice = new(triangles, i * triangleCount, triangleCount);
            NativeSlice<float2> uvSlice = new(uvs, i * vertexCount, vertexCount);

            ShardCalculator(i, vertexCount, vertexSlice, triangleSlice, uvSlice, shardsParams[i]);
        }

        if (_visualScale != 1)
        {
            for (int i = 0; i < vertices.Length; i++) 
            {
                vertices[i] *= _visualScale;
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

        UpdateObjectInfo();

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
        if (!CanInteract) return;

        TerrainType type = TerrainManager.Instance.GetTerrainFromCoord(cx, cy);
        Debug.Log(type.ToString());
        if (type == TerrainType.Field) 
        {
            SetState(new CrystalStateGrowing());
        }
    }

    public float ReceivePowder(PowderComposition mix)
    {
        float addingAmount = mix.GetTotalAmount();
        GrowthUpAmountCurrent += addingAmount;
        if (GrowthUpAmountCurrent >= _growthUpAmount - .01f) 
        {
            SetState(new CrystalStateFed());
        }
        UpdateFeedAmountBar();
        return addingAmount;
    }

    private void UpdateFeedAmountBar() 
    {
        AmountBar.fillAmount = GrowthUpAmountCurrent / _growthUpAmount;
    }

    public float GetAvailableSpace()
    {
        return _growthUpAmount - GrowthUpAmountCurrent;
    }

    public bool CanIGetMixType(PowderComposition mix)
    {
        if (!(_state is CrystalStateGrowing or CrystalStateFed)) return false;
        return mix.Match(_feedComposition);
        //return _growthRecipe.Matches(mix.GetComponentsInShares(), 20f, ProcessType.Crystallization);
    }

    private struct CrystalSaveData
    {
        public float animationProgress;
        public float seed;
        public string stateName;
        public bool canInteractive;
    }

    public string CaptureState()
    {
        var data = new CrystalSaveData
        {
            animationProgress = _animationProgress,
            seed = _seed,
            stateName = _state != null ? _state.GetType().Name : nameof(CrystalStateNormal),
            canInteractive = CanInteract,
        };
        return JsonUtility.ToJson(data);
    }

    public void RestoreState(string jsonState)
    {
        var data = JsonUtility.FromJson<CrystalSaveData>(jsonState);

        _animationProgress = data.animationProgress;
        _seed = data.seed;

        // Воссоздаём нужный класс состояния по имени
        CrystalState loadedState = data.stateName switch
        {
            nameof(CrystalStateNormal) => new CrystalStateNormal(),
            nameof(CrystalStateGrowing) => new CrystalStateGrowing(),
            nameof(CrystalStateFed) => new CrystalStateFed(),
            nameof(CrystalStateCracked) => new CrystalStateCracked(),
            nameof(CrystalStateCompleted) => new CrystalStateCompleted(),
            _ => new CrystalStateNormal()
        };

        // Принудительно применяем состояние без проверки CanTransitionTo
        ForceSetState(loadedState);

        // Восстанавливаем интерактивность поверх состояния, если она менялась отдельно
        CanInteract = data.canInteractive;

        Size = ConvertAnimationProgressToSize(_animationProgress);
        UpdateFeedAmountBar();
        GenerateCrystal(_seed);
    }


    public bool SetState(CrystalState newState)
    {
        if (newState == null || newState.GetType() == _state.GetType()) return false;

        if (_state == null || _state.CanTransitionTo(newState))
        {
            ForceSetState(newState);
            return true;
        }

        return false;
    }

    private void ForceSetState(CrystalState newState)
    {
        _state = newState;
        _state.Init(this);
        _state.Enter();
        UpdateObjectInfo();
    }
}

public abstract class CrystalState
{
    protected Crystal crystal;

    public void Init(Crystal crystal)
    {
        this.crystal = crystal;
    }

    public virtual void Enter()
    {
        
    }

    public virtual bool CanTransitionTo(CrystalState state)
    {
        throw new NotImplementedException();
    }
}

public class CrystalStateNormal : CrystalState 
{
    public override void Enter()
    {
        crystal.GrowthIndicator.SetActive(false);
        crystal.AmountBar.gameObject.SetActive(false);
        crystal.Draggable.SetState(new DraggableUnlockedState());
        crystal.Particles.SetActive(false);
    }

    public override bool CanTransitionTo(CrystalState state)
    {
        return state is CrystalStateGrowing;
    }
}

public class CrystalStateGrowing : CrystalState
{

    public override void Enter()
    {
        crystal.GrowthIndicator.SetActive(true);
        crystal.AmountBar.gameObject.SetActive(true);
        crystal.Draggable.SetState(new DraggableLockedState());
        crystal.GrowthUpAmountCurrent = 0f;
        crystal.Particles.SetActive(false);
    }

    public override bool CanTransitionTo(CrystalState state)
    {
        return state is CrystalStateNormal or CrystalStateFed or CrystalStateCracked or CrystalStateCompleted;
    }
}

public class CrystalStateFed : CrystalState
{
    public override void Enter()
    {
        crystal.CanInteract = false;
        crystal.Particles.SetActive(true);
    }

    public override bool CanTransitionTo(CrystalState state)
    {
        return state is CrystalStateGrowing or CrystalStateNormal or CrystalStateCracked;
    }
}

public class CrystalStateCracked : CrystalState
{

    public override void Enter()
    {
        crystal.Material.SetFloat("_TextureAlpha", 0f);
        crystal.CanInteract = false;
        crystal.Draggable.SetState(new DraggableUnlockedState());
        crystal.CanInteract = false;
        crystal.Particles.SetActive(false);
    }

    public override bool CanTransitionTo(CrystalState state)
    {
        return state is CrystalStateGrowing;
    }
}

public class CrystalStateCompleted : CrystalState 
{
    public override void Enter()
    {
        crystal.GrowthIndicator.SetActive(false);
        crystal.AmountBar.gameObject.SetActive(false);
        crystal.Draggable.SetState(new DraggableUnlockedState());
        crystal.CanInteract = false;
        crystal.Particles.SetActive(true);
    }

    public override bool CanTransitionTo(CrystalState state)
    {
        return false;
    }
}