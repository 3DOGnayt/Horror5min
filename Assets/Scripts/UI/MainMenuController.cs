using UnityEngine;
using UnityEngine.SceneManagement;

namespace HorrorCafe.UI
{
    public sealed class MainMenuController : MonoBehaviour
    {
        [SerializeField] private int gameSceneBuildIndex = 1;
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private SettingsController settings;

        public void StartGame() => SceneManager.LoadScene(gameSceneBuildIndex);

        public void OpenSettings()
        {
            settings?.OpenSettings();
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void Start()
        {
            Time.timeScale = 1f;
            AudioListener.pause = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (mainPanel != null) 
                mainPanel.SetActive(true);
            
            settings?.CloseSettings();
        }
    }
}
