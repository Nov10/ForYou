using System.Collections;
using UnityEngine;

namespace ForYou.GamePlay
{
    [RequireComponent(typeof(Collider2D))]
    public class PlanktonDetector : MonoBehaviour
    {
        public System.Action<Plankton> OnPlanktonDetected;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            TriggerEnterOrStay(collision);
        }
        private void OnTriggerStay2D(Collider2D collision)
        {
            TriggerEnterOrStay(collision);
        }

        void TriggerEnterOrStay(Collider2D collision)
        {
            if (IsDetecting == false)
                return;
            if (collision.transform.TryGetComponent<Plankton>(out var plankton))
            {
                OnPlanktonDetected?.Invoke(plankton);
            }
        }


        bool IsDetecting = false;

        public void StartDetect()
        {
            IsDetecting = true;
        }
        public void EndDetect()
        {
            IsDetecting = false;
        }

        public void DetectOneFrame()
        {
            StartDetect();
            StartCoroutine(_EndDetect());
        }
        IEnumerator _EndDetect()
        {
            yield return null;
            yield return new WaitForFixedUpdate();
            EndDetect();
        }
    }
}