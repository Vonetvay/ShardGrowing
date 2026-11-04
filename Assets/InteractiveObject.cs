using UnityEngine;
using System.Collections.Generic;

public class InteractiveObject : MonoBehaviour
{
    protected bool _canInteract = true;
    public virtual void Interact(GameObject draggedObject) 
    {
        if (!_canInteract) return;
    }
}
