using System;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private Button playButton;

    private void Awake()
    {
        playButton.onClick.AddListener(LoadGame);
    }

    private void LoadGame()
    {
        ServiceLocator.Get<SceneService>().LoadGameSceneAsync().Forget();
    }
}
