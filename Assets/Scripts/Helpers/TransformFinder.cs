using System.Collections.Generic;
using UnityEngine;

namespace Helpers
{
    [ExecuteAlways]
    /// <summary>
    /// Transform의 자식, 부모, 혹은 그 중 특정한 객체를 찾기 위해 사용됩니다.
    /// </summary>
    public class TransformFinder : MonoBehaviour
    {
#if UNITY_EDITOR
        public Transform RightHand;
        public Transform LeftHand;

        public Transform Neck;
        public Transform Hips;
        public Transform Head;
        public Transform Spine;
        public Transform Chest;
        public Transform UpperChest;
        public Transform RightElbow;
        public Transform LeftElbow;
        public Transform RightShoulder;
        public Transform LeftShoulder;

        void Awake()
        {
            Head = FindChild(transform, HumanoidBone.Head);
            Neck = FindChild(transform, HumanoidBone.Neck);
            RightHand = FindChild(transform, HumanoidBone.RightHand);
            LeftHand = FindChild(transform, HumanoidBone.LeftHand);
            RightShoulder = FindChild(transform, HumanoidBone.RightShoulder);
            LeftShoulder = FindChild(transform, HumanoidBone.LeftShoulder);
            RightElbow = FindChild(transform, HumanoidBone.RightElbow);
            LeftElbow = FindChild(transform, HumanoidBone.LeftElbow);
            UpperChest = FindChild(transform, HumanoidBone.UpperChest);
            Chest = FindChild(transform, HumanoidBone.Chest);
            Spine = FindChild(transform, HumanoidBone.Spine);
            Hips = FindChild(transform, HumanoidBone.Hips);
        }
#endif
        public static class HumanoidBone
        {
            public static string Head = "cf_J_Head";
            public static string Neck = "cf_J_Neck";
            public static string RightHand = "cf_J_Hand_R";
            public static string LeftHand = "cf_J_Hand_L";
            public static string RightShoulder = "cf_J_Shoulder_R";
            public static string LeftShoulder = "cf_J_Shoulder_L";
            public static string RightElbow = "cf_J_ArmLow01_R";
            public static string LeftElbow = "cf_J_ArmLow01_L";
            public static string UpperChest = "cf_J_Spine03";
            public static string Chest = "cf_J_Spine02";
            public static string Spine = "cf_J_Spine01";
            public static string Hips = "cf_J_Hips";
        }

        /// <summary>
        /// components 중 name의 이름을 가진 첫번째 component을 반환합니다.
        /// </summary>
        public static T FindComponent<T>(T[] components, string name) where T : Component
        {
            for(int i = 0; i < components.Length; i++)
            {
                if(string.Equals(components[i].transform.name, name) == true)
                {
                    return components[i];
                }
            }
            return null;
        }
        /// <summary>
        /// root의 자식 중 boneName의 이름을 가진 첫번째 Transform을 반환합니다.
        /// </summary>
        public static Transform FindChild(Transform root, string boneName)
        {
            return _FindChild(root, boneName);
        }
        /// <summary>
        /// root의 자식 중 boneName의 이름을 가진 첫번째 Transform을 반환합니다.
        /// </summary>
        public static Transform FindChildContains(Transform root, string boneName)
        {
            return _FindChildContains(root, boneName);
        }
        /// <summary>
        /// root의 자식 중 boneName의 이름을 가진 첫번째 Transform을 반환합니다.
        /// </summary>
        /// <param name="debug">찾을 수 없을시 오류를 출력할 것인가?</param>
        public static Transform FindChild(Transform root, string boneName, bool debug)
        {
            Transform result = _FindChild(root, boneName);
            if (debug == true && result == null)
                Debug.LogError("Trying to find " + boneName + " in " + root + " was failed!");
            return result;
        }
        /// <summary>
        /// root의 자식 중 boneName의 이름을 가진 첫번째 Transform을 result에 할당하고, 성공 시 true 실패 시 false를 반환힙니다.
        /// </summary>
        /// <param name="debug">찾을 수 없을시 오류를 출력할 것인가?</param>
        public static bool TryFindChild(Transform root, string boneName, out Transform result, bool debug)
        {
            result = _FindChild(root, boneName);
            if(result != null)
            {
                return true;
            }
            if (debug == true && result == null)
                Debug.LogError("Trying to find " + boneName + " in " + root + " was failed!");
            return false;
        }

