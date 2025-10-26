using Helpers;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class SceneLoader
{
    public static void LoadScene(int sceneIndex)
    {
        ExtendedEnumeratorRunner.Instance.StartCoroutine(_LoadScene(sceneIndex));
    }

    static IEnumerator _LoadScene(int sceneIndex)
    {
        // 1. �ε� �� �ҷ����� (Additive)
        AsyncOperation loadingSceneOp =
            SceneManager.LoadSceneAsync(ConstValue.SCENE_INDEX_LoadingScene, LoadSceneMode.Additive);

        // 2. �ε� �� �ε� �Ϸ���� ���
        while (!loadingSceneOp.isDone)
        {
            yield return null;
        }

        // ���� �ε� �� ���� UI�� �����ϰ� ã�� �� ����
        //TMP_Text loadText = null;
        //Image loadImage = null;
        {
            // �ε� ���� ���� ã�Ƽ� �� �� �ȿ����� ã�� ����� �� �����ϴ�.
            Scene loadingScene = SceneManager.GetSceneByBuildIndex(ConstValue.SCENE_INDEX_LoadingScene);

            // Ȥ�� �ε����� ���� Ȱ��ȭ �� ��������, Ȱ��ȭ ����
            if (!loadingScene.isLoaded)
            {
                // �� ��Ȳ�� ���� ������ ��������� �� �� �� ��ٷ���
                yield return null;
            }

            // �ε� ���� Ȱ�� ������ ��� �ٲ�θ� Find�� ���� ���� �پ��
            SceneManager.SetActiveScene(loadingScene);

            // ���� GameObject.Find �ᵵ ������, �� �����ϰԴ�
            // �ε����� root objects���� ã�ư��� ���:
            foreach (var rootObj in loadingScene.GetRootGameObjects())
            {
                //if (rootObj.name == "LoadingText")
                //{
                //    loadText = rootObj.GetComponent<TMP_Text>();
                //}

                //// Ȥ�� LoadingText�� �ڽĿ� �ִٸ�:
                //var s = TransformFinder.FindChild(rootObj.transform, "LoadingText");
                //if (s != null && s.TryGetComponent<TMP_Text>(out var txt))
                //{
                //    loadText = txt;
                //}

                //// Ȥ�� LoadingText�� �ڽĿ� �ִٸ�:
                //s = TransformFinder.FindChild(rootObj.transform, "LoadingImage");
                //if (s != null && s.TryGetComponent<Image>(out var img))
                //{
                //    loadImage = img;
                //}
            }
        }
        //EditorApplication.isPaused = true;
        //loadText.gameObject.SetActive(false);
        //loadImage.CrossFadeAlpha(0.0f, 0.0f, true);
        //loadImage.CrossFadeAlpha(1.0f, 1.0f, true);
        yield return new WaitForSeconds(1.0f);
        //loadText.gameObject.SetActive(true);
        //EditorApplication.isPaused = true;

        // 3. ���� ����(�ε� �� ����) ��ε��ϰ� ���� ������ ��ٸ���
        {
            // ���� � ������ ������ ��ϸ� ���� �̾� (�ݺ��� ���鼭 ��ε��ϸ� sceneCount�� �ٲ�ϱ�)
            List<Scene> scenesToUnload = new List<Scene>();
            int sceneCount = SceneManager.sceneCount;
            for (int i = 0; i < sceneCount; i++)
            {
                Scene s = SceneManager.GetSceneAt(i);
                if (s.buildIndex != ConstValue.SCENE_INDEX_LoadingScene)
                {
                    scenesToUnload.Add(s);
                }
            }

            // ���� ��ε�
            List<AsyncOperation> unloadOps = new List<AsyncOperation>();
            foreach (var s in scenesToUnload)
            {
                if (s.isLoaded)
                {
                    unloadOps.Add(SceneManager.UnloadSceneAsync(s));
                }
            }

            // ���� ���� ������ ��ٸ���
            bool unloading = true;
            while (unloading)
            {
                unloading = false;
                foreach (var op in unloadOps)
                {
                    if (op != null && !op.isDone)
                    {
                        unloading = true;
                        break;
                    }
                }
                yield return null;
            }
        }

        // 4. ��ǥ �� �ε� ���� (Additive)
        AsyncOperation targetLoadOp =
            SceneManager.LoadSceneAsync(sceneIndex, LoadSceneMode.Additive);

        // (����) allowSceneActivation �����ϰ� ������ ���⼭:
        // targetLoadOp.allowSceneActivation = false;

        // progress ������Ʈ
        while (!targetLoadOp.isDone)
        {
            //if (loadText != null)
            //{
            //    // Unity�� 0~0.9���� �ö󰡴ٰ� �������� �ѹ��� �����Ƿ�
            //    float p = Mathf.Clamp01(targetLoadOp.progress / 0.9f);
            //    int percent = Mathf.RoundToInt(p * 100f);
            //    loadText.text = "Now Loading... " + percent + "%";
            //}

            // ���� allowSceneActivation=false�� ���ٸ�,
            // p�� 1.0(=90%) ��ó �������� �� UI �ٲٰ�
            // � ���ǿ��� allowSceneActivation=true�� ������ ������ �ϸ� ��.

            yield return null;
        }


        // 5. �� ���� ActiveScene���� ����
        {
            Scene newScene = SceneManager.GetSceneByBuildIndex(sceneIndex);
            if (newScene.IsValid() && newScene.isLoaded)
            {
                SceneManager.SetActiveScene(newScene);
            }
        }
        yield return new WaitForSeconds(1.0f);
        GameObject.FindFirstObjectByType<LoadingScene>().Hide();
       // loadText.gameObject.SetActive(false);
        //loadImage.CrossFadeAlpha(0.0f, 1.0f, true);
        yield return new WaitForSeconds(1.5f);
        // 6. �ε� �� ��ε�
        SceneManager.UnloadSceneAsync(ConstValue.SCENE_INDEX_LoadingScene);
    }

}
