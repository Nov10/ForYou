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
        // 1. 로딩 씬 불러오기 (Additive)
        AsyncOperation loadingSceneOp =
            SceneManager.LoadSceneAsync(ConstValue.SCENE_INDEX_LoadingScene, LoadSceneMode.Additive);

        // 2. 로딩 씬 로드 완료까지 대기
        while (!loadingSceneOp.isDone)
        {
            yield return null;
        }

        // 이제 로딩 씬 안의 UI를 안전하게 찾을 수 있음
        //TMP_Text loadText = null;
        //Image loadImage = null;
        {
            // 로딩 씬을 직접 찾아서 그 씬 안에서만 찾는 방식이 더 안전하다.
            Scene loadingScene = SceneManager.GetSceneByBuildIndex(ConstValue.SCENE_INDEX_LoadingScene);

            // 혹시 로딩씬이 아직 활성화 안 돼있으면, 활성화 먼저
            if (!loadingScene.isLoaded)
            {
                // 이 상황은 거의 없지만 방어적으로 한 번 더 기다려줌
                yield return null;
            }

            // 로딩 씬을 활성 씬으로 잠깐 바꿔두면 Find가 꼬일 일이 줄어듬
            SceneManager.SetActiveScene(loadingScene);

            // 직접 GameObject.Find 써도 되지만, 더 안전하게는
            // 로딩씬의 root objects에서 찾아가는 방식:
            foreach (var rootObj in loadingScene.GetRootGameObjects())
            {
                //if (rootObj.name == "LoadingText")
                //{
                //    loadText = rootObj.GetComponent<TMP_Text>();
                //}

                //// 혹시 LoadingText가 자식에 있다면:
                //var s = TransformFinder.FindChild(rootObj.transform, "LoadingText");
                //if (s != null && s.TryGetComponent<TMP_Text>(out var txt))
                //{
                //    loadText = txt;
                //}

                //// 혹시 LoadingText가 자식에 있다면:
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

        // 3. 기존 씬들(로딩 씬 제외) 언로드하고 끝날 때까지 기다리기
        {
            // 먼저 어떤 씬들을 지울지 목록만 따로 뽑아 (반복문 돌면서 언로드하면 sceneCount가 바뀌니까)
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

            // 실제 언로드
            List<AsyncOperation> unloadOps = new List<AsyncOperation>();
            foreach (var s in scenesToUnload)
            {
                if (s.isLoaded)
                {
                    unloadOps.Add(SceneManager.UnloadSceneAsync(s));
                }
            }

            // 전부 끝날 때까지 기다리기
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

        // 4. 목표 씬 로드 시작 (Additive)
        AsyncOperation targetLoadOp =
            SceneManager.LoadSceneAsync(sceneIndex, LoadSceneMode.Additive);

        // (선택) allowSceneActivation 제어하고 싶으면 여기서:
        // targetLoadOp.allowSceneActivation = false;

        // progress 업데이트
        while (!targetLoadOp.isDone)
        {
            //if (loadText != null)
            //{
            //    // Unity는 0~0.9까지 올라가다가 마지막에 한번에 끝나므로
            //    float p = Mathf.Clamp01(targetLoadOp.progress / 0.9f);
            //    int percent = Mathf.RoundToInt(p * 100f);
            //    loadText.text = "Now Loading... " + percent + "%";
            //}

            // 만약 allowSceneActivation=false를 쓴다면,
            // p가 1.0(=90%) 근처 도달했을 때 UI 바꾸고
            // 어떤 조건에서 allowSceneActivation=true로 돌리는 식으로 하면 됨.

            yield return null;
        }


        // 5. 새 씬을 ActiveScene으로 지정
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
        // 6. 로딩 씬 언로드
        SceneManager.UnloadSceneAsync(ConstValue.SCENE_INDEX_LoadingScene);
    }

}
