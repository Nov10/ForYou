using ForYou.GamePlay;
using Helpers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Anemone : MonoBehaviour
{
    [SerializeField] int Gage;
    [SerializeField] int[] GageThresholdsForLevelUp;
    [SerializeField] TMP_Text GageText;
    [SerializeField] Slider GageSlider;

    [SerializeField] EnemyFishDetector AttackOrEatRange;

    Animator ThisAnimator;

    public int GetNowLevel()
    {
        int gage = Gage;
        for(int i = 0; i < GageThresholdsForLevelUp.Length; i++)
        {
            gage -= GageThresholdsForLevelUp[i];
            if(gage < 0)
            {
                return i + 1;
            }
        }
        return GageThresholdsForLevelUp.Length + 1;
    }

    int GetGageThreshold(int level)
    {
        return GageThresholdsForLevelUp[level - 1];
    }
    public int GetNetGage()
    {
        int gage = Gage;
        for (int i = 0; i < GageThresholdsForLevelUp.Length; i++)
        {
            gage -= GageThresholdsForLevelUp[i];
            if (gage < 0)
            {
                return GageThresholdsForLevelUp[i] + gage;
            }
        }
        return -1;
    }

    float BaseSize;
    [SerializeField] float SizePerLevel;

    [SerializeField] PlayerFishDetector Detector;

    private void Start()
    {
        if (Detector == null)
            Detector = GetComponent<PlayerFishDetector>();

        Detector.OnPlayerFishDetected += OnPlayerFishDetected;
        Detector.StartDetect();

        AttackOrEatRange.OnEnemyFishDetected += OnEnemyFishDetected;
        AttackOrEatRange.StartDetect();

        ThisAnimator = GetComponent<Animator>();

        BaseSize = transform.localScale.x;
        UpGage(0);
    }
    public void SetActiveGageSlider(bool active)
    {
        GageSlider.gameObject.SetActive(active);
    }
    public void UpGage(int delta)
    {
        Gage += delta;
        SetSizeByLevel();
        GageText.text = $"{GetNetGage()} / {GetGageThreshold(GetNowLevel())}";
        GageSlider.value = GetNetGage() / (float)GetGageThreshold(GetNowLevel());
    }

    public int GetGage()
    {
        return (int)Gage;
    }

    void SetSizeByLevel()
    {
        float size = CalculateSizeByLevel(GetNowLevel());
        transform.localScale = new Vector3(size, size, 1);
    }

    float CalculateSizeByLevel(int level)
    {
        return BaseSize + level * SizePerLevel;
    }


    public void OnPlayerFishDetected(PlayerFish fish)
    {
        if(fish.DoesHavePlankton == true)
        {
            var plankton = fish.GetPlankton();
            fish.DropPlankton();

            UpGage(plankton.GetComponent<EatableByAnemone>().Gage);
            InGameManager.Instance.OnAnemoneEatPlankton(plankton);

            Destroy(plankton.gameObject);
            ThisAnimator.Play("Eat");
        }
        else
        {

        }
    }

    public void OnEnemyFishDetected(EnemyFish fish)
    {
        var data = fish.GetComponent<EatableByAnemone>();
        var levelThreshold = data.LevelThresholdToEathThis;

        if(levelThreshold <= GetNowLevel() && data.CanBeEaten)
        {
            ThisAnimator.Play("Eat");
            //¸Ô±â
            var gage = data.Gage;
            UpGage(gage);
            InGameManager.Instance.OnAnemoneEatEnemyFish(fish);
            Destroy(fish.gameObject);
        }
        else
        {
            ThisAnimator.Play("Attack");
            //ÂÑ¾Æ³»±â
            fish.OnAttackedByAnemone(this);

            if(Detector.IsDetectingPlayerFish)
            {
                var player = FindFirstObjectByType<PlayerFish>();
                DelayedFunctionHelper.InvokeDelayed(1.0f, () =>
                {
                    player.AddForceByAnemone(Vector2.up * ForcePower, ForceDuration);
                });
            }
        }
    }

    [SerializeField] float ForcePower = 10;
    [SerializeField] float ForceDuration = 0.8f;
}