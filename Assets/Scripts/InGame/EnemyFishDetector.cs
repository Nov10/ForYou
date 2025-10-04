using System.Collections;
using UnityEngine;

namespace ForYou.GamePlay
{
    [RequireComponent(typeof(Collider2D))]
    public class EnemyFishDetector : MonoBehaviour
    {
        public System.Action<EnemyFish> OnEnemyFishDetected;
        bool IsDetecting;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (IsDetecting == false)
                return;
            if (collision.transform.TryGetComponent<EnemyFish>(out var fish))
            {
                OnEnemyFishDetected(fish);
            }
        }

        private void OnTriggerStay2D(Collider2D collision)
        {
            if (IsDetecting == false)
                return;
            if (collision.transform.TryGetComponent<EnemyFish>(out var fish))
            {
                OnEnemyFishDetected(fish);
            }
        }

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