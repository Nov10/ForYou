using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Helpers
{
    public enum ePosition
    {
        World = 0,
        Local
    }

    
    public class ObjectMoveHandler
    {
        Transform Mover;
        int PositionID;
        int RotationID;

        Dictionary<int, RecorededLocalTransformData> RecordedDatas;

        public void AddNewRecordTransformData(int key, Vector3 localPostion, Quaternion localRotation)
        {
            RecorededLocalTransformData d = new RecorededLocalTransformData(localPostion, localRotation);
            RecordedDatas.Add(key, d);
        }
        public void RecordNowLocalTransformData(int key)
        {
            RecorededLocalTransformData d = new RecorededLocalTransformData(Mover.localPosition, Mover.localRotation);
            RecordedDatas.Add(key, d);
        }

        class RecorededLocalTransformData
        {
            public Vector3 LocalPosition;
            public Quaternion LocalRotation;

            public RecorededLocalTransformData(Vector3 localPosition, Quaternion localRotation)
            {
                LocalPosition = localPosition;
                LocalRotation = localRotation;
            }
        }

        public ObjectMoveHandler(Transform mover)
        {
            Mover = mover;
            RecordedDatas = new Dictionary<int, RecorededLocalTransformData>();
        }

        public void TransformByRecoredData(int key, float duration)
        {
            var d = RecordedDatas[key];
            Move(d.LocalPosition, d.LocalRotation, duration, ePosition.Local);
        }

        public void Move(Vector3 position, Quaternion rotation, float time, ePosition positionType)
        {
            ObjectMoveHelper.TryStop(PositionID);
            PositionID = ObjectMoveHelper.MoveObject(Mover, position, time, positionType);
            ObjectMoveHelper.TryStop(RotationID);
            RotationID = ObjectMoveHelper.RotatebjectSlerp(Mover, rotation, time, positionType);
        }
        public void MoveSlerp(Vector3 position, Quaternion rotation, float time, ePosition positionType)
        {
            ObjectMoveHelper.TryStop(PositionID);
            PositionID = ObjectMoveHelper.MoveObjectSlerp(Mover, position, time, positionType);
            ObjectMoveHelper.TryStop(RotationID);
            RotationID = ObjectMoveHelper.RotatebjectSlerp(Mover, rotation, time, positionType);
        }
        public void MovePosition(Vector3 position, float time, ePosition positionType)
        {
            ObjectMoveHelper.TryStop(PositionID);
            PositionID = ObjectMoveHelper.MoveObject(Mover, position, time, positionType);
        }
        public void RotateSlerp(Quaternion rotation, float time, ePosition positionType)
        {
            ObjectMoveHelper.TryStop(RotationID);
            RotationID = ObjectMoveHelper.RotatebjectSlerp(Mover, rotation, time, positionType);
        }
    }
    public struct ExtendedEnumerator
    {
        /// <summary> 실제 실행되는 내용을 가지고 있는 Enumerator </summary>
        public IEnumerator Enumerator;
        /// <summary> 고유 ID </summary>
        public int ID { get; private set; }
        static int IDCounter = int.MinValue;

        public ExtendedEnumerator(IEnumerator enumerator)
        {
            Enumerator = enumerator;
            ID = IDCounter;
            IDCounter++;
        }
    }

    public class ObjectMoveHelper
    {

        /// <summary> ID에 해당하는 코루틴을 중지시킵니다. </summary>
        public static bool TryStop(int ID)
        {
            return ExtendedEnumeratorRunner.Instance.Stop(ID);
        }
        public static bool IsPlaying(int ID)
        {
            return ExtendedEnumeratorRunner.Instance.IsPlaying(ID);
        }
        public static int ScaleObject(Transform mover, Vector3 target, float time)
        {
            ExtendedEnumerator info = new ExtendedEnumerator(_ScaleObject(mover, target, time, null));

            ExtendedEnumeratorRunner.Instance.Run(info);
            return info.ID;
        }
        static IEnumerator _ScaleObject(Transform mover, Vector3 target, float time, Action onEnd)
        {
            float t = 0.0f;
            Vector3 startPos;
            startPos = mover.localScale;
            while (t <= time)
            {
                t += Time.deltaTime;
                mover.localScale = Vector3.Lerp(startPos, target, t / time);
                yield return null;
            }
            mover.localScale = target;
            onEnd?.Invoke();
        }
        public static int ScaleObject_Log(Transform mover, Vector3 target, float time)
        {
            ExtendedEnumerator info = new ExtendedEnumerator(_ScaleObject_Log(mover, target, time, null));

            ExtendedEnumeratorRunner.Instance.Run(info);
            return info.ID;
        }
        static IEnumerator _ScaleObject_Log(Transform mover, Vector3 target, float time, Action onEnd)
        {
            float t = 0.0f;
            Vector3 startPos;
            startPos = mover.localScale;
            while (t <= time)
            {
                t += Time.deltaTime;
                mover.localScale = Vector3.Slerp(startPos, target, t / time);
                yield return null;
            }
            mover.localScale = target;
            onEnd?.Invoke();
        }

        /// <summary>
        /// mover를 현재 위치에서 target까지 time의 시간동안 '부드러운(SmootherStep)' 이징으로 이동시킵니다. ID를 반환합니다.
        /// </summary>
        /// <param name="position">좌표 기준(World, Local)</param>
        public static int MoveObjectSmooth(Transform mover, Vector3 target, float time, ePosition position)
        {
            ExtendedEnumerator info = new ExtendedEnumerator(_MoveObjectSmooth(mover, target, time, position, null));
            ExtendedEnumeratorRunner.Instance.Run(info);
            return info.ID;
        }
        /// <summary>
        /// mover를 현재 위치에서 target까지 time의 시간동안 '부드러운(SmootherStep)' 이징으로 이동시킵니다. ID를 반환합니다.
        /// </summary>
        /// <param name="position">좌표 기준(World, Local)</param>
        public static int MoveObjectSmooth_FixedUpdate(Transform mover, Vector3 target, float time, ePosition position)
        {
            ExtendedEnumerator info = new ExtendedEnumerator(_MoveObjectSmooth_FixedUpdate(mover, target, time, position, null));
            ExtendedEnumeratorRunner.Instance.Run(info);
            return info.ID;
        }

        /// <summary>
        /// mover를 현재 위치에서 target까지 time의 시간동안 '부드러운(SmootherStep)' 이징으로 이동시킵니다. 종료 시 onEnd를 호출합니다.
        /// </summary>
        /// <param name="position">좌표 기준(World, Local)</param>
        public static int MoveObjectSmooth(Transform mover, Vector3 target, float time, ePosition position, Action onEnd)
        {
            ExtendedEnumerator info = new ExtendedEnumerator(_MoveObjectSmooth(mover, target, time, position, onEnd));
            ExtendedEnumeratorRunner.Instance.Run(info);
            return info.ID;
        }
        static IEnumerator _MoveObjectSmooth(Transform mover, Vector3 target, float time, ePosition position, Action onEnd)
        {
            // 즉시 이동 케이스 방어
            if (time <= 0f)
            {
                if (position == ePosition.World) mover.position = target;
                else mover.localPosition = target;

                onEnd?.Invoke();
                yield break;
            }

            float t = 0f;
            if (position == ePosition.World)
            {
                Vector3 startPos = mover.position;
                while (t < time)
                {
                    t += Time.deltaTime;
                    float u = Mathf.Clamp01(t / time);
                    // SmootherStep: 6x^5 - 15x^4 + 10x^3  (양 끝 미분 0 → 더 매끈)
                    float s = u * u * u * (u * (u * 6f - 15f) + 10f);

                    mover.position = Vector3.Lerp(startPos, target, s);
                    yield return null;
                }
                mover.position = target; // 최종 보정
            }
            else // Local
            {
                Vector3 startPos = mover.localPosition;
                while (t < time)
                {
                    t += Time.deltaTime;
                    float u = Mathf.Clamp01(t / time);
                    float s = u * u * u * (u * (u * 6f - 15f) + 10f);

                    mover.localPosition = Vector3.Lerp(startPos, target, s);
                    yield return null;
                }
                mover.localPosition = target; // 최종 보정
            }

            onEnd?.Invoke();
        }

        static IEnumerator _MoveObjectSmooth_FixedUpdate(Transform mover, Vector3 target, float time, ePosition position, Action onEnd)
        {
            // 즉시 이동 케이스 방어
            if (time <= 0f)
            {
                if (position == ePosition.World) mover.position = target;
                else mover.localPosition = target;

                onEnd?.Invoke();
                yield break;
            }

            float t = 0f;
            if (position == ePosition.World)
            {
                Vector3 startPos = mover.position;
                while (t < time)
                {
                    t += Time.deltaTime;
                    float u = Mathf.Clamp01(t / time);
                    // SmootherStep: 6x^5 - 15x^4 + 10x^3  (양 끝 미분 0 → 더 매끈)
                    float s = u * u * u * (u * (u * 6f - 15f) + 10f);

                    mover.position = Vector3.Lerp(startPos, target, s);
                    yield return new WaitForFixedUpdate();
                }
                mover.position = target; // 최종 보정
            }
            else // Local
            {
                Vector3 startPos = mover.localPosition;
                while (t < time)
                {
                    t += Time.deltaTime;
                    float u = Mathf.Clamp01(t / time);
                    float s = u * u * u * (u * (u * 6f - 15f) + 10f);

                    mover.localPosition = Vector3.Lerp(startPos, target, s);
                    yield return new WaitForFixedUpdate();
                }
                mover.localPosition = target; // 최종 보정
            }

            onEnd?.Invoke();
        }

        /// <summary> mover를 현재 위치에서 target까지 time의 시간동안 Lerp로 이동시킵니다. ID를 반환합니다. </summary>
        /// <param name="position">좌표 기준(World, Local)</param>
        public static int MoveObject(Transform mover, Vector3 target, float time, ePosition position)
        {
            ExtendedEnumerator info = new ExtendedEnumerator(_MoveObject(mover, target, time, position, null));

            ExtendedEnumeratorRunner.Instance.Run(info);
            return info.ID;
        }
        public static int MoveObject(Transform mover, Vector3 target, float time, ePosition position, Action onEnd)
        {
            ExtendedEnumerator info = new ExtendedEnumerator(_MoveObject(mover, target, time, position, onEnd));

            ExtendedEnumeratorRunner.Instance.Run(info);
            return info.ID;
        }
        static IEnumerator _MoveObject(Transform mover, Vector3 target, float time, ePosition position, Action onEnd)
        {
            float t = 0.0f;
            Vector3 startPos;
            if (position == ePosition.World)
            {
                startPos = mover.position;
                while (t <= time)
                {
                    t += Time.deltaTime;
                    mover.position = Vector3.Lerp(startPos, target, t / time);
                    yield return null;
                }
                mover.position = target;
            }
            else// if(position == ePosition.Local)
            {
                startPos = mover.localPosition;
                while (t <= time)
                {
                    t += Time.deltaTime;
                    mover.localPosition = Vector3.Lerp(startPos, target, t / time);
                    yield return null;
                }
                mover.localPosition = target;
            }
            onEnd?.Invoke();
        }

        /// <summary> mover를 현재 위치에서 target까지 time의 시간동안 SLerp로 이동시킵니다. ID를 반환합니다. </summary>
        /// <param name="position">좌표 기준(World, Local)</param>
        public static int MoveObjectSlerp(Transform mover, Vector3 target, float time, ePosition position)
        {
            ExtendedEnumerator info = new ExtendedEnumerator(_MoveObjectSlerp(mover, target, time, position, null));

            ExtendedEnumeratorRunner.Instance.Run(info);
            return info.ID;
        }
        public static int MoveObjectSlerp(Transform mover, Vector3 target, float time, ePosition position, Action onEnd)
        {
            ExtendedEnumerator info = new ExtendedEnumerator(_MoveObjectSlerp(mover, target, time, position, onEnd));

            ExtendedEnumeratorRunner.Instance.Run(info);
            return info.ID;
        }
        static IEnumerator _MoveObjectSlerp(Transform mover, Vector3 target, float time, ePosition position, Action onEnd)
        {
            float t = 0.0f;
            Vector3 startPos;
            if (position == ePosition.World)
            {
                startPos = mover.position;
                while (t <= time)
                {
                    t += Time.deltaTime;
                    mover.position = Vector3.Slerp(startPos, target, t / time);
                    yield return null;
                }
                mover.position = target;
            }
            else// if(position == ePosition.Local)
            {
                startPos = mover.localPosition;
                while (t <= time)
                {
                    t += Time.deltaTime;
                    mover.localPosition = Vector3.Slerp(startPos, target, t / time);
                    yield return null;
                }
                mover.localPosition = target;
            }

            onEnd?.Invoke();
        }

        /// <summary> mover를 현재 위치에서 target까지 time의 시간동안 Lerp로 이동시킵니다. ID를 반환합니다. </summary>
        /// <param name="position">좌표 기준(World, Local)</param>
        public static int RotatebjectLerp(Transform mover, Vector3 target, float time, ePosition position)
        {
            ExtendedEnumerator info = new ExtendedEnumerator(_RotateObjectLerp(mover, target, time, position, null));

            ExtendedEnumeratorRunner.Instance.Run(info);
            return info.ID;
        }
        public static int RotatebjectLerp(Transform mover, Vector3 target, float time, ePosition position, Action onEnd)
        {
            ExtendedEnumerator info = new ExtendedEnumerator(_RotateObjectLerp(mover, target, time, position, onEnd));

            ExtendedEnumeratorRunner.Instance.Run(info);
            return info.ID;
        }
        static IEnumerator _RotateObjectLerp(Transform mover, Vector3 target, float time, ePosition position, Action onEnd)
        {
            float t = 0.0f;
            Quaternion startRot, targetRot;
            targetRot = Quaternion.Euler(target);
            if (position == ePosition.World)
            {
                startRot = Quaternion.Euler(mover.eulerAngles);
                while (t <= time)
                {
                    t += Time.deltaTime;
                    mover.rotation = Quaternion.Lerp(startRot, targetRot, t / time);
                    yield return null;
                }
                mover.eulerAngles = target;
            }
            else// if(position == ePosition.Local)
            {
                startRot = Quaternion.Euler(mover.localEulerAngles);
                while (t <= time)
                {
                    t += Time.deltaTime;
                    mover.localRotation = Quaternion.Lerp(startRot, targetRot, t / time);
                    yield return null;
                }
                mover.localEulerAngles = target;
            }

            onEnd?.Invoke();
        }

        public static int ChangeAlpha(CanvasGroup group, float targetAlpha, float time)
        {
            ExtendedEnumerator info = new ExtendedEnumerator(_ChangeAlpha(group, targetAlpha, time, null));
            ExtendedEnumeratorRunner.Instance.Run(info);
            return info.ID;
        }
        public static int ChangeAlpha(UnityEngine.UI.Image img, float targetAlpha, float time)
        {
            ExtendedEnumerator info = new ExtendedEnumerator(_ChangeAlpha(img, targetAlpha, time, null));
            ExtendedEnumeratorRunner.Instance.Run(info);
            return info.ID;
        }
        static IEnumerator _ChangeAlpha(CanvasGroup group, float targetAlpha, float time, Action onEnd)
        {
            float t = 0.0f;
            float startAlpha = group.alpha;
            while (t <= time)
            {
                t += Time.deltaTime;
                group.alpha = Mathf.Lerp(startAlpha, targetAlpha, t / time);
                yield return null;
            }
            group.alpha = targetAlpha;
            onEnd?.Invoke();
        }
        static IEnumerator _ChangeAlpha(UnityEngine.UI.Image img, float targetAlpha, float time, Action onEnd)
        {
            float t = 0.0f;
            Color startColor = img.color;
            Color targetColor = new Color(startColor.r, startColor.g, startColor.b, targetAlpha);
            while (t <= time)
            {
                t += Time.deltaTime;
                img.color = Color.Lerp(startColor, targetColor, t / time);
                yield return null;
            }
            img.color = targetColor;
            onEnd?.Invoke();
        }

        /// <summary> mover를 현재 위치에서 target까지 time의 시간동안 SLerp로 이동시킵니다. ID를 반환합니다. </summary>
        /// <param name="position">좌표 기준(World, Local)</param>
        public static int RotatebjectSlerp(Transform mover, Quaternion target, float time, ePosition position)
        {
            ExtendedEnumerator info = new ExtendedEnumerator(_RotateObjectSlerp(mover, target, time, position, null));

            ExtendedEnumeratorRunner.Instance.Run(info);
            return info.ID;
        }
        public static int RotatebjectSlerp(Transform mover, Quaternion target, float time, ePosition position, Action onEnd)
        {
            ExtendedEnumerator info = new ExtendedEnumerator(_RotateObjectSlerp(mover, target, time, position, onEnd));

            ExtendedEnumeratorRunner.Instance.Run(info);
            return info.ID;
        }
        static IEnumerator _RotateObjectSlerp(Transform mover, Quaternion target, float time, ePosition position, Action onEnd)
        {
            float t = 0.0f;
            Quaternion startRot, targetRot;
            targetRot = target;
            if (position == ePosition.World)
            {
                startRot = mover.rotation;
                while (t <= time)
                {
                    t += Time.deltaTime;
                    mover.rotation = Quaternion.Slerp(startRot, targetRot, t / time);
                    yield return null;
                }
                mover.rotation = target;
            }
            else// if(position == ePosition.Local)
            {
                startRot = mover.localRotation;
                while (t <= time)
                {
                    t += Time.deltaTime;
                    mover.localRotation = Quaternion.Slerp(startRot, targetRot, t / time);
                    yield return null;
                }
                mover.localRotation = target;
            }

            onEnd?.Invoke();
        }
    }
}