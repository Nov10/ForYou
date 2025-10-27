using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ExtendedSentence
{
    [SerializeField, TextArea] string Sentence;
    [SerializeField] float IntervalDelayBetweenCharacters = 0.08f;

    [Header("Shock Wave (Square-like)")]
    [SerializeField, Tooltip("Shock 파형의 날카로움(↑ 더 각지게)")] public float ShockSharpness = 6f;
    [SerializeField, Tooltip("Shock 파형 계단 단계 수(0 또는 1이면 비활성)")] public int ShockStairSteps = 0;

    [Header("Chat Bubble")]
    public Vector2 Padding { get; private set; } = new Vector2(20, 20);
    public Vector2 MinSize { get; private set; } = new Vector2(80, 50);
    [SerializeField] public float ChatBubbleScaleDuration = 0.18f;
    [SerializeField] public float ChatKeepAliveDurationBeforeBubbleOff = 1.0f;

    enum TagType { Rich, Time, Vibrate }

    public enum Waveform { Smooth, Shock }

    [Serializable]
    public struct VibrateSample
    {
        public int visibleCharIndex;
        public int localIndex;
        public float startTime;
        public float duration;

        public float linAmpY, expAmpY, freqY, phasePerCharY;
        public float linAmpX, expAmpX, freqX, phasePerCharX;

        public Waveform waveform;
    }

    struct Preset
    {
        public Waveform waveform;
        public float duration;
        public float linY, expY, freqY, phaseY;
        public float linX, expX, freqX, phaseX;

        public Preset(Waveform wf, float d, float ly, float ey, float fy, float py,
                      float lx, float ex, float fx, float px)
        {
            waveform = wf; duration = d;
            linY = ly; expY = ey; freqY = fy; phaseY = py;
            linX = lx; expX = ex; freqX = fx; phaseX = px;
        }
    }

    static readonly Dictionary<string, Preset> s_presets = new(StringComparer.OrdinalIgnoreCase)
    {
        ["smooth_high"] = new Preset(Waveform.Smooth, 1.20f, 0.60f, 1.0f, 22f, 0.20f, 0.40f, 1.0f, 18f, 0.15f),
        ["smooth_middle"] = new Preset(Waveform.Smooth, 1.00f, 0.45f, 1.0f, 20f, 0.18f, 0.25f, 1.0f, 16f, 0.12f),
        ["smooth_low"] = new Preset(Waveform.Smooth, 0.80f, 0.30f, 1.0f, 14f, 0.12f, 0.15f, 1.0f, 12f, 0.08f),

        ["shock_high"] = new Preset(Waveform.Shock, 0.90f, 0.70f, 1.2f, 35f, 0.35f, 0.35f, 1.2f, 28f, 0.25f),
        ["shock_middle"] = new Preset(Waveform.Shock, 0.80f, 0.50f, 1.2f, 32f, 0.30f, 0.25f, 1.2f, 24f, 0.20f),
        ["shock_low"] = new Preset(Waveform.Shock, 0.70f, 0.35f, 1.2f, 30f, 0.25f, 0.15f, 1.2f, 20f, 0.15f),
    };

    int FindFirstCharacterFromIndex(string str, char target, int startIndex)
    {
        for (int k = startIndex; k < str.Length; k++)
            if (str[k] == target) return k;
        return -1;
    }

    // {vibrate:<preset>[:<durationSeconds>]} 파서
    bool TryApplyPreset(string raw, out Preset preset, out bool hasDurationOverride, out float durationOverride)
    {
        preset = default;
        hasDurationOverride = false;
        durationOverride = 0f;

        const string prefix = "vibrate:";
        if (!raw.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            return false;

        string rest = raw.Substring(prefix.Length).Trim();          // "smooth_high[:1.2]"
        if (string.IsNullOrEmpty(rest)) return false;

        // 이름과 지속시간 분리: smooth_high[:1.2]
        // 콜론이 여러 개여도 첫 토큰=이름, 두번째 토큰=지속시간으로 처리
        var parts = rest.Split(':');
        string name = parts[0].Trim();

        if (!s_presets.TryGetValue(name, out preset))
            return false;

        if (parts.Length >= 2)
        {
            string durToken = parts[1].Trim();
            if (durToken.EndsWith("s", StringComparison.OrdinalIgnoreCase))
                durToken = durToken.Substring(0, durToken.Length - 1).Trim();

            if (float.TryParse(durToken,
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture,
                out float d))
            {
                hasDurationOverride = true;
                durationOverride = d; // <=0 이면 무한으로 취급(아래 로직과 동일)
            }
            else
            {
                Debug.LogWarning($"ExtendedSentence: invalid duration '{parts[1]}' in preset tag '{raw}'. Using preset default.");
            }
        }

        return true;
    }

    /// <summary>
    /// time: 경과시간(초). 반환: 현재 시점 "보여줄" 텍스트.
    /// vibrates: 현재 보이는 문자들 중 진동 적용 대상 목록.
    ///
    /// 태그:
    /// - 리치: &lt;b&gt;...&lt;/b&gt; 등
    /// - 지연: [0.5]
    /// - 진동(프리셋): {vibrate:smooth_high[:1.2]} ... {/}
    /// - 진동(수치):   {dur/linY/expY/freqY[/phaseY]|linX/expX/freqX[/phaseX]} ... {/}
    /// - 종료: {/}
    ///
    /// duration이 0 이하이면 무한 지속으로 처리.
    /// </summary>
    public string Evaluate(float time, out List<VibrateSample> vibrates, out bool isEnd)
    {
        vibrates = new List<VibrateSample>();

        bool isOnTag = false;
        TagType type = TagType.Rich;
        float timer = 0f;
        string result = string.Empty;

        bool vibrateActive = false;
        float vibDuration = 0f;

        float vibLinAmpY = 0f, vibExpAmpY = 0f, vibFreqY = 0f, vibPhasePerCharY = 0f;
        float vibLinAmpX = 0f, vibExpAmpX = 0f, vibFreqX = 0f, vibPhasePerCharX = 0f;
        Waveform vibWaveform = Waveform.Shock;

        float vibStartTimer = 0f;
        int vibCharIndex = 0;
        int visibleIndex = 0;

        bool isend = false;

        for (int i = 0; i < Sentence.Length; i++)
        {
            if (i == Sentence.Length - 1)
            {
                isend = true;
            }

            char c = Sentence[i];
            bool isTagJustStarted = false;

            if (c == '<' || c == '[' || c == '{')
            {
                isOnTag = true;
                if (c == '<') type = TagType.Rich;
                if (c == '[') type = TagType.Time;
                if (c == '{') type = TagType.Vibrate;
                isTagJustStarted = true;
            }

            if (!isOnTag)
            {
                result += c;

                if (vibrateActive)
                {
                    float localStart = vibStartTimer + vibCharIndex * IntervalDelayBetweenCharacters;

                    vibrates.Add(new VibrateSample
                    {
                        visibleCharIndex = visibleIndex,
                        localIndex = vibCharIndex,
                        startTime = localStart,
                        duration = vibDuration,

                        linAmpY = vibLinAmpY,
                        expAmpY = vibExpAmpY,
                        freqY = vibFreqY,
                        phasePerCharY = vibPhasePerCharY,

                        linAmpX = vibLinAmpX,
                        expAmpX = vibExpAmpX,
                        freqX = vibFreqX,
                        phasePerCharX = vibPhasePerCharX,

                        waveform = vibWaveform
                    });

                    vibCharIndex++;
                }

                visibleIndex++;

                timer += IntervalDelayBetweenCharacters;
                if (timer >= time)
                {
                    break;
                }
            }
            else if (isTagJustStarted)
            {
                switch (type)
                {
                    case TagType.Rich:
                        {
                            int end = FindFirstCharacterFromIndex(Sentence, '>', i + 1);
                            if (end == -1) { isOnTag = false; break; }
                            result += Sentence.Substring(i, end - i + 1);
                            break;
                        }
                    case TagType.Time:
                        {
                            int end = FindFirstCharacterFromIndex(Sentence, ']', i + 1);
                            if (end == -1) { isOnTag = false; break; }

                            var delayStr = Sentence.Substring(i + 1, end - i - 1);
                            if (float.TryParse(delayStr,
                                System.Globalization.NumberStyles.Float,
                                System.Globalization.CultureInfo.InvariantCulture,
                                out var val))
                                timer += val;
                            else
                                Debug.LogError("ExtendedSentence: Cannot parse time tag to float: " + delayStr);
                            break;
                        }
                    case TagType.Vibrate:
                        {
                            if (i + 1 < Sentence.Length && Sentence[i + 1] == '/')
                            {
                                vibrateActive = false;
                            }
                            else
                            {
                                int end = FindFirstCharacterFromIndex(Sentence, '}', i + 1);
                                if (end == -1) { isOnTag = false; break; }

                                var raw = Sentence.Substring(i + 1, end - i - 1).Trim();

                                // 1) 프리셋(+선택적 지속시간) 우선
                                if (TryApplyPreset(raw, out var p, out bool hasOverride, out float durOverride))
                                {
                                    vibDuration = hasOverride ? durOverride : p.duration;

                                    vibLinAmpY = p.linY; vibExpAmpY = p.expY; vibFreqY = p.freqY; vibPhasePerCharY = p.phaseY;
                                    vibLinAmpX = p.linX; vibExpAmpX = p.expX; vibFreqX = p.freqX; vibPhasePerCharX = p.phaseX;

                                    vibWaveform = p.waveform;

                                    vibrateActive = true;
                                    vibStartTimer = timer;
                                    vibCharIndex = 0;
                                }
                                else
                                {
                                    // 2) 기존 수치 문법: "Y블록|X블록"
                                    var blocks = raw.Split('|');
                                    var yargs = blocks[0].Split('/');

                                    float Parse(string s, float dflt) =>
                                        float.TryParse(s,
                                            System.Globalization.NumberStyles.Float,
                                            System.Globalization.CultureInfo.InvariantCulture,
                                            out var v) ? v : dflt;

                                    // Y: dur/lin/exp/freq[/phase]
                                    vibDuration = (yargs.Length > 0) ? Parse(yargs[0], 1f) : 1f;
                                    vibLinAmpY = (yargs.Length > 1) ? Parse(yargs[1], 0.6f) : 0.6f;
                                    vibExpAmpY = (yargs.Length > 2) ? Parse(yargs[2], 1f) : 1f;
                                    vibFreqY = (yargs.Length > 3) ? Parse(yargs[3], 40f) : 40f;
                                    vibPhasePerCharY = (yargs.Length > 4) ? Parse(yargs[4], 0f) : 0f;

                                    vibLinAmpX = vibExpAmpX = vibFreqX = vibPhasePerCharX = 0f;
                                    if (blocks.Length > 1)
                                    {
                                        var xargs = blocks[1].Split('/');
                                        vibLinAmpX = (xargs.Length > 0) ? Parse(xargs[0], 0.0f) : 0.0f;
                                        vibExpAmpX = (xargs.Length > 1) ? Parse(xargs[1], 1f) : 1f;
                                        vibFreqX = (xargs.Length > 2) ? Parse(xargs[2], 25f) : 25f;
                                        vibPhasePerCharX = (xargs.Length > 3) ? Parse(xargs[3], 0f) : 0f;
                                    }

                                    // 수치 문법은 기존과 동일하게 Shock 파형 기본
                                    vibWaveform = Waveform.Shock;
                                    vibrateActive = true;
                                    vibStartTimer = timer;
                                    vibCharIndex = 0;
                                }
                            }
                            break;
                        }
                }
            }

            if (c == '>' || c == ']' || c == '}')
                isOnTag = false;
        }

        isEnd = isend;
        return result;
    }
}
