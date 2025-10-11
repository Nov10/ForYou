using System;
using UnityEngine;

namespace ForYou.Cutscene
{
    [Serializable]
    public class ChangeCameraOffset : CutsceneElement
    {
        public Vector2 Offset;
        public float Duration;
        public bool AutoReturn = true;
    }
}