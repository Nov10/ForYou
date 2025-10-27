using ForYou.GamePlay;
using Helpers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ForYou.Cutscene
{
    public class TutorialScene : MonoBehaviour
    {
        //bool SnatchedPlankton;
        public void CUTSCENE_FUNCTION_TakePlankton()
        {
            P1.gameObject.SetActive(true);
        }
        public bool CUTSCENE_ENDFUNCTION_TakePlankton()
        {
            if(FindFirstObjectByType<Anemone>().GetGage() > 0)
            {
                return false;
            }
            return true;
        }
        private void Start()
        {
            Enemy.gameObject.SetActive(false);
            P1.gameObject.SetActive(false);
            P2.gameObject.SetActive(false);
        }


        public void CUTSCENE_FUNCTION_DoesHavePlankton()
        {
            P2.gameObject.SetActive(true);
        }
        public bool CUTSCENE_ENDFUNCTION_DoesHavePlankton()
        {
            if (FindFirstObjectByType<PlayerFish>().DoesHavePlankton == true)
            {
                FindFirstObjectByType<PlayerFish>().PreventDropPlankton = true;
                return false;
            }
            return true;
        }

        [SerializeField] Plankton P1;
        [SerializeField] Plankton P2;
        [SerializeField] NormalEnemyFish Enemy;
        [SerializeField] Transform EnemyMoveTarget1;
        [SerializeField] Transform EnemyMoveTarget2;
        [SerializeField] float MoveDuration = 3.0f;
        [SerializeField] float Duration = 3.0f;
        [SerializeField] float MoveDuration2 = 3.0f;
        [SerializeField] float Duration2 = 3.0f;
        float t;
        int ID1;
        public void CUTSCENE_FUNCTION_ShowEnemyFish()
        {
            Enemy.gameObject.SetActive(true);
            Enemy.IsRunningAI = false;
            t = Time.time;
            ID1 = ObjectMoveHelper.MoveObjectSmooth(Enemy.transform, EnemyMoveTarget1.position, MoveDuration, ePosition.World);
            Enemy.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        }
        public bool CUTSCENE_ENDFUNCTION_ShowEnemyFish()
        {

            if (Time.time - t > Duration)
            {
                return false;
            }
            return true;
        }

        public void CUTSCENE_FUNCTION_RunFish()
        {
            ObjectMoveHelper.TryStop(ID1);
            Enemy.IsRunningAI = true;
            Enemy.SetTarget(FindFirstObjectByType<PlayerFish>());
            Enemy.SetState(NormalEnemyFish.State.Chase);

            DelayedFunctionHelper.InvokeDelayed(0.4f, () =>
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
        public void CUTSCENE_FUNCTION_KillFish()
        {
            Enemy.IsRunningAI = false;
            t = Time.time;
            ID1 = ObjectMoveHelper.MoveObjectSmooth(Enemy.transform, EnemyMoveTarget2.position, MoveDuration2, ePosition.World);
            //Enemy.GetComponent<Rigidbody2D>().linearVelocity = new Vector2(1f, 1f);
            Enemy.transform.rotation = Quaternion.Euler(0, 180, -35);
        }
        public bool CUTSCENE_ENDFUNCTION_KillFish()
        {
            if (Time.time - t > Duration2)
            {
                Enemy.gameObject.SetActive(false);
                return false;
            }
            return true;
        }
    }
}