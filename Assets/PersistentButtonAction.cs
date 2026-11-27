using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class PersistentInspectorButton : MonoBehaviour
{
    private struct SavedCall
    {
        public Object target;
        public string methodName;
        public object[] args;
    }

    private static Dictionary<string, List<SavedCall>> savedButtons = new Dictionary<string, List<SavedCall>>();

    private void Start()
    {
        Button button = GetComponent<Button>();
        string key = gameObject.name;

        int count = button.onClick.GetPersistentEventCount();
        bool hasAliveTargets = false;

        for (int i = 0; i < count; i++)
        {
            if (button.onClick.GetPersistentTarget(i) != null)
            {
                hasAliveTargets = true;
                break;
            }
        }

        // 1. Если ссылки в инспекторе живые — сохраняем их
        if (hasAliveTargets)
        {
            var list = new List<SavedCall>();

            for (int i = 0; i < count; i++)
            {
                Object target = button.onClick.GetPersistentTarget(i);
                string methodName = button.onClick.GetPersistentMethodName(i);

                if (target != null && !string.IsNullOrEmpty(methodName))
                {
                    object[] args = ExtractArguments(button.onClick, i, target, methodName);
                    list.Add(new SavedCall { target = target, methodName = methodName, args = args });
                }
            }

            savedButtons[key] = list;
            return;
        }

        // 2. Если ссылки стали Missing (null) после перезагрузки — восстанавливаем
        if (savedButtons.TryGetValue(key, out var savedList))
        {
            foreach (var call in savedList)
            {
                if (call.target == null) continue;

                MethodInfo method = call.target.GetType().GetMethod(
                    call.methodName,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                );

                if (method != null)
                {
                    // Копируем в локальные переменные для замыкания
                    Object targetCopy = call.target;
                    object[] argsCopy = call.args;

                    button.onClick.AddListener(() => method.Invoke(targetCopy, argsCopy));
                }
            }
        }
    }

    // Достаём аргумент, если метод в инспекторе вызывается с параметром (int, float, string, bool)
    private object[] ExtractArguments(UnityEngine.Events.UnityEventBase unityEvent, int index, Object target, string methodName)
    {
        MethodInfo method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (method == null) return null;

        ParameterInfo[] parameters = method.GetParameters();
        if (parameters.Length == 0) return null;

        // Достаём скрытое поле m_PersistentCalls из UnityEvent для чтения аргумента
        FieldInfo callsField = typeof(UnityEngine.Events.UnityEventBase).GetField("m_PersistentCalls", BindingFlags.Instance | BindingFlags.NonPublic);
        object persistentCalls = callsField?.GetValue(unityEvent);
        MethodInfo getListener = persistentCalls?.GetType().GetMethod("GetListener", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        object listener = getListener?.Invoke(persistentCalls, new object[] { index });

        FieldInfo argsField = listener?.GetType().GetField("m_Arguments", BindingFlags.Instance | BindingFlags.NonPublic);
        object argsCache = argsField?.GetValue(listener);
        if (argsCache == null) return null;

        System.Type paramType = parameters[0].ParameterType;
        if (paramType == typeof(int))
            return new object[] { argsCache.GetType().GetField("m_IntArgument", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(argsCache) };
        if (paramType == typeof(float))
            return new object[] { argsCache.GetType().GetField("m_FloatArgument", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(argsCache) };
        if (paramType == typeof(string))
            return new object[] { argsCache.GetType().GetField("m_StringArgument", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(argsCache) };
        if (paramType == typeof(bool))
            return new object[] { argsCache.GetType().GetField("m_BoolArgument", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(argsCache) };

        return null;
    }
}
