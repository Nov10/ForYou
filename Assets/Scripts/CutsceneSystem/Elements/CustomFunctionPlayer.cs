using UnityEngine;

namespace ForYou.Cutscene
{
    [System.Serializable]
    public class CustomFunctionPlayer : CutsceneElement
    {
        public MonoBehaviour Target;
        public string FunctionName;
        public string EndCheckFunction;
    }
}