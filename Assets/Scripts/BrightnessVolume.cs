using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class BrightnessVolume : MonoBehaviour
{
    Volume Volume;
    ColorAdjustments Adjuster;
    public static BrightnessVolume Instance { get; private set; }

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
        Volume = GetComponent<Volume>();
        Volume.profile.TryGet<ColorAdjustments>(out Adjuster);
    }
    public void SetValue(float brightness)
    {
        Adjuster.postExposure.value = brightness - 0.5f;
    }
}
