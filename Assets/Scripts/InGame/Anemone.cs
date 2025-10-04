using ForYou.GamePlay;
using TMPro;
using UnityEngine;

public class Anemone : MonoBehaviour
{
    [SerializeField] int Gage;
    [SerializeField] int[] GageThresholdsForLevelUp;
    [SerializeField] TMP_Text GageText;

    [SerializeField] EnemyFishDetector AttackOrEatRange;

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
    int GetNetGage()
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

        BaseSize = transform.localScale.x;
        UpGage(0);
    }

    public void UpGage(int delta)
    {
        Gage += delta;
        SetSizeByLevel();
        GageText.text = $"{GetNetGage()} / {GetGageThreshold(GetNowLevel())}";
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

            Destroy(plankton.gameObject);
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
            //¸Ô±â
            var gage = data.Gage;
            UpGage(gage);
            Destroy(fish.gameObject);
        }
        else
        {
            //ÂÑ¾Æ³»±â
            fish.OnAttackedByAnemone(this);
        }
    }
}