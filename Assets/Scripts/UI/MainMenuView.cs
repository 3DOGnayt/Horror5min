using UnityEngine;
using UnityEngine.UI;

namespace HorrorCafe.UI
{
    public sealed class MainMenuView : MonoBehaviour
    {
        [SerializeField] private MainMenuController controller;
        [SerializeField] private Button startButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button exitButton;

        private void Awake()
        {
            if (controller == null) 
                controller = GetComponent<MainMenuController>();
        }

        private void OnEnable() => Bind();

        private void OnDisable() => Unbind();

        private void Bind()
        {
            if (controller == null)
                return;

            Unbind();
            startButton?.onClick.AddListener(controller.StartGame);
            settingsButton?.onClick.AddListener(controller.OpenSettings);
            exitButton?.onClick.AddListener(controller.QuitGame);
        }

        private void Unbind()
        {
            if (controller == null)
                return;

            startButton?.onClick.RemoveListener(controller.StartGame);
            settingsButton?.onClick.RemoveListener(controller.OpenSettings);
            exitButton?.onClick.RemoveListener(controller.QuitGame);
        }
    }
}