using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// ExtendedSentence가 만든 텍스트와 Vibrate 샘플을 받아
/// TextMeshPro 버텍스를 직접 이동시켜 진동 효과를 구현.
/// </summary>
[RequireComponent(typeof(TMP_Text))]
public class TMPVibrateAnimator : MonoBehaviour
{
    public ExtendedSentence Sentence;
    [Tooltip("시작 지연(초)")]
    public float StartDelay = 0f;
    [Tooltip("전체 시간 배속(=1은 실시간)")]
    public float TimeScale = 1f;

    TMP_Text _tmp;
    TMP_MeshInfo[] _cachedMeshInfo; // 원본 버텍스 캐시
    string _lastText = null;
    int _lastCharCount = -1;

    float _t0;
    readonly List<ExtendedSentence.VibrateSample> _samples = new();

    void Awake()
    {
        _tmp = GetComponent<TMP_Text>();
    }

    void OnEnable()
    {
        ResetTimer();
    }

    public void ResetTimer()
    {
        _t0 = Time.time + StartDelay;
        _lastText = null;
        _lastCharCount = -1;
    }

    void Update()
    {
        if (Sentence == null || _tmp == null) return;

        float elapsed = Mathf.Max(0f, (Time.time - _t0) * TimeScale);

        // 1) 현재 시점에 보여줄 텍스트와 Vibrate 샘플 얻기
        string display = Sentence.Evaluate(elapsed, out var vibrates);

        // 2) 텍스트가 바뀌었거나(타자 진행) 글자 수가 달라지면 메쉬 갱신 및 원본 버텍스 캐시
        bool needRebuild = _lastText != display;
        if (needRebuild)
        {
            _tmp.text = display;
            _tmp.ForceMeshUpdate();
            _cachedMeshInfo = _tmp.textInfo.CopyMeshInfoVertexData();
            _lastText = display;
            _lastCharCount = _tmp.textInfo.characterCount;
        }
        else if (_lastCharCount != _tmp.textInfo.characterCount)
        {
            _tmp.ForceMeshUpdate();
            _cachedMeshInfo = _tmp.textInfo.CopyMeshInfoVertexData();
            _lastCharCount = _tmp.textInfo.characterCount;
        }

        // 3) 버텍스 진동 적용
        _samples.Clear();
        _samples.AddRange(vibrates);
        ApplyVibrations(elapsed);
    }

    void ApplyVibrations(float elapsed)
    {
        var textInfo = _tmp.textInfo;

        // 매 프레임 원본 버텍스로 리셋
        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            var src = _cachedMeshInfo[i].vertices;
            var dst = textInfo.meshInfo[i].vertices;
            if (dst.Length != src.Length) continue;
            Array.Copy(src, dst, src.Length);
        }

        // 각 진동 문자에 오프셋 적용
        foreach (var v in _samples)
        {
            if (v.visibleCharIndex < 0 || v.visibleCharIndex >= textInfo.characterCount) continue;

            var charInfo = textInfo.characterInfo[v.visibleCharIndex];
            if (!charInfo.isVisible) continue;

            int matIndex = charInfo.materialReferenceIndex;
            int vIndex = charInfo.vertexIndex;

            // 1em을 글자 높이로 근사 (ascender - descender)
            float em = Mathf.Max(0.0001f, charInfo.ascender - charInfo.descender);

            float t = (v.duration > 0f) ? Mathf.Clamp01((elapsed - v.startTime) / v.duration) : 1f;

            float ampY = v.linAmpY * Mathf.Exp(-v.expAmpY * t);
            float ampX = v.linAmpX * Mathf.Exp(-v.expAmpX * t);

            float y = ampY * Mathf.Sign(Mathf.Cos(v.freqY * t + v.phasePerCharY * v.localIndex));
            float x = ampX * Mathf.Sign( Mathf.Cos(v.freqX * t + v.phasePerCharX * v.localIndex));

            Vector3 offset = new Vector3(x * em, y * em, 0f);

            var verts = textInfo.meshInfo[matIndex].vertices;
            verts[vIndex + 0] += offset;
            verts[vIndex + 1] += offset;
            verts[vIndex + 2] += offset;
            verts[vIndex + 3] += offset;
        }

        // 메쉬에 반영
        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            var meshInfo = textInfo.meshInfo[i];
            meshInfo.mesh.vertices = meshInfo.vertices;
            _tmp.UpdateGeometry(meshInfo.mesh, i);
        }
    }
}
