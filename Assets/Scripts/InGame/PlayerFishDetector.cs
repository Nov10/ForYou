using System.Collections;
using UnityEngine;

namespace ForYou.GamePlay
{
    [RequireComponent(typeof(Collider2D))]
    public class PlayerFishDetector : MonoBehaviour
    {
        public System.Action<PlayerFish> OnPlayerFishDetected;

        bool _IsDetectingPlayerFish;
        public bool IsDetectingPlayerFish
        {
            get { return _IsDetectingPlayerFish; }
        }


        private void OnTriggerEnter2D(Collider2D collision)
        {
            _IsDetectingPlayerFish = true;
            TriggerEnterOrStay(collision);
        }
        private void OnTriggerStay2D(Collider2D collision)
        {
            _IsDetectingPlayerFish = true;
            TriggerEnterOrStay(collision);
        }
        private void OnTriggerExit2D(Collider2D collision)
        {
            _IsDetectingPlayerFish = false;
        }
        void TriggerEnterOrStay(Collider2D collision)
        {
            if (IsDetecting == false)
                return;
            if (collision.transform.TryGetComponent<PlayerFish>(out var fish))
            {
                OnPlayerFishDetected?.Invoke(fish);
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