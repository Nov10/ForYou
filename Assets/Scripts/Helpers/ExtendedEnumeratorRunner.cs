using System.Collections;
using UnityEngine;
using System.Collections.Generic;

namespace Helpers
{
    public class ExtendedEnumeratorRunner : MonoBehaviour
    {
        static ExtendedEnumeratorRunner _InternalRunner;
        public static ExtendedEnumeratorRunner Instance
        {
            get
            {
                if (_InternalRunner == null)
                {
                    _InternalRunner = UnityEngine.Object.FindFirstObjectByType<ExtendedEnumeratorRunner>();
                    if (_InternalRunner != null)
                        return _InternalRunner;

                    GameObject go = new GameObject("ExtendedEnumeratorRunner");
                    ExtendedEnumeratorRunner runner = go.AddComponent<ExtendedEnumeratorRunner>();
                    _InternalRunner = runner;
                }
                return _InternalRunner;
            }
        }



        Dictionary<int, Coroutine> Coroutines = new Dictionary<int, Coroutine>();
#if UNITY_EDITOR
        [SerializeField] List<int> IDs = new List<int>();
#endif
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        public void Run(ExtendedEnumerator coroutineInformation)
        {
            Coroutine coroutine = StartCoroutine(_Run(coroutineInformation.Enumerator, coroutineInformation.ID));
            Coroutines.Add(coroutineInformation.ID, coroutine);
#if UNITY_EDITOR
            IDs.Add(coroutineInformation.ID);
#endif
        }

        IEnumerator _Run(IEnumerator enumerator, int id)
        {
            while (enumerator.MoveNext())
                yield return enumerator.Current;

            Stop(id);
        }

        public bool IsPlaying(int id)
        {
            return Coroutines.ContainsKey(id);
        }
        public bool Stop(int id)
        {
#if UNITY_EDITOR
            IDs.Remove(id);
#endif
            if (Coroutines.TryGetValue(id, out Coroutine coroutine))
            {
                StopCoroutine(coroutine);
                Coroutines.Remove(id);
                return true;
            }
            return false;
        }
    }
}