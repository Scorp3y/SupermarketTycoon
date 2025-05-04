using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using System.Collections.Generic;
using System;

public class LoadingManager : MonoBehaviour
{
    public Slider loadingBar;  
    public TextMeshProUGUI loadingText, infoText;
    public float fakeFillSpeed = 0.05f; 
    private float targetProgress = 0f;

    List<string> facts = new List<string> {
        "Fact 1: The first supermarket that changed the future of shopping opened in 1930 in St. Louis, Missouri. It was called \"Piggly Wiggly\" and was revolutionary because customers could choose items from the shelves themselves, rather than ordering from a salesperson, as was the case in traditional stores of the time.",
        "Fact 2: The first version of the game was created in 5 days, by one person and with a small amount of alcohol. Initially, the project was created as an experiment, the game was not intended for other players, but despite this, it became a full-fledged project.",
        "Fact 3: Walmart is the world's largest supermarket chain by revenue, with more than 11,000 stores in 27 countries. It was founded in 1962 by Sam Walton and remains a retail icon with a huge impact on the global economy.",
        "Fact 4: There is a tractor in the game that, according to the plot, helped develop the supermarket. This is a reference to a friend of the developer who almost became a policeman. The tractor became a kind of reference.",
        "Fact 5: In Australia, a supermarket cartel case was established where three of the largest supermarket chains agreed to raise prices on certain products. This led to increased scrutiny of such chains and the introduction of new anti-cartel laws.",
        "Fact 6: The idea for the game came from one popular Roblox mode, Retail Tycoon. After the developer was fascinated by the concept of managing a business and supermarkets, the idea for his own project was found.", };
    void Start()
    {
        RandomFact();
        StartCoroutine(LoadText());
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

    IEnumerator LoadText()
    {
        int dotCount = 0;
        while (true)
        {
            loadingText.text = "Loading" + new string('.', dotCount);
            dotCount = (dotCount + 1) % 4; 
            yield return new WaitForSeconds(0.5f);
        }
    }

    void RandomFact()
    {
        System.Random rnd = new System.Random();
        int index = rnd.Next(facts.Count);
        infoText.text = facts[index];
    }


}
