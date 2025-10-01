using UnityEngine;

namespace ForYou.Cutscene
{
    public class CutsceneData : MonoBehaviour
    {
        [SerializeReference, SubclassSelector] public CutsceneElement[] Elements;
    }
}