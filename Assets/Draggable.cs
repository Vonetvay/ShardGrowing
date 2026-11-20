using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SaveableEntity))]
[RequireComponent(typeof(SaveableTransform))]
public class Draggable : MonoBehaviour
{
    private DraggableState _state;

    public Rigidbody2D Rigidbody { get; private set; }
    public Collider2D Collider { get; private set; }

    private void Awake()
    {
        Collider = GetComponent<Collider2D>();
        Rigidbody = GetComponent<Rigidbody2D>();
        Rigidbody.gravityScale = 0f;
        Rigidbody.linearDamping = 1f;
        Rigidbody.freezeRotation = true;

        SetState(new DraggableFreeState());
    }

    public bool SetState(DraggableState state)
    {
        if (_state == null || _state.CanTransitionTo(state)) 
        {
            _state = state;
            _state.Init(this);
            _state.Enter();
            return true;
        }

        return false;
    }

    public bool TryStartDragging() 
    {
        return SetState(new DraggableDraggedState());
    }

    public bool TryEndDragging() 
    {
        return SetState(new DraggableFreeState());
    }
}

public class DraggableState 
{
    protected Draggable _draggable;

    public void Init(Draggable draggable) 
    {
        _draggable = draggable;
    }

    public virtual void Enter() 
    {

    }

    public virtual bool CanTransitionTo(DraggableState state) 
    {
        return false;
    }
}

public class DraggableDraggedState : DraggableState 
{
    public override void Enter()
    {
        base.Enter();
        _draggable.Rigidbody.bodyType = RigidbodyType2D.Kinematic;
        _draggable.Collider.enabled = false;
    }

    public override bool CanTransitionTo(DraggableState state)
    {
        return state is DraggableFreeState or DraggableLockedState;
    }
}

public class DraggableFreeState : DraggableState
{
    public override void Enter()
    {
        base.Enter();
        _draggable.Rigidbody.bodyType = RigidbodyType2D.Dynamic;
        _draggable.Collider.enabled = true;
    }

    public override bool CanTransitionTo(DraggableState state)
    {
        return state is DraggableDraggedState or DraggableLockedState;
    }
}

public class DraggableUnlockedState : DraggableState 
{
    public override void Enter()
    {
        _draggable.SetState(new DraggableFreeState());
    }

    public override bool CanTransitionTo(DraggableState state)
    {
        return state is DraggableFreeState;
    }
}

public class DraggableLockedState : DraggableState
{
    public override void Enter()
    {
        base.Enter();
        _draggable.Rigidbody.bodyType = RigidbodyType2D.Static;
        _draggable.Collider.enabled = true;
    }
    public override bool CanTransitionTo(DraggableState state)
    {
        return state is DraggableUnlockedState;
    }
}