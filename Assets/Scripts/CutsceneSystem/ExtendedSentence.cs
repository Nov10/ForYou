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

    // ���ؽ� �ִϸ��̼ǿ��� ����� 1���� ���� ����
    [Serializable]
    public struct VibrateSample
    {
        public int visibleCharIndex; // TMP �󿡼��� ���� ���� �ε���
        public int localIndex;       // �ش� Vibrate ���� �� n��° ���� (���� ������)
        public float startTime;      // �� ������ ���� ���� �ð�(Ÿ�� ȿ�� �ݿ�, Evaluate ���� �ð���)
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
    /// time: Ÿ��/ȿ���� ���� ����ð�(��). ������ ���� ������ ������ �ؽ�Ʈ.
    /// out vibrates: ���� '���̴�' ���ڵ� �� �����ؾ� �� ���ڵ��� �Ķ���� ���.
    /// </summary>
    public string Evaluate(float time, out List<VibrateSample> vibrates)
    {
        vibrates = new List<VibrateSample>();

        bool isOnTag = false;
        TagType type = TagType.Rich;
        float timer = 0f;                  // Ÿ�� ����� ���� Ÿ�̸�
        string result = string.Empty;

        // Vibrate ���� ����
        bool vibrateActive = false;
        float vibDuration = 0f;

        // Y(����)
        float vibLinAmpY = 0f, vibExpAmpY = 0f, vibFreqY = 0f, vibPhasePerCharY = 0f;
        // X(����)
        float vibLinAmpX = 0f, vibExpAmpX = 0f, vibFreqX = 0f, vibPhasePerCharX = 0f;

        float vibStartTimer = 0f; // �ش� Vibrate ������ ���� Ÿ��(Ÿ�� ���� �ð�)
        int vibCharIndex = 0;     // Vibrate ���� �� ���ù��� ����

        int visibleIndex = 0;     // TMP ���� '���̴�' ���� �ε���

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
                // ���� ���� 1�� ���
                result += c;

                // ���� ���ڰ� Vibrate ���� ����̸� ������ ���(���ؽ����� ���)
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

                // Ÿ�� ���� �ð� ����
                timer += IntervalDelayBetweenCharacters;
                if (timer >= time) break; // ���� ������ �ð��� �� �� ��� �ߴ�
            }
            else if (isTagJustStarted)
            {
                switch (type)
                {
                    case TagType.Rich:
                        {
                            // TMP ��ġ�±״� �ؽ�Ʈ�� �״�� ���� (���ù��� ī��Ʈ���� ���� �� ��)
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
                            // "{/}" -> ���� ����
                            if (i + 1 < Sentence.Length && Sentence[i + 1] == '/')
                            {
                                vibrateActive = false;
                            }
                            else
                            {
                                int end = FindFirstCharacterFromIndex(Sentence, '}', i + 1);
                                if (end == -1) { isOnTag = false; break; }

                                var raw = Sentence.Substring(i + 1, end - i - 1);

                                // "Y���|X���" ���� �и� (X����� ����)
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

                                // X: lin/exp/freq[/phase]  (������ X ��鸲 ����)
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
                                vibStartTimer = timer; // ���� Ÿ�� ���� �ð��� ���� ��������
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
