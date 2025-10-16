using ForYou.GamePlay;
using Helpers;
using UnityEngine;

namespace ForYou.Cutscene
{
    public class Stage2 : MonoBehaviour
    {
        [SerializeField] Transform Enemy;
        [SerializeField] Transform EnemyMoveTarget1;
        [SerializeField] float Duration1 = 3.0f;
        int ID1;
        public void CUTSCENE_FUNCTION_ShowEnemyFish1()
        {
            ID1 = ObjectMoveHelper.MoveObjectSmooth(Enemy, EnemyMoveTarget1.position, Duration1, ePosition.World);
            Enemy.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        }
        public bool CUTSCENE_ENDFUNCTION_ShowEnemyFish1()
        {
            return ObjectMoveHelper.IsPlaying(ID1);
        }


        [SerializeField] Transform EnemyMoveTarget2;
        [SerializeField] float Duration2 = 3.0f;
        public void CUTSCENE_FUNCTION_ShowEnemyFish2()
        {
            ID1 = ObjectMoveHelper.MoveObjectSmooth(Enemy, EnemyMoveTarget2.position, Duration2, ePosition.World);
            Enemy.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        }
        public bool CUTSCENE_ENDFUNCTION_ShowEnemyFish2()
        {
            return ObjectMoveHelper.IsPlaying(ID1);
        }
        [SerializeField] Transform EnemyMoveTarget3;
        [SerializeField] float Duration3 = 3.0f;
        public void CUTSCENE_FUNCTION_ShowEnemyFish3()
        {
            ID1 = ObjectMoveHelper.MoveObjectSmooth(Enemy, EnemyMoveTarget3.position, Duration3, ePosition.World);
            Enemy.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        }
        public bool CUTSCENE_ENDFUNCTION_ShowEnemyFish3()
        {
            return ObjectMoveHelper.IsPlaying(ID1);
        }
    }
}