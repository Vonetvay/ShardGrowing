using System;
using UnityEngine;

[RequireComponent(typeof(SaveableEntity))]
public class SaveableTransform : MonoBehaviour, ISaveable
{
    [Serializable]
    private struct TransformData
    {
        public Vector3 position;
        public Quaternion rotation;
        public bool isActive;
    }

    public string CaptureState()
    {
        var data = new TransformData
        {
            position = transform.position,
            rotation = transform.rotation,
            isActive = gameObject.activeSelf
        };
        return JsonUtility.ToJson(data);
    }

    public void RestoreState(string jsonState)
    {
        var data = JsonUtility.FromJson<TransformData>(jsonState);

        // Если у объекта есть CharacterController, его нужно временно отключить при телепортации
        var cc = GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        transform.SetPositionAndRotation(data.position, data.rotation);
        gameObject.SetActive(data.isActive);

        if (cc != null) cc.enabled = true;
    }
}
