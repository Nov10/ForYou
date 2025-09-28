
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Helpers
{
    [ExecuteAlways]
    public class DelayedFunctionHelper : MonoBehaviour
    {
        static DelayedFunctionHelper _instance;
        static DelayedFunctionHelper Instance
        {
            get
            {
                if(_instance == null)
                {
                    _instance = FindFirstObjectByType<DelayedFunctionHelper>();
                    if (_instance == null)
                    {
                        GameObject obj = new GameObject("DelayedFunctionHelper");
                        _instance = obj.AddComponent<DelayedFunctionHelper>();
                    }
                }
                if(_instance == null)
                {
                    throw new Exception("Failed to create DelayedFunctionHelper instance.");
                }
                return _instance;
            }
        }
        Dictionary<int, Coroutine> Functions = new Dictionary<int, Coroutine>();
        void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }
        private void Start()
        {
            DontDestroyOnLoad(gameObject);
        }
        System.Random IDGenerator = new System.Random();
        public static int InvokeDelayed(float delay, Action action)
        {
            int id = Instance.IDGenerator.Next(int.MinValue, int.MaxValue);
            Instance.Functions.Add(id, Instance.StartCoroutine(Instance.InvokeAfterDelay(action, delay, id)));
            return id;
        }

        public static void CancelInvoke(int id)
        {
            if (Instance.Functions.TryGetValue(id, out Coroutine coroutine))
            {
                Instance.StopCoroutine(coroutine);
                Instance.Functions.Remove(id);
            }
        }

        IEnumerator InvokeAfterDelay(Action action, float delay, int id)
        {
            yield return new WaitForSeconds(delay);
            action?.Invoke();
            Instance.Functions.Remove(id);
        }
    }
}