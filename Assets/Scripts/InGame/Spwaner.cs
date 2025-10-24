using Helpers;
using UnityEngine;

namespace ForYou.GamePlay
{
    public class Spwaner : MonoBehaviour
    {
        [SerializeField] GameObject Prefab;
        GameObject NowFish;
        [SerializeField] float ReSpawnDuration = 5.0f;
        float EnemyDiedTime;
        bool IsTimerRunning = false;

        private void OnEnable()
        {
            Spawn();
        }

        private void Update()
        {
            if (IsTimerRunning == false && NowFish == null)
            {
                IsTimerRunning = true;
                EnemyDiedTime = Time.time;
            }


            if (IsTimerRunning == true && Time.time - EnemyDiedTime > ReSpawnDuration)
            {
                IsTimerRunning = false;
                Spawn();
            }
        }

        void Spawn()
        {
            NowFish = Instantiate(Prefab, transform.position, Quaternion.identity);
            DelayedFunctionHelper.InvokeDelayed(0.001f, () =>
            {
                NowFish.gameObject.SetActive(true);
            });
        }
    }
}