using ForYou.GamePlay;
using UnityEngine;

public class Anemone : MonoBehaviour
{
    [SerializeField] float Level;
    float BaseSize;
    [SerializeField] float SizePerLevel;

    private void Start()
    {
        BaseSize = transform.localScale.x;
        UpLevel(0);
    }

    public void UpLevel(float delta)
    {
        Level += delta;
        SetSizeByLevel(Level);
    }

    void SetSizeByLevel(float level)
    {
        float size = CalculateSizeByLevel(level);
        transform.localScale = new Vector3(size, size, 1);
    }

    float CalculateSizeByLevel(float level)
    {
        return BaseSize + level * SizePerLevel;
    }


    public void OnPlayerFishDetected(PlayerFish fish)
    {
        if(fish.DoesHavePlankton == true)
        {
            var plankton = fish.GetPlankton();
            fish.DropPlankton();

            float additionalLevel = plankton.GetLevel();
            UpLevel(additionalLevel);

            Destroy(plankton.gameObject);
        }
        else
        {

        }
    }
}