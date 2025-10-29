using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using URPGlitch;

namespace Opening
{
    [ExecuteInEditMode]
    public class OpeningSceneVolumeController : MonoBehaviour
    {
        public Volume volume;
        private ChromaticAberration chromaticAberration;
        private LensDistortion lensDistortion;
        private Vignette vignette;
        private AnalogGlitchVolume analogGlitchVolume;
        private DigitalGlitchVolume digitalGlitchVolume;

        [Header("Chromatic Aberration Ping-Pong")]
        [Range(0f, 1f)] public float chromaticIntensityMin = 0f;
        [Range(0f, 1f)] public float chromaticIntensityMax = 1f;
        [Min(0.01f)] public float chromaticPingPongSeconds = 2f;

        [Header("Lens Distortion Ping-Pong")]
        [Range(-1f, 1f)] public float lensDistortionIntensityMin = 0f;
        [Range(-1f, 1f)] public float lensDistortionIntensityMax = 0.5f;
        [Min(0.01f)] public float lensDistortionPingPongSeconds = 2f;
        public Vector2 lensDistortionCenter = new Vector2(0.5f, 0.5f);

        [Header("Vignette Ping-Pong + Manual Color")]
        [Range(0f, 1f)] public float vignetteIntensityMin = 0f;
        [Range(0f, 1f)] public float vignetteIntensityMax = 0.45f;
        [Min(0.01f)] public float vignettePingPongSeconds = 2f;
        public Color vignetteColor = Color.black;

        [Header("Sprite Renderer Color Ping-Pong")]
        public SpriteRenderer spriteRenderer;
        public Color spriteColorA = Color.white;
        public Color spriteColorB = Color.gray;
        [Min(0.01f)] public float spriteColorPingPongSeconds = 2f;

        [Header("Glitch Effects Target Values")]
        [Range(0f, 1f)] public float targetAnalogScanLineJitter = 0f;
        [Range(0f, 1f)] public float targetAnalogVerticalJump = 0f;
        [Range(0f, 1f)] public float targetAnalogColorDrift = 0f;
        [Range(0f, 1f)] public float targetDigitalIntensity = 0f;
        [Min(0.01f)] public float glitchTransitionDuration = 1f;

        [Header("References")]
        public Transform Dream1;
        public Transform Dream2;
        public Transform Dream3;

        // Lens distortion center animation state
        private bool isAnimatingCenter = false;
        private float centerAnimStartTime;

        // Glitch effects animation state
        private bool isAnimatingGlitch = false;
        private float glitchAnimStartTime;
        private float startAnalogScanLineJitter;
        private float startAnalogVerticalJump;
        private float startAnalogColorDrift;
        private float startDigitalIntensity;

        private void Awake()
        {
            volume.profile.TryGet(out chromaticAberration);
            volume.profile.TryGet(out lensDistortion);
            volume.profile.TryGet(out vignette);
            volume.profile.TryGet(out analogGlitchVolume);
            volume.profile.TryGet(out digitalGlitchVolume);

            if (lensDistortion != null)
            {
                lensDistortion.intensity.overrideState = true;
                lensDistortion.center.overrideState = true;
                lensDistortionCenter = lensDistortion.center.value;
            }

            if (chromaticAberration != null)
            {
                chromaticAberration.intensity.overrideState = true;
            }

            if (vignette != null)
            {
                vignetteColor = vignette.color.value;
                vignette.intensity.overrideState = true;
                vignette.color.overrideState = true;
            }

            if (analogGlitchVolume != null)
            {
                analogGlitchVolume.scanLineJitter.overrideState = true;
                analogGlitchVolume.verticalJump.overrideState = true;
                analogGlitchVolume.colorDrift.overrideState = true;
            }

            if (digitalGlitchVolume != null)
            {
                digitalGlitchVolume.intensity.overrideState = true;
            }
        }

