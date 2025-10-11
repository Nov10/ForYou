using ForYou.GamePlay;
using Helpers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ForYou.Cutscene
{
    public class TutorialScene : MonoBehaviour
    {
        public void CUTSCENE_FUNCTION_DoesHavePlankton()
        {
        }
        public bool CUTSCENE_ENDFUNCTION_DoesHavePlankton()
        {
            if(FindFirstObjectByType<PlayerFish>().DoesHavePlankton == true)
            {
                //ÇÃ¶ûÅ©ÅæÀ» ¾ò¾úÀ½ -> Á¾·á
                FindFirstObjectByType<PlayerFish>().PreventDropPlankton = true;
                return false;
            }
            return true;
        }


        [SerializeField] NormalEnemyFish Enemy;
        [SerializeField] Transform EnemyMoveTarget1;
        [SerializeField] float Duration = 3.0f;
        int ID1;
        public void CUTSCENE_FUNCTION_ShowEnemyFish()
        {
            Enemy.IsRunningAI = false;
            ID1 = ObjectMoveHelper.MoveObject(Enemy.transform, EnemyMoveTarget1.position, Duration, ePosition.World);
            Enemy.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        }
        public bool CUTSCENE_ENDFUNCTION_ShowEnemyFish()
        {
            return ObjectMoveHelper.IsPlaying(ID1);
        }

        public void CUTSCENE_FUNCTION_RunFish()
        {
            Enemy.IsRunningAI = true;
            Enemy.SetTarget(FindFirstObjectByType<PlayerFish>());
            Enemy.SetState(NormalEnemyFish.State.Chase);

            DelayedFunctionHelper.InvokeDelayed(0.8f, () =>
            {
                FindFirstObjectByType<PlayerFish>().PreventDropPlankton = false;
            });
        }
        public bool CUTSCENE_ENDFUNCTION_RunFish()
        {
            if (FindFirstObjectByType<PlayerFish>().DoesHavePlankton == false)
            {
                //ÇÃ¶ûÅ©ÅæÀ» ¹ö·ÈÀ½ -> Á¾·á
                return false;
            }
            return true;
        }

        public void CUTSCENE_FUNCTION_CheckSuccess()
        {
            Debug.Log(FindFirstObjectByType<Anemone>().GetNetGage());
            bool isAnemoneEatPlankton = FindFirstObjectByType<Anemone>().GetNetGage() > 0;

            if (isAnemoneEatPlankton == true)
            {
                SceneManager.LoadScene(ConstValue.SCENE_INDEX_Stage1);
            }
            else
            {
                SceneManager.LoadScene(ConstValue.SCENE_INDEX_Tutorial);
            }
        }
        public bool CUTSCENE_ENDFUNCTION_CheckSuccess()
        {
            return false;
        }
    }
}