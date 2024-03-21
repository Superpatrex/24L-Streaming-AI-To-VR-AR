using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Loader
{
    private class LoadingMonoBehavior : MonoBehaviour { }
    public enum Scene { 
        StartMenu,
        LoadingScene,
        JackCesiumEnvironment,
        
    }

    private static Action onLoaderCallback;
    private static AsyncOperation loadingAsyncOperation;
   


    public static void Load(Scene scene) {


  

        onLoaderCallback = () => {
            GameObject loadingGameObject = new GameObject("Loading Game Object");
            loadingGameObject.AddComponent<LoadingMonoBehavior>().StartCoroutine(LoadSceneAsync(scene));
            
        };

        SceneManager.LoadScene(Scene.LoadingScene.ToString());

        // Loader.Load(Loader.Scene.CesiumEnvironment);

    }

    private static IEnumerator LoadSceneAsync(Scene scene) {
        // yield return new WaitForSeconds(5);
        //yield return null;

        //fadeScreen.FadeOut();

        AsyncOperation asyncOperation =  SceneManager.LoadSceneAsync(scene.ToString());
        asyncOperation.allowSceneActivation = false;

        float timer = 0;

        /*while (!loadingAsyncOperation.isDone) {
         
            yield return null;
        }*/

        while (timer <= 2 && !asyncOperation.isDone)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        asyncOperation.allowSceneActivation = true;

    }

    public static float GetLoadingProgress() {

        if (loadingAsyncOperation != null){

            return loadingAsyncOperation.progress;

        }
        else {
            return 1f;
        }
    }


    public static void LoaderCallback() {

        if (onLoaderCallback != null)
        {
            onLoaderCallback();
            onLoaderCallback = null;
        }

    }
}
