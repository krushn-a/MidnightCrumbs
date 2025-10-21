using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button credits;
    [SerializeField] private AudioSource AudioSource;

    private void Awake()
    {
        playButton.onClick.AddListener(() => 
        {
            Loader.Load(Loader.Scene.GameScene);
            AudioSource.Stop();
        });

        credits.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("Credit");
        });

        quitButton.onClick.AddListener(() =>
        {
            Application.Quit();
        });
    }
}