        private void Update()
        {
            if (chromaticAberration != null)
            {
                float tChroma = chromaticPingPongSeconds > 0.0001f
                    ? Mathf.PingPong(Time.time, chromaticPingPongSeconds) / chromaticPingPongSeconds
                    : 0f;
                chromaticAberration.intensity.value = Mathf.Lerp(chromaticIntensityMin, chromaticIntensityMax, tChroma);
            }

            if (lensDistortion != null)
            {
                float tLens = lensDistortionPingPongSeconds > 0.0001f
                    ? Mathf.PingPong(Time.time, lensDistortionPingPongSeconds) / lensDistortionPingPongSeconds
                    : 0f;
                lensDistortion.intensity.value = Mathf.Lerp(lensDistortionIntensityMin, lensDistortionIntensityMax, tLens);

                // Update center animation if active
                if (isAnimatingCenter)
                {
                    float elapsed = Time.time - centerAnimStartTime;
                    float centerX;

                    if (elapsed < 0.2f)
                    {
                        // Phase 1: 0.5 -> -0.25 over 0.5 seconds
                        centerX = Mathf.Lerp(0.5f, -0.25f, elapsed / 0.2f);
                    }
                    else if (elapsed < 0.4f)
                    {
                        // Phase 2: 1.25 -> 0.5 over 0.5 seconds (0.5s to 1.0s)
                        centerX = Mathf.Lerp(1.25f, 0.5f, (elapsed - 0.2f) / 0.2f);
                    }
                    else
                    {
                        // Animation complete
                        centerX = 0.5f;
                        isAnimatingCenter = false;
                    }

                    lensDistortionCenter.x = centerX;
                }

                lensDistortion.center.value = lensDistortionCenter;
            }

            if (vignette != null)
            {
                float tVignette = vignettePingPongSeconds > 0.0001f
                    ? Mathf.PingPong(Time.time, vignettePingPongSeconds) / vignettePingPongSeconds
                    : 0f;
                vignette.intensity.value = Mathf.Lerp(vignetteIntensityMin, vignetteIntensityMax, tVignette);
                vignette.color.value = vignetteColor;
            }

            if (spriteRenderer != null)
            {
                float tSprite = spriteColorPingPongSeconds > 0.0001f
                    ? Mathf.PingPong(Time.time, spriteColorPingPongSeconds) / spriteColorPingPongSeconds
                    : 0f;
                spriteRenderer.color = Color.Lerp(spriteColorA, spriteColorB, tSprite);
            }

            // Update glitch effects animation if active
            if (isAnimatingGlitch)
            {
                float elapsed = Time.time - glitchAnimStartTime;
                float t = glitchTransitionDuration > 0.0001f ? Mathf.Clamp01(elapsed / glitchTransitionDuration) : 1f;

                float currentScanLineJitter = Mathf.Lerp(startAnalogScanLineJitter, targetAnalogScanLineJitter, t);
                float currentVerticalJump = Mathf.Lerp(startAnalogVerticalJump, targetAnalogVerticalJump, t);
                float currentColorDrift = Mathf.Lerp(startAnalogColorDrift, targetAnalogColorDrift, t);
                float currentDigitalIntensity = Mathf.Lerp(startDigitalIntensity, targetDigitalIntensity, t);

                if (analogGlitchVolume != null)
                {
                    analogGlitchVolume.scanLineJitter.value = currentScanLineJitter;
                    analogGlitchVolume.verticalJump.value = currentVerticalJump;
                    analogGlitchVolume.colorDrift.value = currentColorDrift;
                }

                if (digitalGlitchVolume != null)
                {
                    digitalGlitchVolume.intensity.value = currentDigitalIntensity;
                }

                // Animation complete
                if (t >= 1f)
                {
                    isAnimatingGlitch = false;
                }
            }
        }

        /// <summary>
        /// 开始镜头畸变中心点 X 轴动画：
        /// 0.5 -> -0.25 (0.2秒) -> 突变至 1.25 -> 0.5 (0.2秒)
        /// </summary>
        public void AnimateLensDistortionCenter()
        {
            isAnimatingCenter = true;
            centerAnimStartTime = Time.time;
        }

        /// <summary>
        /// 开始 Glitch 效果动画，平滑过渡到 Inspector 中设置的目标值
        /// 可通过 UnityEvent 调用（无参数）
        /// </summary>
        public void PlayGlitchEffects()
        {
            // 记录当前值作为起始值
            if (analogGlitchVolume != null)
            {
                startAnalogScanLineJitter = analogGlitchVolume.scanLineJitter.value;
                startAnalogVerticalJump = analogGlitchVolume.verticalJump.value;
                startAnalogColorDrift = analogGlitchVolume.colorDrift.value;
            }
            else
            {
                startAnalogScanLineJitter = 0f;
                startAnalogVerticalJump = 0f;
                startAnalogColorDrift = 0f;
            }

            if (digitalGlitchVolume != null)
            {
                startDigitalIntensity = digitalGlitchVolume.intensity.value;
            }
            else
            {
                startDigitalIntensity = 0f;
            }

            // 开始动画
            isAnimatingGlitch = true;
            glitchAnimStartTime = Time.time;
        }
        
        public void SwitchToScene(string sceneName)
        {
            SceneMng.Instance.SwitchScene(sceneName);
        }
    }


}