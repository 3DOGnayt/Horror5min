using TMPro;
using UnityEngine;

namespace HorrorCafe.UI
{
    public sealed class FpsCounterUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text label;
        [SerializeField] private float refreshSeconds = 0.25f;
        
        private float elapsed;
        private int frames;
        private float timer;

        private void Update()
        {
            elapsed += Time.unscaledDeltaTime;
            timer += Time.unscaledDeltaTime;
            frames++;
            
            if (timer < refreshSeconds)
                return;
            
            var fps = elapsed > 0f ? Mathf.RoundToInt(frames / elapsed) : 0;
            
            if (label != null) 
                label.text = $"{fps} FPS";
            
            elapsed = 0f;
            timer = 0f;
            frames = 0;
        }
    }
}