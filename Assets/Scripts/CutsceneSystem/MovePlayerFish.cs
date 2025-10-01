using UnityEngine;

namespace ForYou.Cutscene
{
    [System.Serializable]
    public class MovePlayerFish : CutsceneElement
    {
        public Transform TargetPosition;
        [Header("�� �Ÿ���ŭ ��������� ���������� ����˴ϴ�. �浹 ������ �� ����Ͽ� �����ϼ���.")]
        public float AllowDistance = 1;
        [Header("�� �Ÿ���ŭ ��������� ���������� ����˴ϴ�. �浹 ������ �� ����Ͽ� �����ϼ���.")]
        public float SlowDistance = 1;
        [Header("�ڵ����� �÷��̾� ������ ������ ���� �ٲܱ��?")]
        public bool AutoReturnToPlayerControlMode = true;
    }
}