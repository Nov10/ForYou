using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace ForYou.GamePlay
{
    public class JellyFishFX : MonoBehaviour
    {
        Volume FXVolume;
        Coroutine Player;
        private void Awake()
        {
            FXVolume = GetComponent<Volume>();
            FXVolume.enabled = false;
        }
        public void Play(float duration, float intensity)
        {
            FXVolume.enabled = true;
            if (Player != null)
                StopCoroutine(Player);
            Player = StartCoroutine(_Play(duration, intensity));
        }

        IEnumerator _Play(float duration, float intensity)
        {
            float targetValue = intensity;
            float elapsed = 0.0f;
            float reachThreshold = 0.05f;
            float snapping = 30.0f;
            while(elapsed < duration)
            {
                elapsed += Time.deltaTime;
                FXVolume.weight = Mathf.Lerp(FXVolume.weight, targetValue, Time.deltaTime * snapping);

                if(elapsed > duration * 0.8f)
                {
                    targetValue = 0.0f;
                    snapping = 5.0f;
                }
                else if(Mathf.Abs(FXVolume.weight - targetValue) < reachThreshold)
                {
                    targetValue = UnityEngine.Random.Range(intensity * 0.8f, intensity * 1.0f);
                }

                yield return null;
            }
        }
    }
}