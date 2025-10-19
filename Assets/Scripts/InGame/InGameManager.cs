using ForYou.Cutscene;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

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

        public bool IsGameOver { get; private set; } = false;

        [SerializeField] int Score;
        [SerializeField] TMP_Text ScoreText;
        [SerializeField] RectTransform FinalScoreTextContainer;
        [SerializeField] TMP_Text FinalScoreText;


        public void GameOver()
        {
            IsGameOver = true;

            FinalScoreTextContainer.gameObject.SetActive(true);
            FinalScoreText.text = "���� ���� : " + CalculateScore().ToString();
        }
        private void OnEnable()
        {
            LastEatTime = -ComboDuration * 10;
            FinalScoreTextContainer.gameObject.SetActive(false);
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
            result += "\n���� : " + CalculateScore().ToString();
            return result;
        }
        public string GetScoreExplainString()
        {
            string result = string.Empty;
            //����
            result += "������ ���� : " + GetScoreByAnemoneLevel(Anemone.GetNowLevel());

            //�⺻
            result += "\n�ö�ũ�� : " + 10 * EatedPlanktonCount;
            result += "\n2���� ����� : " + 30 * (EatedCount_NormalEnemy_L2);
            result += "\n3���� ����� : " + 60 * (EatedCount_NormalEnemy_L3);
            result += "\n4���� ����� : " + 90 * (EatedCount_NormalEnemy_L4 + EatedCount_Squid + EatedCount_Bacruda);

            result += "\n�޺� ���ʽ� : " + ComboScore; //�޺�

            result += "\nƯ�� ���ʽ� : " + 10 * (EatedCount_Bacruda + EatedCount_Squid); //Ư�� ���ʽ�
            result += "\n�հ� " + CalculateScore().ToString();  
            return result;
        }
        public int CalculateScore()
        {
            int sum = 0;

            //����
            sum += GetScoreByAnemoneLevel(Anemone.GetNowLevel());

            //�⺻
            sum += 10 * EatedPlanktonCount;
            sum += 30 * (EatedCount_NormalEnemy_L2);
            sum += 60 * (EatedCount_NormalEnemy_L3);
            sum += 90 * (EatedCount_NormalEnemy_L4 + EatedCount_Squid + EatedCount_Bacruda);

            sum += ComboScore; //�޺�

            sum += 10 * (EatedCount_Bacruda + EatedCount_Squid); //Ư�� ���ʽ�

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
                    Debug.LogError($"{fish} ??");
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
                    str = (Time.time - LastEatTime <= ComboDuration ? $"�޺� �ð�({NowComboCounter}) : {Time.time - LastEatTime}\n" : "");
                }

                if (SimplifiedScoreDisplay == true)
                    str += GetSimplifiedScoreExplainString().ToString();
                else
                    str += GetScoreExplainString().ToString();

                ScoreText.text = str;
            }
            else
                ScoreText.text = string.Empty;
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