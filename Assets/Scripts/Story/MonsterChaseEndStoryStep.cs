using System.Collections;
using HorrorCafe.Player;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace HorrorCafe.Story
{
    public sealed class MonsterChaseEndStoryStep : StoryStep
    {
        [SerializeField] private GameObject monsterObject;
        [SerializeField] private Transform playerTarget;
        [SerializeField] private HorrorPlayerController playerController;
        [SerializeField] private CameraLook cameraLook;
        [SerializeField] private Camera playerCamera;
        [SerializeField, Min(0.05f)] private float fallbackMoveSpeed = 2.2f;
        [SerializeField, Min(0.1f)] private float attackDistance = 1.2f;
        [SerializeField] private Animator monsterAnimator;
        [SerializeField] private string monsterWalkingBoolName = "IsWalking";
        [SerializeField] private string monsterAttackTriggerName = "Attack";
        [SerializeField] private AudioSource chaseAudioSource;
        [SerializeField] private AudioClip chaseClip;
        [SerializeField] private Vector3 cameraLookOffset = new Vector3(0f, 1.5f, 0f);
        [SerializeField, Min(0f)] private float lookAtDuration = 0.35f;
        [SerializeField, Min(0f)] private float attackDelay = 0.45f;
        [SerializeField, Min(0f)] private float blackScreenDelay = 0.15f;
        [SerializeField, Min(0f)] private float returnToMenuDelay = 2f;
        [SerializeField] private string menuSceneName = "Menu";
        [SerializeField] private string endText = "Вы прошли игру.";

        protected override IEnumerator Execute(StoryScenarioController controller)
        {
            if (monsterObject == null || playerTarget == null)
                yield break;

            monsterObject.SetActive(true);
            PlayChaseLoop();

            SetMonsterWalking(true);
            yield return MoveMonsterToPlayer();
            SetMonsterWalking(false);

            LockPlayer();
            yield return LookAtMonster();
            PlayAttack();

            if (attackDelay > 0f)
                yield return new WaitForSeconds(attackDelay);

            if (blackScreenDelay > 0f)
                yield return new WaitForSeconds(blackScreenDelay);

            StopChaseLoop();
            ShowEndScreen();

            if (returnToMenuDelay > 0f)
                yield return new WaitForSeconds(returnToMenuDelay);

            if (!string.IsNullOrWhiteSpace(menuSceneName))
            {
                ResetGlobalStateForMenu();
                SceneManager.LoadScene(menuSceneName);
            }
        }

        private IEnumerator MoveMonsterToPlayer()
        {
            var monsterTransform = monsterObject.transform;
            var agent = monsterObject.GetComponent<NavMeshAgent>();

            if (agent != null && agent.enabled && agent.isOnNavMesh)
            {
                agent.isStopped = false;

                while (Vector3.Distance(monsterTransform.position, playerTarget.position) > attackDistance)
                {
                    agent.SetDestination(playerTarget.position);
                    yield return null;
                }

                agent.isStopped = true;
                yield break;
            }

            while (Vector3.Distance(monsterTransform.position, playerTarget.position) > attackDistance)
            {
                monsterTransform.position = Vector3.MoveTowards(monsterTransform.position, playerTarget.position, fallbackMoveSpeed * Time.deltaTime);
                FaceTarget(monsterTransform, playerTarget.position, 8f);
                yield return null;
            }
        }

        private IEnumerator LookAtMonster()
        {
            if (playerCamera == null)
                yield break;

            var elapsed = 0f;
            var cameraTransform = playerCamera.transform;
            var startRotation = cameraTransform.rotation;
            var targetRotation = GetLookRotation(cameraTransform.position);

            while (elapsed < lookAtDuration)
            {
                elapsed += Time.deltaTime;
                var t = lookAtDuration <= 0f ? 1f : Mathf.Clamp01(elapsed / lookAtDuration);
                cameraTransform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
                yield return null;
            }

            cameraTransform.rotation = targetRotation;
        }

        private Quaternion GetLookRotation(Vector3 from)
        {
            var lookPosition = monsterObject.transform.position + cameraLookOffset;
            var direction = lookPosition - from;
            if (direction.sqrMagnitude < 0.0001f)
                return Quaternion.identity;

            return Quaternion.LookRotation(direction.normalized, Vector3.up);
        }

        private void FaceTarget(Transform transformToRotate, Vector3 targetPosition, float speed)
        {
            var direction = targetPosition - transformToRotate.position;
            direction.y = 0f;
            if (direction.sqrMagnitude <= 0.0001f)
                return;

            transformToRotate.rotation = Quaternion.Slerp(transformToRotate.rotation, Quaternion.LookRotation(direction), Time.deltaTime * speed);
        }

        private void LockPlayer()
        {
            if (playerController != null)
                playerController.ControlsEnabled = false;

            if (cameraLook != null)
                cameraLook.LookEnabled = false;
        }

        private void SetMonsterWalking(bool value)
        {
            if (monsterAnimator == null && monsterObject != null)
                monsterAnimator = monsterObject.GetComponentInChildren<Animator>(true);

            if (monsterAnimator == null || string.IsNullOrWhiteSpace(monsterWalkingBoolName))
                return;

            monsterAnimator.SetBool(monsterWalkingBoolName, value);
        }

        private void PlayAttack()
        {
            if (monsterAnimator == null && monsterObject != null)
                monsterAnimator = monsterObject.GetComponentInChildren<Animator>(true);

            if (monsterAnimator == null || string.IsNullOrWhiteSpace(monsterAttackTriggerName))
                return;

            monsterAnimator.SetTrigger(monsterAttackTriggerName);
        }

        private void PlayChaseLoop()
        {
            if (chaseAudioSource == null || chaseClip == null)
                return;

            chaseAudioSource.clip = chaseClip;
            chaseAudioSource.loop = true;
            chaseAudioSource.Play();
        }

        private void StopChaseLoop()
        {
            if (chaseAudioSource == null)
                return;

            chaseAudioSource.Stop();
            chaseAudioSource.loop = false;
        }

        private void ShowEndScreen()
        {
            var canvasObject = new GameObject("RuntimeEndScreen", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;

            var backgroundObject = new GameObject("BlackScreen", typeof(RectTransform), typeof(Image));
            backgroundObject.transform.SetParent(canvasObject.transform, false);
            var backgroundRect = backgroundObject.GetComponent<RectTransform>();
            backgroundRect.anchorMin = Vector2.zero;
            backgroundRect.anchorMax = Vector2.one;
            backgroundRect.offsetMin = Vector2.zero;
            backgroundRect.offsetMax = Vector2.zero;
            backgroundObject.GetComponent<Image>().color = Color.black;

            var textObject = new GameObject("EndText", typeof(RectTransform), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(canvasObject.transform, false);
            var textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            var label = textObject.GetComponent<TextMeshProUGUI>();
            label.text = endText;
            label.alignment = TextAlignmentOptions.Center;
            label.fontSize = 48f;
            label.color = Color.white;
        }

        private void ResetGlobalStateForMenu()
        {
            Time.timeScale = 1f;
            AudioListener.pause = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
