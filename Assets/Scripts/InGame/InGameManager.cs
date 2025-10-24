using ForYou.Cutscene;
using Helpers;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace ForYou.GamePlay
{
    public class InGameManager : MonoBehaviour
    {
        public static InGameManager Instance { get; private set; }
        private void Awake()
        {
            if(Instance != null)
            {
                Destroy(gameObject);
                return;
            }    
            Instance = this;
        }
        [SerializeField] PlayerFish Player;
        [SerializeField] Anemone Anemone;

        [SerializeField] float Timer = 30.0f;
        [SerializeField] Slider TimeSlider;
        float ElaspedTime;
        public float GetLeftTimeRate()
        {
            return Mathf.Clamp01((ElaspedTime) / Timer);
        }

        public bool IsGameOver { get; private set; } = false;

        [SerializeField] int Score;
        [Header("GameOver By Die")]
        [SerializeField] RectTransform FinalScoreTextContainer;
        [SerializeField] Image FinalScoreBackground;
        [SerializeField] float GameOverFadeTime = 2.0f;
        [SerializeField] TMP_Text GameOverText;
        [Space(10)]
        [SerializeField] Image DieNimo;
        [SerializeField] Vector2 StartPosition;
        [SerializeField] float DieNimoMoveDuration = 1.0f;
        [SerializeField] TMP_Text FinalScoreText;

        [Space(10)]
        [Header("GameOver By Timer")]
        [SerializeField] RectTransform FinalScoreTextContainer_Timer;
        [SerializeField] Image FinalScoreBackground_Timer;
        [SerializeField] float ClearFadeTime = 2.0f;
        [SerializeField] TMP_Text ClearText;
        [Space(10)]
        [SerializeField] Image RotateNimo;
        [SerializeField] Image RotatedAnemone;
        [SerializeField] Vector2 StartOffset;
        [SerializeField] float NimoStartOffsetDuration = 1.0f;
        [SerializeField] float NimoRotateDuration = 3.0f;
        [Space(10)]
        [SerializeField] TMP_Text FinalScoreText_Timer;

        [SerializeField] TMP_Text ScoreText;
        public void GameOver_ByDie()
        {
            GameOver();

            FinalScoreTextContainer.gameObject.SetActive(true);
            FinalScoreTextContainer.transform.localPosition = Vector3.zero;
            FinalScoreText.gameObject.SetActive(false);

            FinalScoreBackground.CrossFadeAlpha(0.0f, 0, true);
            FinalScoreBackground.CrossFadeAlpha(1.0f, GameOverFadeTime, true);

            DelayedFunctionHelper.InvokeDelayed(GameOverFadeTime, () =>
            {
                GameOverText.gameObject.SetActive(true);


                DieNimo.rectTransform.anchoredPosition = StartPosition;
                DieNimo.CrossFadeAlpha(0.0f, 0, true);
                DieNimo.CrossFadeAlpha(1.0f, DieNimoMoveDuration * 0.5f, true);
                ObjectMoveHelper.MoveObject(DieNimo.transform, Vector3.zero, DieNimoMoveDuration, ePosition.Local);

                DelayedFunctionHelper.InvokeDelayed(DieNimoMoveDuration, () =>
                {
                    FinalScoreText.gameObject.SetActive(true);
                    FinalScoreText.text = "최종 점수 : " + CalculateScore().ToString();
                });

            });
        }


        public void GameOver_ByTimer()
        {
            GameOver();
            FinalScoreTextContainer_Timer.gameObject.SetActive(true);
            FinalScoreTextContainer_Timer.transform.localPosition = Vector3.zero;
            FinalScoreText_Timer.gameObject.SetActive(false);
            ClearText.gameObject.SetActive(false);

            FinalScoreBackground_Timer.CrossFadeAlpha(0.0f, 0, true);
            FinalScoreBackground_Timer.CrossFadeAlpha(1.0f, ClearFadeTime, true);

            RotateNimo.gameObject.SetActive(false);
            RotatedAnemone.gameObject.SetActive(false);
            DelayedFunctionHelper.InvokeDelayed(ClearFadeTime, () =>
            {
                ClearText.gameObject.SetActive(true);

                RotatedAnemone.gameObject.SetActive(true);
                RotatedAnemone.CrossFadeAlpha(0.0f, 0, true);
                RotatedAnemone.CrossFadeAlpha(1.0f, 1.0f, true);

                DelayedFunctionHelper.InvokeDelayed(1.0f, () =>
                {
                    RotateNimo.gameObject.SetActive(true);
                    ObjectMoveHelper.MoveObject(RotateNimo.transform, StartOffset, NimoStartOffsetDuration, ePosition.Local);
                    DelayedFunctionHelper.InvokeDelayed(NimoStartOffsetDuration, () =>
                    {
                        RotateNimo.GetComponent<Animator>().Play("Rotate");

                        DelayedFunctionHelper.InvokeDelayed(NimoRotateDuration, () =>
                        {
                            FinalScoreText_Timer.gameObject.SetActive(true);
                            FinalScoreText_Timer.text = "최종 점수 : " + CalculateScore().ToString();
                        });
                    });
                });
            });
        }
        void GameOver()
        {
            IsGameOver = true;
        }
        private void OnEnable()
        {
            LastEatTime = -ComboDuration * 10;
            ElaspedTime = 0.0f;
            FinalScoreTextContainer.gameObject.SetActive(false);

            FinalScoreTextContainer.gameObject.SetActive(false);

            //DelayedFunctionHelper.InvokeDelayed(1.0f, () =>
            //{
            //    GameOver_ByTimer();

            //});
        }
        int EatedPlanktonCount = 0;
        int EatedCount_NormalEnemy_L2 = 0;
        int EatedCount_NormalEnemy_L3 = 0;
        int EatedCount_NormalEnemy_L4 = 0;
        int EatedCount_Bacruda = 0;
        int EatedCount_Squid = 0;
        float LastEatTime;
        [SerializeField] float ComboDuration = 20.0f;
        int NowComboCounter = 1;
        int ComboScore;
        int GetComboScore(int comboCounter)
        {
            return 5 * (comboCounter - 1);
        }
        int GetScoreByAnemoneLevel(int level)
        {
            if (level == 1) return 0;
            if (level == 2) return 300;
            if (level == 3) return 800;
            if (level == 4) return 1500;
            if (level == 5) return 2400;
            return 0;
        }
        public string GetSimplifiedScoreExplainString()
        {
            string result = string.Empty;
            result += CalculateScore().ToString("N0");
            return result;
        }
        public string GetScoreExplainString()
        {
            string result = string.Empty;
            //레벨
            result += "말미잘 레벨 : " + GetScoreByAnemoneLevel(Anemone.GetNowLevel());

            //기본
            result += "\n플랑크톤 : " + 10 * EatedPlanktonCount;
            result += "\n2레벨 물고기 : " + 30 * (EatedCount_NormalEnemy_L2);
            result += "\n3레벨 물고기 : " + 60 * (EatedCount_NormalEnemy_L3);
            result += "\n4레벨 물고기 : " + 90 * (EatedCount_NormalEnemy_L4 + EatedCount_Squid + EatedCount_Bacruda);

            result += "\n콤보 보너스 : " + ComboScore; //콤보

            result += "\n특수 보너스 : " + 10 * (EatedCount_Bacruda + EatedCount_Squid); //특수 보너스
            result += "\n합계 " + CalculateScore().ToString();  
            return result;
        }
        public int CalculateScore()
        {
            int sum = 0;

            //레벨
            sum += GetScoreByAnemoneLevel(Anemone.GetNowLevel());

            //기본
            sum += 10 * EatedPlanktonCount;
            sum += 30 * (EatedCount_NormalEnemy_L2);
            sum += 60 * (EatedCount_NormalEnemy_L3);
            sum += 90 * (EatedCount_NormalEnemy_L4 + EatedCount_Squid + EatedCount_Bacruda);

            sum += ComboScore; //콤보

            sum += 10 * (EatedCount_Bacruda + EatedCount_Squid); //특수 보너스

            return sum;
        }
        public void OnAnemoneEatPlankton(Plankton plankton)
        {
            if (Time.time - LastEatTime <= ComboDuration)
            {
                NowComboCounter++;
                if(NowComboCounter >= 2)
                {
                    NowComboCounter = Mathf.Max(NowComboCounter % 6, 1);
                    ComboScore += GetComboScore(NowComboCounter);
                }
            }
            else
            {
                NowComboCounter = 1;
            }
            LastEatTime = Time.time;

            EatedPlanktonCount++;
        }
        public void OnAnemoneEatEnemyFish(EnemyFish fish)
        {
            if (Time.time - LastEatTime <= ComboDuration)
            {
                NowComboCounter++;
                if (NowComboCounter >= 2)
                {
                    NowComboCounter = Mathf.Max(NowComboCounter % 6, 1);
                    ComboScore += GetComboScore(NowComboCounter);
                }
            }
            else
            {
                NowComboCounter = 1;
            }
            LastEatTime = Time.time;

            if (fish.TryGetComponent<NormalEnemyFish>(out var normal))
            {
                var eatable = fish.GetComponent<EatableByAnemone>();
                if (eatable.LevelThresholdToEathThis == 2)
                {
                    EatedCount_NormalEnemy_L2++;
                }
                else if (eatable.LevelThresholdToEathThis == 3)
                {
                    EatedCount_NormalEnemy_L3++;
                }
                else if (eatable.LevelThresholdToEathThis == 4)
                {
                    EatedCount_NormalEnemy_L4++;
                }
                else
                {
                    //Debug.LogError($"{fish} ??");
                }
            }
            else if(fish.TryGetComponent<Bacruda>(out var bac))
            {
                EatedCount_Bacruda++;
            }
            else if(fish.TryGetComponent<Squid>(out var squid))
            {
                EatedCount_Squid++;
            }
        }

        public bool SimplifiedScoreDisplay = true;
        public bool IncludeComboTimeInScoreDisplay = true;
        private void Update()
        {
            var cutscene = FindFirstObjectByType<CutscenePlayer>();
            if (cutscene != null && cutscene.IsPlaying == false)
            {
                string str = string.Empty;
                if(IncludeComboTimeInScoreDisplay == true)
                {
                    str = (Time.time - LastEatTime <= ComboDuration ? $"콤보 시간({NowComboCounter}) : {Time.time - LastEatTime}\n" : "");
                }

                if (SimplifiedScoreDisplay == true)
                    str += GetSimplifiedScoreExplainString().ToString();
                else
                    str += GetScoreExplainString().ToString();

                ScoreText.text = str;
            }
            else
                ScoreText.text = string.Empty;


            ElaspedTime += Time.deltaTime;
            TimeSlider.value = GetLeftTimeRate();
        }

        [SerializeField] SquidFX SquidBlackFX;
        [SerializeField] JellyFishFX JellyFishBlurFX;
        public void PlaySquidBlackFX()
        {
            SquidBlackFX.Play();
        }

        public void PlayJellyFishBlurFX(float duration)
        {
            JellyFishBlurFX.Play(duration);
        }

        [SerializeField] TutorialMessageDrawer TutorialMessage;
        public void PlayTutorial(GameObject prefab)
        {
            TutorialMessage.Play(prefab);
        }
        public bool IsTutorialMessagePlaying()
        {
            return TutorialMessage.IsEnd;
        }
    }
}