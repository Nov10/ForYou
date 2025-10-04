using UnityEngine;

namespace ForYou.GamePlay
{
    public class EatableByAnemone : MonoBehaviour
    {
        [Header("Interation With Anemone")]
        [SerializeField] bool _CanBeEaten = true;
        [SerializeField] int _Gage = 0;
        [SerializeField] int _LevelThresholdToEatThis = 1;
        public bool CanBeEaten => _CanBeEaten;
        public int Gage => _Gage;
        public int LevelThresholdToEathThis => _LevelThresholdToEatThis;
    }
}