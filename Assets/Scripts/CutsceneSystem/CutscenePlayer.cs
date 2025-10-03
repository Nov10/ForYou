using ForYou.GamePlay;
using Helpers;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace ForYou.Cutscene
{
    public class CutscenePlayer : MonoBehaviour
    {
        [SerializeField] CutsceneData Data;

        private void Start()
        {
            Play();
        }

        int NowIndex = 0;

        public void Play()
        {
            NowIndex = -1;
            PlayNext();
        }

        private void OnDrawGizmos()
        {
            DrawPlayerFishPath();
        }

        void DrawPlayerFishPath()
        {
            List<Vector2> positions = new();
            foreach(var d in Data.Elements)
            {
                if (d == null)
                    continue;
                if(d.GetType() == typeof(MovePlayerFish))
                {
                    positions.Add(((MovePlayerFish)d).TargetPosition.position);
                }
            }

            for(int i = 0; i < positions.Count - 1; i++)
            {
                Debug.DrawLine(positions[i], positions[i + 1]);
            }
        }


        void PlayNext()
        {
            NowIndex++;
            if (NowIndex >= Data.Elements.Length)
                return;
            StartCoroutine(_PlaySingleElement(Data.Elements[NowIndex], PlayNext));
        }

        public ExtendedStringPlayer PlayerPrefab;
        IEnumerator _PlaySingleElement(CutsceneElement element, System.Action onEnd)
        {
            var type = element.GetType();
            if(type == typeof(Delay))
            {
                if (element.PlayWithNextElement == true)
                {
                    onEnd();
                }
                else
                {
                    yield return new WaitForSeconds(((Delay)element).Duration);
                    onEnd();
                }
            }
            else if(type == typeof(SpeechBubbleText))
            {
                var text = (SpeechBubbleText)element;
                var bubble = Instantiate(text.SpeechBubblePrefab.gameObject, text.Position.transform);
                bubble.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                var player = Instantiate(PlayerPrefab.gameObject, transform).GetComponent<ExtendedStringPlayer>();
                yield return null;
                player.Play(text.Sentence, bubble.GetComponentInChildren<TMP_Text>());

                if (element.PlayWithNextElement == true)
                    onEnd();
                while (player.IsEnd == false)
                {
                    yield return null;
                }
                if (element.PlayWithNextElement == false)
                    onEnd();
                Destroy(bubble.gameObject);
                Destroy(player.gameObject);
            }
            else if(type == typeof(MovePlayerFish))
            {
                var move = (MovePlayerFish)element;
                float allowDistance = move.AllowDistance;
                float slowRadius = move.SlowDistance;

                var target = move.TargetPosition;
                var player = FindFirstObjectByType<PlayerFish>();
                var playerRigidBody = player.GetComponent<Rigidbody2D>();

                player.ChangeControlMode(ControlMode.Cutscene);
                var snap = player.GetSnapping();
                Vector2 diff = (Vector2)target.position - playerRigidBody.position;

                if (element.PlayWithNextElement == true)
                    onEnd();

                while (diff.sqrMagnitude > allowDistance * allowDistance)
                {
                    diff = (Vector2)target.position - playerRigidBody.position;
                    float t = Mathf.Clamp01((diff.magnitude - allowDistance)  / (slowRadius - allowDistance));
                    player.InputDirectionByCutscene = diff.normalized * (t + 0.01f);

                    if(diff.sqrMagnitude <= slowRadius * slowRadius)
                    {
                        player.SetSnapping(Vector2.one * (1-t) * 10f);
                    }
                    yield return new WaitForFixedUpdate();
                }
                player.SetSnapping(snap);
                player.InputDirectionByCutscene = Vector2.zero;

                if (move.AutoReturnToPlayerControlMode == true)
                    player.ChangeControlMode(ControlMode.Self);
                if (element.PlayWithNextElement == false)
                    onEnd();
            }
            else if(type == typeof(ShakeCamera))
            {
                var shake = (ShakeCamera)element;
                shake.Shake();

                if (shake.PlayWithNextElement == true)
                    onEnd();
                else
                {
                    yield return new WaitForSeconds(shake.Duration);
                    onEnd();
                }
            }
        }
    }
}