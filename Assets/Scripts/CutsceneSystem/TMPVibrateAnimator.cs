using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// ExtendedSentence�� ���� �ؽ�Ʈ�� Vibrate ������ �޾�
/// TextMeshPro ���ؽ��� ���� �̵����� ���� ȿ���� ����.
/// </summary>
[RequireComponent(typeof(TMP_Text))]
public class TMPVibrateAnimator : MonoBehaviour
{
    public ExtendedSentence Sentence;
    [Tooltip("���� ����(��)")]
    public float StartDelay = 0f;
    [Tooltip("��ü �ð� ���(=1�� �ǽð�)")]
    public float TimeScale = 1f;

    TMP_Text _tmp;
    TMP_MeshInfo[] _cachedMeshInfo; // ���� ���ؽ� ĳ��
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

        // 1) ���� ������ ������ �ؽ�Ʈ�� Vibrate ���� ���
        string display = Sentence.Evaluate(elapsed, out var vibrates);

        // 2) �ؽ�Ʈ�� �ٲ���ų�(Ÿ�� ����) ���� ���� �޶����� �޽� ���� �� ���� ���ؽ� ĳ��
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

        // 3) ���ؽ� ���� ����
        _samples.Clear();
        _samples.AddRange(vibrates);
        ApplyVibrations(elapsed);
    }

    void ApplyVibrations(float elapsed)
    {
        var textInfo = _tmp.textInfo;

        // �� ������ ���� ���ؽ��� ����
        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            var src = _cachedMeshInfo[i].vertices;
            var dst = textInfo.meshInfo[i].vertices;
            if (dst.Length != src.Length) continue;
            Array.Copy(src, dst, src.Length);
        }

        // �� ���� ���ڿ� ������ ����
        foreach (var v in _samples)
        {
            if (v.visibleCharIndex < 0 || v.visibleCharIndex >= textInfo.characterCount) continue;

            var charInfo = textInfo.characterInfo[v.visibleCharIndex];
            if (!charInfo.isVisible) continue;

            int matIndex = charInfo.materialReferenceIndex;
            int vIndex = charInfo.vertexIndex;

            // 1em�� ���� ���̷� �ٻ� (ascender - descender)
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

        // �޽��� �ݿ�
        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            var meshInfo = textInfo.meshInfo[i];
            meshInfo.mesh.vertices = meshInfo.vertices;
            _tmp.UpdateGeometry(meshInfo.mesh, i);
        }
    }
}
