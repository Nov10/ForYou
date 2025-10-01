using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class ExtendedSentence
{
    [SerializeField, TextArea] string Sentence;
    [SerializeField] float IntervalDelayBetweenCharacters = 0.03f;

    enum TagType { Rich, Time, Vibrate }

    // 버텍스 애니메이션에서 사용할 1문자 단위 샘플
    [Serializable]
    public struct VibrateSample
    {
        public int visibleCharIndex; // TMP 상에서의 가시 문자 인덱스
        public int localIndex;       // 해당 Vibrate 구간 내 n번째 문자 (위상 증가용)
        public float startTime;      // 이 문자의 진동 시작 시간(타자 효과 반영, Evaluate 기준 시간축)
        public float duration;
        public float linAmpY, expAmpY, freqY, phasePerCharY;
        public float linAmpX, expAmpX, freqX, phasePerCharX;
    }

    int FindFirstCharacterFromIndex(string str, char target, int startIndex)
    {
        for (int k = startIndex; k < str.Length; k++)
            if (str[k] == target) return k;
        return -1;
    }

    /// <summary>
    /// time: 타자/효과를 위한 경과시간(초). 리턴은 현재 시점에 보여줄 텍스트.
    /// out vibrates: 현재 '보이는' 문자들 중 진동해야 할 문자들의 파라미터 목록.
    /// </summary>
    public string Evaluate(float time, out List<VibrateSample> vibrates)
    {
        vibrates = new List<VibrateSample>();

        bool isOnTag = false;
        TagType type = TagType.Rich;
        float timer = 0f;                  // 타자 진행용 누적 타이머
        string result = string.Empty;

        // Vibrate 구간 상태
        bool vibrateActive = false;
        float vibDuration = 0f;

        // Y(세로)
        float vibLinAmpY = 0f, vibExpAmpY = 0f, vibFreqY = 0f, vibPhasePerCharY = 0f;
        // X(가로)
        float vibLinAmpX = 0f, vibExpAmpX = 0f, vibFreqX = 0f, vibPhasePerCharX = 0f;

        float vibStartTimer = 0f; // 해당 Vibrate 구간의 시작 타임(타자 기준 시간)
        int vibCharIndex = 0;     // Vibrate 구간 내 가시문자 순번

        int visibleIndex = 0;     // TMP 상의 '보이는' 문자 인덱스

        for (int i = 0; i < Sentence.Length; i++)
        {
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
                // 가시 글자 1개 출력
                result += c;

                // 현재 글자가 Vibrate 적용 대상이면 샘플을 기록(버텍스에서 사용)
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
                    });

                    vibCharIndex++;
                }

                visibleIndex++;

                // 타자 간격 시간 진행
                timer += IntervalDelayBetweenCharacters;
                if (timer >= time) break; // 아직 보여줄 시간이 안 된 경우 중단
            }
            else if (isTagJustStarted)
            {
                switch (type)
                {
                    case TagType.Rich:
                        {
                            // TMP 리치태그는 텍스트에 그대로 포함 (가시문자 카운트에는 포함 안 됨)
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
                            if (float.TryParse(delayStr, System.Globalization.NumberStyles.Float,
                                System.Globalization.CultureInfo.InvariantCulture, out var val))
                                timer += val;
                            else
                                Debug.LogError("ExtendedSentence: Cannot parse time tag to float: " + delayStr);
                            break;
                        }
                    case TagType.Vibrate:
                        {
                            // "{/}" -> 구간 종료
                            if (i + 1 < Sentence.Length && Sentence[i + 1] == '/')
                            {
                                vibrateActive = false;
                            }
                            else
                            {
                                int end = FindFirstCharacterFromIndex(Sentence, '}', i + 1);
                                if (end == -1) { isOnTag = false; break; }

                                var raw = Sentence.Substring(i + 1, end - i - 1);

                                // "Y블록|X블록" 으로 분리 (X블록은 선택)
                                var blocks = raw.Split('|');
                                var yargs = blocks[0].Split('/');

                                float Parse(string s, float dflt) =>
                                    float.TryParse(s, System.Globalization.NumberStyles.Float,
                                        System.Globalization.CultureInfo.InvariantCulture, out var v) ? v : dflt;

                                // Y: dur/lin/exp/freq[/phase]
                                vibDuration = (yargs.Length > 0) ? Parse(yargs[0], 1f) : 1f;
                                vibLinAmpY = (yargs.Length > 1) ? Parse(yargs[1], 0.5f) : 0.5f;
                                vibExpAmpY = (yargs.Length > 2) ? Parse(yargs[2], 1f) : 1f;
                                vibFreqY = (yargs.Length > 3) ? Parse(yargs[3], 40f) : 40f;
                                vibPhasePerCharY = (yargs.Length > 4) ? Parse(yargs[4], 0f) : 0f;

                                // X: lin/exp/freq[/phase]  (없으면 X 흔들림 꺼짐)
                                vibLinAmpX = vibExpAmpX = vibFreqX = vibPhasePerCharX = 0f;
                                if (blocks.Length > 1)
                                {
                                    var xargs = blocks[1].Split('/');
                                    vibLinAmpX = (xargs.Length > 0) ? Parse(xargs[0], 0f) : 0f;
                                    vibExpAmpX = (xargs.Length > 1) ? Parse(xargs[1], 1f) : 1f;
                                    vibFreqX = (xargs.Length > 2) ? Parse(xargs[2], 40f) : 40f;
                                    vibPhasePerCharX = (xargs.Length > 3) ? Parse(xargs[3], 0f) : 0f;
                                }

                                vibrateActive = true;
                                vibStartTimer = timer; // 현재 타자 진행 시각을 구간 시작으로
                                vibCharIndex = 0;
                            }
                            break;
                        }
                }
            }

            if (c == '>' || c == ']' || c == '}')
                isOnTag = false;
        }
        return result;
    }
}
