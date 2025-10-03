using UnityEngine;

namespace ForYou.GamePlay
{
    public class EatableByAnemone : MonoBehaviour
    {
        [SerializeField] int Gage = 1;
        public int GetGage() { return Gage; }
    }
}