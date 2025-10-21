using UnityEngine;
using UnityEngine.SceneManagement;
public class CreditScript : MonoBehaviour
{
    public void BackToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
