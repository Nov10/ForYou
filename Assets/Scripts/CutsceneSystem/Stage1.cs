using ForYou.GamePlay;
using Helpers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ForYou.Cutscene
{
    public class Stage1 : MonoBehaviour
    {
        [SerializeField] float SceneTransitionDelay = 3.0f;
        bool Executed = false;
        private void Update()
        {
            if(InGameManager.Instance.IsGameOver == true && Executed == false)
            {
                DelayedFunctionHelper.InvokeDelayed(SceneTransitionDelay, () =>
                {
                    SceneLoader.LoadScene(ConstValue.SCENE_INDEX_Stage2);
                });




                Executed = true;
            }
        }
    }
}