using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ExtendedStringPlayer : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] public ExtendedSentence Sentence;
    [SerializeField] TMP_Text Text;

    [Header("Playback")]
    [SerializeField, Tooltip("초기 지연(초)")] float StartDelay = 0f;
    [SerializeField, Tooltip("전체 시간 배속(=1은 실시간)")] float TimeScale = 1f;

    [Header("Vibrate Tuning")]
    [SerializeField, Tooltip("진동 전체 크기 배율")] float GlobalAmplitude = 1f;

    [Header("Shock Wave (Square-like)")]
    [SerializeField, Tooltip("Shock 파형의 날카로움(↑ 더 각지게)")] float ShockSharpness = 6f;
    [SerializeField, Tooltip("Shock 파형 계단 단계 수(0 또는 1이면 비활성)")] int ShockStairSteps = 0;

    [Header("Chat Bubble")]
    [SerializeField] Vector2 Padding = Vector2.zero;
    [SerializeField] Vector2 MinSize = new Vector2(80, 50);

    TMP_MeshInfo[] _cachedMeshInfo;
    string _lastText = null;
    int _lastCharCount = -1;
    float _t0;

    readonly List<ExtendedSentence.VibrateSample> _samples = new();

    void Awake()
    {
        if (Text == null) Text = GetComponent<TMP_Text>();
    }

    void OnEnable() => ResetTimer();

    public void Play(ExtendedSentence sentence, Transform _unused = null)
    {
        Sentence = sentence;
        ResetTimer();
    }

    void ResetTimer()
    {
        _t0 = Time.time + StartDelay;
        _lastText = null;
        _lastCharCount = -1;
    }

    void Update()
    {
        if (Sentence == null || Text == null) return;

        float elapsed = Mathf.Max(0f, (Time.time - _t0) * TimeScale);

        // 1) 현재 시점 텍스트/진동 샘플
        string display = Sentence.Evaluate(elapsed, out var vibrates);

        // 2) 텍스트 변경/캐시
        bool needRebuild = _lastText != display;
        if (needRebuild)
        {
            Text.text = display;
            Text.ForceMeshUpdate();
            _cachedMeshInfo = Text.textInfo.CopyMeshInfoVertexData();
            _lastText = display;
            _lastCharCount = Text.textInfo.characterCount;
        }
        else if (_lastCharCount != Text.textInfo.characterCount)
        {
            Text.ForceMeshUpdate();
            _cachedMeshInfo = Text.textInfo.CopyMeshInfoVertexData();
            _lastCharCount = Text.textInfo.characterCount;
        }

        var textInfo = Text.textInfo;

        // 3) 원본으로 리셋
        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            var src = _cachedMeshInfo[i].vertices;
            var dst = textInfo.meshInfo[i].vertices;
            if (dst.Length != src.Length) continue;
            Array.Copy(src, dst, src.Length);
        }

        // 4) 진동 적용
        _samples.Clear();
        _samples.AddRange(vibrates);

        foreach (var v in _samples)
        {
            if (v.visibleCharIndex < 0 || v.visibleCharIndex >= textInfo.characterCount) continue;

            if (v.duration > 0f)
            {
                float tnorm = (elapsed - v.startTime) / v.duration;
                if (tnorm < 0f || tnorm > 1f) continue;
            }

            var ch = textInfo.characterInfo[v.visibleCharIndex];
            if (!ch.isVisible) continue;

            float em = Mathf.Max(0.0001f, ch.ascender - ch.descender);
            float t = (v.duration > 0f) ? Mathf.Clamp01((elapsed - v.startTime) / v.duration) : 1f;

            float ampY = v.linAmpY * Mathf.Exp(-v.expAmpY * t) * GlobalAmplitude;
            float ampX = v.linAmpX * Mathf.Exp(-v.expAmpX * t) * GlobalAmplitude;

            float argY = v.freqY * t + v.phasePerCharY * v.localIndex;
            float argX = v.freqX * t + v.phasePerCharX * v.localIndex;

            // ---- 파형 선택 ----
            float waveY = (v.waveform == ExtendedSentence.Waveform.Smooth)
                ? Mathf.Cos(argY)
                : ShockWave(argY); // ← 변경: sign(cos) 대신

            float waveX = (v.waveform == ExtendedSentence.Waveform.Smooth)
                ? Mathf.Cos(argX)
                : ShockWave(argX); // ← 변경

            Vector3 offset = new Vector3(ampX * waveX * em, ampY * waveY * em, 0f);
            ChangeChatacterPosition(Text, v.visibleCharIndex, offset);
        }

        // 5) 메쉬 반영
        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            var mi = textInfo.meshInfo[i];
            mi.mesh.vertices = mi.vertices;
            Text.UpdateGeometry(mi.mesh, i);
        }

        var rect = Text.transform.parent.GetComponent<RectTransform>();
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Max(Text.renderedWidth + Padding.x, MinSize.x));
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Max(Text.renderedHeight + Padding.y, MinSize.y));
    }

    // Shock 파형: soft-square (tanh(sin)) + (선택) 계단화
    float ShockWave(float arg)
    {
        // 소프트 스퀘어: -1..1
        float soft = System.MathF.Tanh(ShockSharpness * Mathf.Sin(arg));

        // 계단화(선택): 단계 수가 2 이상이면 양자화
        if (ShockStairSteps > 1)
        {
            float s = ShockStairSteps;
            soft = Mathf.Round(soft * s) / s;
        }
        return soft;
    }

    // (가) 방식 — 지정 인덱스 문자만 이동
    void ChangeChatacterPosition(TMP_Text text, int index, Vector3 offset)
    {
        var info = text.textInfo;
        if (index < 0 || index >= info.characterCount) return;

        var ch = info.characterInfo[index];
        if (!ch.isVisible) return;

        int matIndex = ch.materialReferenceIndex;
        int vIndex = ch.vertexIndex;

        var verts = info.meshInfo[matIndex].vertices;
        verts[vIndex + 0] += offset;
        verts[vIndex + 1] += offset;
        verts[vIndex + 2] += offset;
        verts[vIndex + 3] += offset;
    }
}
