using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LeaveHouse : MonoBehaviour
{
    public Transform Player;
    public CanvasGroup fadeCanvasGroup;
    private float fadeDuration = 2f;

    private bool isLeaving = false;

    void OnMouseOver()
    {
        if (Player && !isLeaving)
        {
            float dist = Vector3.Distance(Player.position, transform.position);

            if (dist < 15f && Input.GetMouseButtonDown(0))
            {
                LoadSceneWithFade("MainGameScene");
            }
        }
    }

    private IEnumerator FadeAndSwitchScene(string sceneName)
    {
        isLeaving = true;

        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Clamp01(timer / fadeDuration);
            yield return null;
        }

        fadeCanvasGroup.alpha = 1f;

        SceneManager.LoadScene(sceneName);
    }

    public void LoadSceneWithFade(string sceneName)
    {
        if (!isLeaving)
        {
            StartCoroutine(FadeAndSwitchScene(sceneName));
        }
    }
}
