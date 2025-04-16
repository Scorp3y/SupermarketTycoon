using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class LoadingManager : MonoBehaviour
{
    public Slider loadingBar;         
    public float fakeFillSpeed = 0.5f; 

    private float targetProgress = 0f;

    void Start()
    {
        StartCoroutine(LoadAsyncScene("Game")); 
    }

    IEnumerator LoadAsyncScene(string sceneName)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false; 

        while (!operation.isDone)
        {
            targetProgress = Mathf.Clamp01(operation.progress / 0.9f);

            while (loadingBar.value < targetProgress)
            {
                loadingBar.value += Time.deltaTime * fakeFillSpeed;
                yield return null;
            }

            if (operation.progress >= 0.9f && loadingBar.value >= 1f)
            {
                yield return new WaitForSeconds(0.5f); 
                operation.allowSceneActivation = true; 
            }

            yield return null;
        }
    }
}
