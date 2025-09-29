using System.Linq;
using UnityEngine;

[System.Serializable]
public class ExtendedSentence
{
    [SerializeField] [TextArea] string Sentence;
    [SerializeField] float IntervalDelayBetweenCharacters;


    enum TagType
    {
        Rich,
        Time,
        Vibrate
    }

    int FindFirstCharacterFromIndex(string str, char target, int startIndex)
    {
        for (int k = startIndex; k < str.Length; k++)
        {
            if (str[k] == target)
            {
                return k;
            }
        }
        return -1;
    }
    public string Evaluate(float time)
    {
        bool isOnTag = false;
        TagType type = TagType.Rich;
        float timer = 0f;
        string result = string.Empty;

        // Vibrate 상태
        bool vibrateActive = false;
        float vibDuration = 0f;

        // Y(세로)
        float vibLinAmpY = 0f, vibExpAmpY = 0f, vibFreqY = 0f, vibPhasePerCharY = 0f;

        // X(가로)
        float vibLinAmpX = 0f, vibExpAmpX = 0f, vibFreqX = 0f, vibPhasePerCharX = 0f;

        float vibStartTimer = 0f;
        int vibCharIndex = 0;

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
                if (vibrateActive)
                {
                    float elapsedForThisChar = time - (vibStartTimer + vibCharIndex * IntervalDelayBetweenCharacters);
                    float norm = (vibDuration > 0f) ? (elapsedForThisChar / vibDuration) : 0f;

                    // Y (em 단위로 voffset 적용)
                    float y = vibLinAmpY * Mathf.Exp(-vibExpAmpY * norm)
                              * Mathf.Cos(vibFreqY * norm + vibPhasePerCharY * vibCharIndex);

                    // X (링크 ID에 값 심고, 메쉬 단계에서 적용)
                    float x = vibLinAmpX * Mathf.Exp(-vibExpAmpX * norm)
                              * Mathf.Cos(vibFreqX * norm + vibPhasePerCharX * vibCharIndex);

                    // 각 글자를 독립적으로 감쌈
                    result += "<voffset=" + y.ToString("F2", System.Globalization.CultureInfo.InvariantCulture) + "em>"
                            + "<space=" + x.ToString("F2", System.Globalization.CultureInfo.InvariantCulture) + "em>"
                            + c
                            + "</voffset>";

                    vibCharIndex++;
                }
                else
                {
                    result += c;
                }

                // 타자 간격 시간 진행
                timer += IntervalDelayBetweenCharacters;
                if (timer >= time) break;
            }
            else if (isTagJustStarted)
            {
                switch (type)
                {
                    case TagType.Rich:
                        {
                            int end = FindFirstCharacterFromIndex(Sentence, '>', i + 1);
                            result += Sentence.Substring(i, end - i + 1);
                            break;
                        }
                    case TagType.Time:
                        {
                            int end = FindFirstCharacterFromIndex(Sentence, ']', i + 1);
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
                            // 닫힘 {/}
                            if (Sentence[i + 1] == '/')
                            {
                                vibrateActive = false;
                            }
                            else
                            {
                                int end = FindFirstCharacterFromIndex(Sentence, '}', i + 1);
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
                                vibStartTimer = timer;
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
