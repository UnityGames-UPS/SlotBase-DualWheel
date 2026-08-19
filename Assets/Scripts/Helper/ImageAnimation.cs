using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageAnimation : MonoBehaviour
{
    public enum ImageState
    {
        NONE,
        PLAYING,
        PAUSED
    }

    public enum AnimationMode
    {
        SINGLE_PHASE,
        TWO_PHASE
    }

    public static ImageAnimation Instance;

    public List<Sprite> textureArray;
    public Image rendererDelegate;
    public bool useSharedMaterial = true;
    public bool doLoopAnimation = true;
    
    [Header("Dynamic Timing")]
    public bool useDynamicFramerate = false;
    public float dynamicLoopDuration = 1.0f;
    [Tooltip("Target frame rate (FPS) for smooth animation. When useDynamicFramerate is true, loop count adaptively scales to maintain smooth speed near this FPS.")]
    public float targetFPS = 24.0f;
    
    public System.Action<int> onLoopComplete;
    private int currentLoopCount = 0;
    
    [SerializeField] private bool StartOnAwake;
    [SerializeField] private bool StartonEnable;

    [HideInInspector]
    public ImageState currentAnimationState;

    private int indexOfTexture;
    private float idealFrameRate = 0.0416666679f; // ~24 fps
    private float delayBetweenAnimation;

    public float AnimationSpeed = 5f;
    public float delayBetweenLoop;

    [Header("Two Phase Animation (Optional)")]
    public AnimationMode animationMode = AnimationMode.SINGLE_PHASE;
    
    [Tooltip("Index where Phase 2 starts (Phase 1 is 0 to this index-1)")]
    public int phase2StartIndex = 0;
    
    [Tooltip("How many times Phase 1 should loop (-1 = infinite, 0 = skip phase 1, 1+ = specific count)")]
    public int phase1LoopCount = 1;
    
    [Tooltip("How many times Phase 2 should loop (-1 = infinite, 0 = skip phase 2, 1+ = specific count)")]
    public int phase2LoopCount = -1;

    private int currentPhase = 1;
    private int phase1CurrentLoop = 0;
    private int phase2CurrentLoop = 0;

    // Time-sync tracking to prevent Invoke drift across active animations
    private float animStartTime;
    private float pauseStartTime;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        EnsureRenderer();
        if (StartOnAwake)
        {
            StartAnimation();
        }
    }

    private void EnsureRenderer()
    {
        if (rendererDelegate == null)
        {
            rendererDelegate = GetComponent<Image>();
        }
    }

    void Start()
    {
        EnsureRenderer();
    }

    private void OnEnable()
    {
        EnsureRenderer();
        if (StartonEnable)
        {
            StartAnimation();
        }
    }

    private void OnDisable()
    {
        StopAnimation();
    }

    public void CalculateFrameDelay()
    {
        if (textureArray == null || textureArray.Count == 0)
        {
            delayBetweenAnimation = 0.0416666679f;
            return;
        }

        if (useDynamicFramerate && dynamicLoopDuration > 0f)
        {
            int frameCount = textureArray.Count;
            if (animationMode == AnimationMode.TWO_PHASE)
            {
                if (currentPhase == 1 && phase2StartIndex > 0)
                {
                    frameCount = phase2StartIndex;
                }
                else if (currentPhase == 2 && phase2StartIndex < textureArray.Count)
                {
                    frameCount = textureArray.Count - phase2StartIndex;
                }
            }

            if (frameCount > 0)
            {
                // Dynamic loop duration defines total time for 1 full loop (0th to last frame)
                delayBetweenAnimation = dynamicLoopDuration / frameCount;
            }
            else
            {
                delayBetweenAnimation = 0.0416666679f;
            }
        }
        else
        {
            float baseFPS = targetFPS > 0f ? targetFPS : (idealFrameRate > 0f ? 1f / idealFrameRate : 24f);
            float speed = AnimationSpeed > 0 ? AnimationSpeed : 5f;
            float speedFactor = speed / 5f;
            delayBetweenAnimation = 1f / (baseFPS * speedFactor);
            if (delayBetweenAnimation <= 0f) delayBetweenAnimation = 0.0416666679f;
        }
    }

    private void ScheduleNextFrame()
    {
        if (currentAnimationState != ImageState.PLAYING) return;
        if (textureArray == null || textureArray.Count == 0) return;

        float nextDelay = delayBetweenAnimation;

        if (useDynamicFramerate && dynamicLoopDuration > 0f)
        {
            float elapsedTime = Time.time - animStartTime;
            int totalFrames = textureArray.Count;
            if (animationMode == AnimationMode.TWO_PHASE)
            {
                totalFrames = (currentPhase == 1) ? phase2StartIndex : (textureArray.Count - phase2StartIndex);
            }

            if (totalFrames > 0)
            {
                float frameDuration = dynamicLoopDuration / totalFrames;
                float currentFrameProgress = (elapsedTime % dynamicLoopDuration) / dynamicLoopDuration;
                int currentExpectedFrame = Mathf.Clamp(Mathf.FloorToInt(currentFrameProgress * totalFrames), 0, totalFrames - 1);
                
                // Target time for the NEXT frame relative to animStartTime
                float currentLoopIndex = Mathf.Floor(elapsedTime / dynamicLoopDuration);
                float nextFrameTargetTime = (currentLoopIndex * dynamicLoopDuration) + ((currentExpectedFrame + 1) * frameDuration);
                
                nextDelay = nextFrameTargetTime - elapsedTime;
                if (nextDelay < 0.001f) nextDelay = 0.001f;
            }
        }

        Invoke(nameof(AnimationProcess), nextDelay);
    }

    private void AnimationProcess()
    {
        if (textureArray == null || textureArray.Count == 0) return;

        if (useDynamicFramerate && dynamicLoopDuration > 0f)
        {
            float elapsedTime = Time.time - animStartTime;
            int totalFrames = textureArray.Count;
            if (animationMode == AnimationMode.TWO_PHASE)
            {
                totalFrames = (currentPhase == 1) ? phase2StartIndex : (textureArray.Count - phase2StartIndex);
            }

            if (totalFrames > 0)
            {
                int completedLoops = Mathf.FloorToInt(elapsedTime / dynamicLoopDuration);
                if (completedLoops > currentLoopCount)
                {
                    currentLoopCount = completedLoops;
                    onLoopComplete?.Invoke(currentLoopCount);
                    if (!doLoopAnimation)
                    {
                        indexOfTexture = totalFrames - 1;
                        SetTextureOfIndex();
                        currentAnimationState = ImageState.NONE;
                        return;
                    }
                }

                float loopProgress = (elapsedTime % dynamicLoopDuration) / dynamicLoopDuration;
                int frameOffset = (animationMode == AnimationMode.TWO_PHASE && currentPhase == 2) ? phase2StartIndex : 0;
                indexOfTexture = frameOffset + Mathf.Clamp(Mathf.FloorToInt(loopProgress * totalFrames), 0, totalFrames - 1);
            }
            else
            {
                indexOfTexture++;
            }
        }
        else
        {
            indexOfTexture++;
        }

        if (animationMode == AnimationMode.SINGLE_PHASE)
        {
            if (!useDynamicFramerate)
            {
                if (indexOfTexture >= textureArray.Count)
                {
                    indexOfTexture = 0;
                    currentLoopCount++;
                    onLoopComplete?.Invoke(currentLoopCount);

                    if (!doLoopAnimation)
                    {
                        currentAnimationState = ImageState.NONE;
                        return;
                    }
                }
            }
        }
        else // TWO_PHASE mode
        {
            HandleTwoPhaseAnimation();
            if (currentAnimationState == ImageState.NONE) return;
        }

        SetTextureOfIndex();
        ScheduleNextFrame();
    }

    private void HandleTwoPhaseAnimation()
    {
        // Phase 1 logic
        if (currentPhase == 1)
        {
            if (indexOfTexture >= phase2StartIndex)
            {
                // Phase 1 completed one loop
                phase1CurrentLoop++;
                
                // Check if we should continue Phase 1 or move to Phase 2
                if (phase1LoopCount == -1 || phase1CurrentLoop < phase1LoopCount)
                {
                    // Continue looping Phase 1
                    indexOfTexture = 0;
                }
                else
                {
                    // Move to Phase 2
                    currentPhase = 2;
                    indexOfTexture = phase2StartIndex;
                    CalculateFrameDelay();
                    
                    // Skip Phase 2 if loop count is 0
                    if (phase2LoopCount == 0)
                    {
                        currentAnimationState = ImageState.NONE;
                        return;
                    }
                }
            }
        }
        // Phase 2 logic
        else if (currentPhase == 2)
        {
            if (indexOfTexture >= textureArray.Count)
            {
                // Phase 2 completed one loop
                phase2CurrentLoop++;
                
                // Check if we should continue Phase 2 or stop
                if (phase2LoopCount == -1 || phase2CurrentLoop < phase2LoopCount)
                {
                    // Continue looping Phase 2
                    indexOfTexture = phase2StartIndex;
                }
                else
                {
                    // Animation complete
                    currentAnimationState = ImageState.NONE;
                }
            }
        }
    }

    public void StartAnimation()
    {
        if (textureArray == null || textureArray.Count == 0) return;

        EnsureRenderer();
        if (rendererDelegate == null) return;

        CancelInvoke(nameof(AnimationProcess));
        indexOfTexture = 0;
        currentLoopCount = 0;
        animStartTime = Time.time;

        // Reset two-phase tracking
        currentPhase = 1;
        phase1CurrentLoop = 0;
        phase2CurrentLoop = 0;

        currentAnimationState = ImageState.PLAYING;

        RevertToInitialState();

        // Skip Phase 1 if in TWO_PHASE mode and loop count is 0
        if (animationMode == AnimationMode.TWO_PHASE && phase1LoopCount == 0)
        {
            currentPhase = 2;
            indexOfTexture = phase2StartIndex;
        }

        CalculateFrameDelay();
        ScheduleNextFrame();
    }

    public void PlayAnimation()
    {
        StartAnimation();
    }

    public void Play()
    {
        StartAnimation();
    }

    public void PauseAnimation()
    {
        if (currentAnimationState == ImageState.PLAYING)
        {
            CancelInvoke(nameof(AnimationProcess));
            pauseStartTime = Time.time;
            currentAnimationState = ImageState.PAUSED;
        }
    }

    public void ResumeAnimation()
    {
        if (currentAnimationState == ImageState.PAUSED)
        {
            animStartTime += (Time.time - pauseStartTime);
            currentAnimationState = ImageState.PLAYING;
            ScheduleNextFrame();
        }
    }

    public void StopAnimation()
    {
        if (currentAnimationState != ImageState.NONE)
        {
            EnsureRenderer();
            if (rendererDelegate != null && textureArray != null && textureArray.Count > 0)
            {
                rendererDelegate.sprite = textureArray[0];
            }
            CancelInvoke(nameof(AnimationProcess));
            currentAnimationState = ImageState.NONE;
            
            // Reset two-phase tracking
            currentPhase = 1;
            phase1CurrentLoop = 0;
            phase2CurrentLoop = 0;
            currentLoopCount = 0;
        }
    }

    public void RevertToInitialState()
    {
        indexOfTexture = 0;
        currentPhase = 1;
        phase1CurrentLoop = 0;
        phase2CurrentLoop = 0;
        SetTextureOfIndex();
    }

    private void SetTextureOfIndex()
    {
        if (textureArray == null || textureArray.Count == 0 || indexOfTexture < 0 || indexOfTexture >= textureArray.Count) return;

        EnsureRenderer();
        if (rendererDelegate != null)
        {
            rendererDelegate.sprite = textureArray[indexOfTexture];
        }
    }
}