        //성공 시 Transform 반환, 실패 시 Null 반환
        static Transform _FindChild(Transform root, string boneName)
        {
            Transform result;
            if (root.childCount > 0)
            {
                for (int i = 0; i < root.childCount; i++)
                {
                    if (string.Equals((result = root.GetChild(i)).name, boneName) == true)
                        return result;
                }

                for (int i = 0; i < root.childCount; i++)
                {
                    result = _FindChild(root.GetChild(i), boneName);
                    if(result == null)
                        continue;
                    else
                        return result;
                }
            }
            return null;
        }
        static Transform _FindChildContains(Transform root, string boneName)
        {
            Transform result;
            if (root.childCount > 0)
            {
                for (int i = 0; i < root.childCount; i++)
                {
                    result = root.GetChild(i);
                    if (result.name.Contains(boneName) == true)
                        return result;
                }

                for (int i = 0; i < root.childCount; i++)
                {
                    result = _FindChildContains(root.GetChild(i), boneName);
                    if (result == null)
                        continue;
                    else
                        return result;
                }
            }
            return null;
        }

        /// <summary>
        /// root의 모든 자식들을 반환합니다.
        /// </summary>
        public static Transform[] GetAllDirectChildren(Transform root)
        {
            return _GetAllDirectChildren(root, containsRoot: false);
        }
        /// <summary>
        /// root의 모든 자식들을 반환합니다.
        /// </summary>
        public static Transform[] GetAllChildren(Transform root)
        {
            return _GetAllChildren(root, containsRoot: false);
        }
        /// <summary>
        /// root의 모든 자식들을 반환합니다.
        /// </summary>
        /// <param name="containsRoot">자식 목록의 가장 첫번쨰에 root를 추가할 것인가?</param>
        public static Transform[] GetAllChildren(Transform root, bool containsRoot)
        {
            return _GetAllChildren(root, containsRoot);
        }
        /// <summary>
        /// root까지 포함하여 root의 모든 자식들을 반환합니다.
        /// </summary>
        public static Transform[] GetAllChildrenWithRoot(Transform root)
        {
            return _GetAllChildren(root, true);
        }
        static Transform[] _GetAllDirectChildren(Transform root, bool containsRoot)
        {
            List<Transform> result = new List<Transform>();
            if (containsRoot == true)
                result.Add(root);
            if (root.childCount > 0)
            {
                for (int i = 0; i < root.childCount; i++)
                {
                    var v = root.GetChild(i);
                    result.Add(v);
                }
            }
            return result.ToArray();
        }
        static Transform[] _GetAllChildren(Transform root, bool containsRoot)
        {
            List<Transform> result = new List<Transform>();
            if (containsRoot == true)
                result.Add(root);
            if (root.childCount > 0)
            {
                for (int i = 0; i < root.childCount; i++)
                {
                    var v = _GetAllChildren(root.GetChild(i), true);
                    result.AddRange(new List<Transform>(v));
                }
            }
            return result.ToArray();
        }
        public static bool TryGetNearestParent<T>(Component child, out T result) where T : Component
        {
            Transform now = child.transform;
            while (now.parent != null)
            {
                now = now.parent;
                if (now.TryGetComponent<T>(out result))
                {
                    return true;
                }
            }
            result = null;
            return false;
        }
        public static T GetNearestParent<T>(Component child) where T : Component
        {
            T result;
            Transform now = child.transform;
                                if (now.TryGetComponent<T>(out result))
                return result;
            while (now.parent != null)
            {
                now = now.parent;
                if (now.TryGetComponent<T>(out result))
                    return result;
            }
            return null;
        }
        /// <summary>
        /// child의 최상위 부모 Transform을 반환합니다.
        /// </summary>
        public static Transform GetRoot(Component child)
        {
            Transform now = child.transform;
            while (now.parent != null)
                now = now.parent;
            return now;
        }
        /// <summary>
        /// child의 level 단계만큼 위에 있는 부모 Transform을 반환합니다.
        /// </summary>
        public static Transform GetParentBone(Transform child, int level)
        {
            Transform now = child;
            int nowLevel = -1;
            while (now.parent != null)
            {
                nowLevel++;
                if (nowLevel == level)
                    return now;
                now = now.parent;
            }
            return now;
        }     
    }
}