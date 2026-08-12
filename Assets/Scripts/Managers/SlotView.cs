using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class SlotView : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameManager gameManager;

    [Header("Symbol Sprites - Assign by Name")]
    [SerializeField] private Sprite spriteRed3X;              // ID: 1
    [SerializeField] private Sprite spriteBlue2X;             // ID: 2
    [SerializeField] private Sprite spriteBlue7;              // ID: 3
    [SerializeField] private Sprite spriteWhite7;             // ID: 4
    [SerializeField] private Sprite spriteWhite7Bar;          // ID: 5
    [SerializeField] private Sprite spriteRed7;               // ID: 6
    [SerializeField] private Sprite spriteTripleBar;          // ID: 7
    [SerializeField] private Sprite spriteDoubleBar;          // ID: 8
    [SerializeField] private Sprite spriteSingleBar;          // ID: 9
    [SerializeField] private Sprite spriteSpin;               // ID: 11
    [SerializeField] private Sprite spriteGreenWheel;         // ID: 12
    [SerializeField] private Sprite spriteDoubleWheel;        // ID: 13
    [SerializeField] private Sprite spriteRedWheel;           // ID: 14

    // Internal array built from named sprites
    private Sprite[] symbolSprites;

    [Header("Win Animation Sprite Arrays for All Icons")]
    [SerializeField] private List<Sprite> animSpritesRed3X;           // ID: 1
    [SerializeField] private List<Sprite> animSpritesBlue2X;          // ID: 2
    [SerializeField] private List<Sprite> animSpritesBlue7;           // ID: 3
    [SerializeField] private List<Sprite> animSpritesWhite7;          // ID: 4
    [SerializeField] private List<Sprite> animSpritesWhite7Bar;       // ID: 5
    [SerializeField] private List<Sprite> animSpritesRed7;            // ID: 6
    [SerializeField] private List<Sprite> animSpritesTripleBar;       // ID: 7
    [SerializeField] private List<Sprite> animSpritesDoubleBar;       // ID: 8
    [SerializeField] private List<Sprite> animSpritesSingleBar;       // ID: 9
    [SerializeField] private List<Sprite> animSpritesSpin;            // ID: 11
    [SerializeField] private List<Sprite> animSpritesGreenWheel;      // ID: 12
    [SerializeField] private List<Sprite> animSpritesDoubleWheel;     // ID: 13
    [SerializeField] private List<Sprite> animSpritesRedWheel;        // ID: 14

    // Internal array of animation sprite lists
    private List<Sprite>[] animationSpriteArrays;

    [Header("Reel Containers")]
    [SerializeField] private Transform[] reelTransforms;

    [Header("Reel Images - 14 images per reel")]
    [SerializeField] private List<ReelImages> reelImagesList;

    [Header("Reel Stop Y Positions")]
    [SerializeField] private float case1StopY = 160f; // Case 1: blank, icon, blank
    [SerializeField] private float case2StopY = 0f;    // Case 2: icon, blank, icon

    [Header("Spin Settings")]
    [SerializeField] private float symbolHeight = 100f;
    [SerializeField] private float spinSpeed = 2000f;
    [SerializeField] private float reelStartStagger = 0.08f;
    [SerializeField] private float reelStopStagger = 0.12f;

    [Header("Animation Settings - Casino Style")]
    [SerializeField] private float anticipationUpDistance = 20f;
    [SerializeField] private float anticipationUpDuration = 0.12f;

    [Header("Win Animation Settings")]
    [SerializeField] private float winPopDuration = 0.4f;
    [SerializeField] private int winPopRepeat = 3;


    [Header("Stop Animation Settings")]
    [SerializeField] private float stopOvershootDistance = 50f;
    [SerializeField] private float stopOvershootDuration = 0.20f;
    [SerializeField] private float stopSettleDuration = 0.30f;

    [Header("Quick Spin Settings")]
    [SerializeField] private float quickStopStagger = 0.06f;
    [SerializeField] private float quickStopOvershoot = 20f;
    [SerializeField] private float quickStopDuration = 0.2f;
    [SerializeField] private int minSpinCyclesBeforeStop = 3;


    [Header("Win Animation Settings")]
    [SerializeField] private float winAnimationDuration = 3.0f; // Total duration each win symbol animation plays
    [SerializeField] private float winSymbolLoopDuration = 1.5f;
    [SerializeField] private int winSymbolLoopCount = 3;
    [Tooltip("Delay between enabling winBox overlay and starting the ImageAnimation - for sync timing")]
    [SerializeField] private float winLineBoxToAnimationDelay = 0.05f;

    [Header("Phase 1 Total Win Presentation")]
    [SerializeField] private TMPro.TMP_Text phase1TotalWinText;

    [Header("Win Animation Objects — Col 0..4  (each has 3 rows, contains ImageAnimation component)")]
    [Tooltip("GameObject references for win animations. Each should have an ImageAnimation component attached.")]
    [SerializeField] private ColumnOverlays[] winAnimationColumns = new ColumnOverlays[5];


    [Header("Symbol Info Card")]
    [SerializeField] private SymbolInfoCard symbolInfoCard;


    private float middlePosition = 0f;
    private float cycleDistance;


    private List<Tween> spinTweens = new List<Tween>();
    private List<Tween> winTweens = new List<Tween>();
    private List<int> reelCycleCount = new List<int>();
    private Coroutine winAnimationCoroutine;


    internal List<List<int>> currentDisplayMatrix;

    private bool isSpinning;

    #region Initialization

    private void Start()
    {
        BuildSymbolSpriteArray();
        InitializeReels();
        DisableAllOverlays();
        SetupSymbolButtons();
    }

    private void DisableAllOverlays()
    {
        DisableColumns(winAnimationColumns);
        HidePhase1TotalWinText();
        if (symbolInfoCard) symbolInfoCard.HideCard();
    }

    private void SetupSymbolButtons()
    {
        if (reelImagesList == null) return;
        for (int col = 0; col < reelImagesList.Count; col++)
        {
            var reel = reelImagesList[col];
            if (reel == null || reel.images == null) continue;
            int visibleStartIndex = 6;
            int rowCount = 3;
            for (int row = 0; row < rowCount; row++)
            {
                int imageIndex = visibleStartIndex + row;
                if (imageIndex < reel.images.Count && reel.images[imageIndex] != null)
                {
                    Image img = reel.images[imageIndex];
                    SymbolButtonHandler btnHandler = img.GetComponent<SymbolButtonHandler>();
                    if (btnHandler == null)
                    {
                        btnHandler = img.gameObject.AddComponent<SymbolButtonHandler>();
                    }
                    btnHandler.Init(col, row, this);
                }
            }
        }
    }

    internal void HideSymbolInfoCard()
    {
        if (symbolInfoCard != null) symbolInfoCard.HideCard();
    }

    internal void OnBetChanged()
    {
        if (symbolInfoCard != null && symbolInfoCard.gameObject.activeSelf)
        {
            symbolInfoCard.RefreshCard(gameManager);
        }
    }

    internal void OnSymbolClicked(int col, int row, RectTransform symbolRect)
    {
        if (isSpinning)
        {
            if (symbolInfoCard != null) symbolInfoCard.HideCard();
            return;
        }

        if (currentDisplayMatrix == null || col >= currentDisplayMatrix.Count)
        {
            return;
        }

        var colMatrix = currentDisplayMatrix[col];
        if (colMatrix == null || colMatrix.Count < 3) return;

        bool isCase1 = colMatrix[1] != 0;

        int symbolId = 0;
        if (isCase1)
        {
            if (row == 1) symbolId = colMatrix[1];
        }
        else
        {
            if (row == 0) symbolId = colMatrix[0];
            else if (row == 2 || row == 1) symbolId = colMatrix[2];
        }

        if (symbolId != 0 && symbolInfoCard != null)
        {
            symbolInfoCard.ShowCard(symbolId, col, row, symbolRect, gameManager);
        }
    }

    private static void DisableColumns(ColumnOverlays[] cols)
    {
        if (cols == null) return;
        foreach (var col in cols)
            if (col?.rows != null)
                foreach (var go in col.rows)
                    if (go) go.SetActive(false);
    }

    private static GameObject WinBox(ColumnOverlays[] cols, int col, int row)
        => (col >= 0 && col < cols?.Length && cols[col]?.rows != null && row >= 0 && row < cols[col].rows.Length)
            ? cols[col].rows[row] : null;

    private void BuildSymbolSpriteArray()
    {
        // Build the symbol sprite array from named sprite fields (size 15 for IDs 0..14)
        symbolSprites = new Sprite[15];
        symbolSprites[1] = spriteRed3X;
        symbolSprites[2] = spriteBlue2X;
        symbolSprites[3] = spriteBlue7;
        symbolSprites[4] = spriteWhite7;
        symbolSprites[5] = spriteWhite7Bar;
        symbolSprites[6] = spriteRed7;
        symbolSprites[7] = spriteTripleBar;
        symbolSprites[8] = spriteDoubleBar;
        symbolSprites[9] = spriteSingleBar;
        symbolSprites[11] = spriteSpin;
        symbolSprites[12] = spriteGreenWheel;
        symbolSprites[13] = spriteDoubleWheel;
        symbolSprites[14] = spriteRedWheel;

        // Fallback for unassigned sprites
        Sprite defaultSprite = null;
        for (int i = 0; i < symbolSprites.Length; i++)
        {
            if (symbolSprites[i] != null)
            {
                defaultSprite = symbolSprites[i];
                break;
            }
        }

        for (int i = 0; i < symbolSprites.Length; i++)
        {
            if (symbolSprites[i] == null)
            {
                symbolSprites[i] = defaultSprite;
            }
        }

        // Build the animation sprite arrays for all symbol IDs
        animationSpriteArrays = new List<Sprite>[15];
        animationSpriteArrays[1] = animSpritesRed3X;
        animationSpriteArrays[2] = animSpritesBlue2X;
        animationSpriteArrays[3] = animSpritesBlue7;
        animationSpriteArrays[4] = animSpritesWhite7;
        animationSpriteArrays[5] = animSpritesWhite7Bar;
        animationSpriteArrays[6] = animSpritesRed7;
        animationSpriteArrays[7] = animSpritesTripleBar;
        animationSpriteArrays[8] = animSpritesDoubleBar;
        animationSpriteArrays[9] = animSpritesSingleBar;
        animationSpriteArrays[11] = animSpritesSpin;
        animationSpriteArrays[12] = animSpritesGreenWheel;
        animationSpriteArrays[13] = animSpritesDoubleWheel;
        animationSpriteArrays[14] = animSpritesRedWheel;
    }

    private void InitializeReels()
    {
        cycleDistance = symbolHeight;
        middlePosition = 0f;



        int reelCount = (gameManager != null && gameManager.gameConfig != null) ? gameManager.gameConfig.reelCount : 3;
        int rowCount = (gameManager != null && gameManager.gameConfig != null) ? gameManager.gameConfig.rowCount : 3;

        currentDisplayMatrix = new List<List<int>>();
        reelCycleCount = new List<int>();
        for (int col = 0; col < reelCount; col++)
        {
            var defaultCol = new List<int>();
            for (int r = 0; r < rowCount; r++)
            {
                defaultCol.Add(0);
            }
            currentDisplayMatrix.Add(defaultCol);
            reelCycleCount.Add(0);
        }
    }

    internal void SetInitialMatrix(List<List<int>> matrix)
    {
        if (matrix == null || matrix.Count == 0) return;

        int reelCount = matrix.Count;
        int rowCount = (gameManager != null && gameManager.gameConfig != null) ? gameManager.gameConfig.rowCount : 3;

        for (int col = 0; col < reelCount; col++)
        {
            if (matrix[col] != null && matrix[col].Count != rowCount) return;
        }

        currentDisplayMatrix = matrix;

        for (int col = 0; col < reelCount; col++)
        {
            if (col < reelImagesList.Count)
            {
                SetReelSymbols(col, matrix[col], true);
            }
        }
    }

    #endregion

    #region Symbol Display

    private float GetTargetYForResult(List<int> columnSymbols)
    {
        if (columnSymbols == null || columnSymbols.Count < 3)
            return middlePosition + case2StopY;

        bool isMiddleIcon = columnSymbols[1] != 0;
        if (isMiddleIcon)
        {
            // Case 1: blank, icon, blank -> Stop at case1StopY (-160f default)
            return middlePosition + case1StopY;
        }
        else
        {
            // Case 2: icon, blank, icon -> Stop at case2StopY (0f default)
            return middlePosition + case2StopY;
        }
    }

    private void SetReelSymbols(int columnIndex, List<int> visibleSymbolIds, bool isInitial = false)
    {
        if (columnIndex >= reelImagesList.Count) return;

        var reel = reelImagesList[columnIndex];
        if (reel.images == null || reel.images.Count < 14) return;

        bool isCase1 = visibleSymbolIds != null && visibleSymbolIds.Count >= 3 && visibleSymbolIds[1] != 0;

        // Build list of non-blank symbol IDs for random buffer images
        List<int> nonBlankIds = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 11, 12, 13, 14 };

        // Exclude current visible non-blank result symbols from buffer pool to prevent nearby duplicates
        if (visibleSymbolIds != null)
        {
            foreach (int sId in visibleSymbolIds)
            {
                if (sId != 0) nonBlankIds.Remove(sId);
            }
        }

        // Shuffle nonBlankIds for this reel
        for (int i = nonBlankIds.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            int temp = nonBlankIds[i];
            nonBlankIds[i] = nonBlankIds[randomIndex];
            nonBlankIds[randomIndex] = temp;
        }

        int bufferIndex = 0;
        HashSet<int> reservedIndices = new HashSet<int>();

        if (isCase1)
        {
            // Case 1: blank, icon, blank -> Stop at -160f
            // Visible elements are 7th, 8th, 9th (0-based array indices 6, 7, 8)
            reservedIndices.Add(6);
            reservedIndices.Add(7);
            reservedIndices.Add(8);

            reel.images[7].sprite = GetSymbolSprite(visibleSymbolIds[1]); // 8th element (mid icon)
            reel.images[6].sprite = GetRandomNonBlankSprite();            // 7th element (random top)
            reel.images[8].sprite = GetRandomNonBlankSprite();            // 9th element (random bottom)
        }
        else
        {
            // Case 2: icon, blank, icon -> Stop at 0f
            // Visible elements are 7th, 8th (0-based array indices 6, 7)
            reservedIndices.Add(6);
            reservedIndices.Add(7);

            int topSymbolId = (visibleSymbolIds != null && visibleSymbolIds.Count > 0) ? visibleSymbolIds[0] : 1;
            int botSymbolId = (visibleSymbolIds != null && visibleSymbolIds.Count > 2) ? visibleSymbolIds[2] : 1;

            reel.images[6].sprite = GetSymbolSprite(topSymbolId); // 7th element (1st row result)
            reel.images[7].sprite = GetSymbolSprite(botSymbolId); // 8th element (3rd row result)
        }

        // Populate all other buffer images (outside reserved indices) with non-blank symbols
        for (int i = 0; i < reel.images.Count; i++)
        {
            if (reservedIndices.Contains(i)) continue;

            int symId = nonBlankIds[bufferIndex % nonBlankIds.Count];
            bufferIndex++;
            reel.images[i].sprite = GetSymbolSprite(symId);
        }

        if (isInitial && reelTransforms[columnIndex] != null)
        {
            reelTransforms[columnIndex].localPosition = new Vector3(
                reelTransforms[columnIndex].localPosition.x,
                0f,
                0f
            );
        }
    }

    private Sprite GetSymbolSprite(int symbolId)
    {
        // Validate symbolId range (0-12)
        if (symbolId < 0 || symbolId >= symbolSprites.Length)
        {
            Debug.LogWarning($"[SlotView] Invalid symbolId {symbolId}, using default sprite 0. Total sprites: {symbolSprites.Length}");
            return symbolSprites[0];
        }

        if (symbolSprites[symbolId] == null)
        {
            Debug.LogError($"[SlotView] Symbol sprite for ID {symbolId} is null!");
            return symbolSprites[0];
        }

        return symbolSprites[symbolId];
    }

    #endregion

    #region Spin Animation

    internal void StartSpin()
    {
        if (isSpinning) return;

        if (symbolInfoCard != null) symbolInfoCard.HideCard();

        isSpinning = true;
        KillAllTweens();

        DisableAllOverlays();

        for (int i = 0; i < reelCycleCount.Count; i++)
        {
            reelCycleCount[i] = 0;
        }

        int reelCount = currentDisplayMatrix != null ? currentDisplayMatrix.Count : (gameManager?.gameConfig != null ? gameManager.gameConfig.reelCount : 3);
        int maxCols = Mathf.Min(reelCount, reelTransforms != null ? reelTransforms.Length : 3);

        for (int col = 0; col < maxCols; col++)
        {
            StartReelCycleWithDelay(col, col * reelStartStagger);
        }
    }

    private void StartReelCycleWithDelay(int columnIndex, float delay)
    {
        if (columnIndex >= reelTransforms.Length) return;

        if (delay > 0)
        {
            Sequence startSequence = DOTween.Sequence();
            startSequence.AppendInterval(delay);
            startSequence.OnComplete(() => {
                if (isSpinning)
                {
                    StartReelCycle(columnIndex);
                }
            });
            startSequence.Play();

            if (spinTweens.Count <= columnIndex)
                spinTweens.Add(startSequence);
            else
                spinTweens[columnIndex] = startSequence;
        }
        else
        {
            StartReelCycle(columnIndex);
        }
    }

    private void StartReelCycle(int columnIndex)
    {
        if (columnIndex >= reelTransforms.Length) return;
        if (!isSpinning) return;

        Transform slotTransform = reelTransforms[columnIndex];
        var reel = (columnIndex < reelImagesList.Count) ? reelImagesList[columnIndex] : null;
        int totalImages = (reel != null && reel.images != null && reel.images.Count > 0) ? reel.images.Count : 14;

        // Number of buffer images outside the visible 3-row area
        int bufferCount = totalImages - 3;
        float fullDistance = bufferCount * symbolHeight;
        float halfDistance = fullDistance / 2f;

        // Top start position and bottom loop exit position for spinning the full 14-symbol strip (top to bottom)
        float spinTopY = middlePosition + halfDistance;
        float spinBottomY = middlePosition - halfDistance;

        slotTransform.localPosition = new Vector3(slotTransform.localPosition.x, spinTopY, 0);

        float currentSpeed = spinSpeed;
        float loopDuration = fullDistance / currentSpeed;

        // Continuous linear translation over full strip distance, looping seamlessly
        Tweener loopTweener = slotTransform.DOLocalMoveY(spinBottomY, loopDuration)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Restart)
            .OnStepComplete(() => {
                if (columnIndex < reelCycleCount.Count)
                {
                    reelCycleCount[columnIndex]++;
                }
            });

        if (spinTweens.Count <= columnIndex)
            spinTweens.Add(loopTweener);
        else
            spinTweens[columnIndex] = loopTweener;
    }

    #endregion

    #region Stop Spin

    internal void StopSpin(List<List<int>> resultMatrix, System.Action onComplete, bool isTurbo = false)
    {
        int reelCount = resultMatrix != null ? resultMatrix.Count : (gameManager?.gameConfig != null ? gameManager.gameConfig.reelCount : 3);
        int maxCols = Mathf.Min(reelCount, reelTransforms != null ? reelTransforms.Length : 3);

        if (!isSpinning)
        {
            currentDisplayMatrix = resultMatrix;
            for (int col = 0; col < maxCols; col++)
            {
                SetReelSymbols(col, resultMatrix[col], false);
            }
            onComplete?.Invoke();
            return;
        }

        StartCoroutine(StopSpinSequence(resultMatrix, onComplete, false, isTurbo));
    }

    private IEnumerator StopSpinSequence(List<List<int>> resultMatrix, System.Action onComplete, bool isQuickStop, bool isTurbo = false)
    {
        currentDisplayMatrix = resultMatrix;
        int reelCount = resultMatrix != null ? resultMatrix.Count : 3;
        int maxCols = Mathf.Min(reelCount, reelTransforms != null ? reelTransforms.Length : 3);

        if (!isQuickStop && !isTurbo)
        {
            while (true)
            {
                bool allReelsReady = true;
                for (int col = 0; col < maxCols; col++)
                {
                    if (col < reelCycleCount.Count && reelCycleCount[col] < minSpinCyclesBeforeStop)
                    {
                        allReelsReady = false;
                        break;
                    }
                }

                if (allReelsReady) break;
                yield return null;
            }
        }

        float stagger = isQuickStop ? quickStopStagger : (isTurbo ? (reelStopStagger * 0.5f) : reelStopStagger);

        for (int col = 0; col < maxCols; col++)
        {
            float delay = col * stagger;
            StartCoroutine(StopSingleReel(col, resultMatrix[col], delay, isQuickStop || isTurbo));
        }

        float longestStopTime;
        if (isQuickStop)
        {
            longestStopTime = ((maxCols - 1) * stagger) + quickStopDuration;
        }
        else if (isTurbo)
        {
            longestStopTime = ((maxCols - 1) * stagger) + (stopOvershootDuration * 0.5f) + (stopSettleDuration * 0.5f);
        }
        else
        {
            longestStopTime = ((maxCols - 1) * stagger) + stopOvershootDuration + stopSettleDuration;
        }

        yield return new WaitForSeconds(longestStopTime);

        isSpinning = false;

        onComplete?.Invoke();
    }

    private IEnumerator StopSingleReel(int columnIndex, List<int> targetSymbols, float delay, bool isQuickStop)
    {
        if (delay > 0)
        {
            yield return new WaitForSeconds(delay);
        }

        if (columnIndex < spinTweens.Count && spinTweens[columnIndex] != null)
        {
            spinTweens[columnIndex].Kill();
        }

        Transform slotTransform = reelTransforms[columnIndex];

        float targetY = GetTargetYForResult(targetSymbols);

        // Set target symbols & non-blank buffer images for active case
        SetReelSymbols(columnIndex, targetSymbols, false);

        // Snap transform to landing start point above targetY for smooth deceleration ease down to targetY
        float landingStartTopY = targetY + (2f * symbolHeight);
        slotTransform.localPosition = new Vector3(
            slotTransform.localPosition.x,
            landingStartTopY,
            0
        );

        // ── Play reel-stop sound ──────────
        AudioManager.Instance?.PlayReelStop();

        // Detect wild symbols in this column for hit sounds
        if (currentDisplayMatrix != null && columnIndex < currentDisplayMatrix.Count)
        {
            bool hasWild = false;
            int wildId = gameManager?.gameConfig != null ? gameManager.gameConfig.wildSymbolId : 10;
            foreach (int sym in currentDisplayMatrix[columnIndex])
            {
                if (sym == wildId) hasWild = true;
            }
            if (hasWild) AudioManager.Instance?.PlayReelStop();
        }
        // ──────────────────────────────────────────────────────────────────

        if (isQuickStop)
        {
            Sequence quickStopSequence = DOTween.Sequence();

            quickStopSequence.Append(
                slotTransform.DOLocalMoveY(targetY - quickStopOvershoot, quickStopDuration * 0.3f)
                    .SetEase(Ease.OutQuad)
            );

            quickStopSequence.Append(
                slotTransform.DOLocalMoveY(targetY, quickStopDuration * 0.7f)
                    .SetEase(Ease.InOutQuad)
            );

            quickStopSequence.OnComplete(() => PlayStopAnimationsForColumn(columnIndex));

            if (spinTweens.Count <= columnIndex)
                spinTweens.Add(quickStopSequence);
            else
                spinTweens[columnIndex] = quickStopSequence;
        }
        else
        {
            Sequence stopSequence = DOTween.Sequence();

            stopSequence.Append(
                slotTransform.DOLocalMoveY(targetY - stopOvershootDistance, stopOvershootDuration)
                    .SetEase(Ease.OutQuad)
            );

            stopSequence.Append(
                slotTransform.DOLocalMoveY(targetY, stopSettleDuration)
                    .SetEase(Ease.InOutQuad)
            );

            stopSequence.OnComplete(() => PlayStopAnimationsForColumn(columnIndex));

            if (spinTweens.Count <= columnIndex)
                spinTweens.Add(stopSequence);
            else
                spinTweens[columnIndex] = stopSequence;
        }
    }

    #endregion

    #region Quick Spin

    internal void QuickStop(List<List<int>> resultMatrix, System.Action onComplete = null)
    {
        if (!isSpinning)
        {
            currentDisplayMatrix = resultMatrix;
            int reelCount = resultMatrix != null ? resultMatrix.Count : 3;
            int maxCols = Mathf.Min(reelCount, reelTransforms != null ? reelTransforms.Length : 3);

            for (int col = 0; col < maxCols; col++)
            {
                if (col < reelTransforms.Length)
                {
                    SetReelSymbols(col, resultMatrix[col], false);
                    reelTransforms[col].localPosition = new Vector3(
                        reelTransforms[col].localPosition.x,
                        middlePosition,
                        0
                    );
                    PlayStopAnimationsForColumn(col);
                }
            }
            
            onComplete?.Invoke();
            return;
        }

        StartCoroutine(StopSpinSequence(resultMatrix, onComplete, true));
    }

    #endregion

    #region Stop Symbol Animations

    private void PlayStopAnimationsForColumn(int col)
    {
        if (currentDisplayMatrix == null || col >= currentDisplayMatrix.Count) return;
        
        for (int row = 0; row < currentDisplayMatrix[col].Count; row++)
        {
            int symId = currentDisplayMatrix[col][row];
            int wildId = gameManager?.gameConfig != null ? gameManager.gameConfig.wildSymbolId : 10;
            bool isWild = (symId == wildId);
            
            if (isWild)
            {
                AnimateSymbolSingleLoop(col, row, 1);
            }
        }
    }

    internal void AnimateAllScatters(int loopCount)
    {
        if (currentDisplayMatrix == null) return;

        // Clear any individual hit animations before starting the collective one
        KillWinTweens();

        int actualScatterId = gameManager?.gameConfig != null ? gameManager.gameConfig.scatterSymbolId : -1;
        if (actualScatterId < 0) return;
        
        for (int col = 0; col < 5; col++)
        {
            for (int row = 0; row < currentDisplayMatrix[col].Count; row++)
            {
                if (currentDisplayMatrix[col][row] == actualScatterId)
                {
                    AnimateSymbolSingleLoop(col, row, loopCount);
                }
            }
        }
    }

    internal void AnimateUSpinWin(System.Action onComplete = null)
    {
        if (currentDisplayMatrix == null)
        {
            onComplete?.Invoke();
            return;
        }

        KillWinTweens();
        AudioManager.Instance?.PlayWinLinePhase1Start();

        List<ImageAnimation> activeUSpinAnims = new List<ImageAnimation>();
        int completedCount = 0;
        int targetLoops = 2; // Exactly 2 loops of full animation

        for (int col = 0; col < 5; col++)
        {
            if (col >= currentDisplayMatrix.Count) continue;
            for (int row = 0; row < currentDisplayMatrix[col].Count; row++)
            {
                if (currentDisplayMatrix[col][row] == 11) // Spin/USpin Symbol ID
                {
                    var animGO = WinBox(winAnimationColumns, col, row);
                    if (animGO != null)
                    {
                        ImageAnimation imageAnim = animGO.GetComponent<ImageAnimation>();
                        int imageIndex = 2 + row;
                        Image symbolImage = (col < reelImagesList.Count && reelImagesList[col].images != null && imageIndex < reelImagesList[col].images.Count)
                            ? reelImagesList[col].images[imageIndex]
                            : null;

                        if (imageAnim != null)
                        {
                            activeUSpinAnims.Add(imageAnim);

                            List<Sprite> animSprites = animationSpriteArrays[11];
                            if (animSprites != null && animSprites.Count > 0)
                            {
                                imageAnim.textureArray = animSprites;
                            }
                            imageAnim.doLoopAnimation = true;

                            animGO.SetActive(true);
                            Image animRenderer = imageAnim.rendererDelegate != null ? imageAnim.rendererDelegate : animGO.GetComponent<Image>();
                            if (animRenderer != null)
                            {
                                animRenderer.DOKill();
                                Color c = animRenderer.color;
                                animRenderer.color = new Color(c.r, c.g, c.b, 1f);
                            }
                            if (symbolImage != null)
                            {
                                symbolImage.DOKill();
                                symbolImage.DOFade(0f, 0.2f);
                            }

                            imageAnim.onLoopComplete = (loopCount) =>
                            {
                                if (loopCount >= targetLoops)
                                {
                                    imageAnim.onLoopComplete = null;
                                    imageAnim.StopAnimation();
                                    animGO.SetActive(false);

                                    if (symbolImage != null)
                                    {
                                        symbolImage.DOKill();
                                        symbolImage.DOFade(1f, 0.2f);
                                    }

                                    completedCount++;
                                    if (completedCount >= activeUSpinAnims.Count)
                                    {
                                        onComplete?.Invoke();
                                    }
                                }
                            };

                            imageAnim.StartAnimation();
                        }
                    }
                }
            }
        }

        if (activeUSpinAnims.Count == 0)
        {
            onComplete?.Invoke();
        }
    }

    internal void AnimateMoneyBagWin()
    {
        if (currentDisplayMatrix == null) return;

        KillWinTweens();
        AudioManager.Instance?.PlayWinLinePhase1Start();
    }

    private void AnimateSymbolSingleLoop(int column, int row, int loopCount = 1)
    {
        if (column >= reelImagesList.Count) return;

        var reel = reelImagesList[column];
        if (reel.images == null || reel.images.Count < 3) return;

        int imageIndex = 2 + row;
        if (imageIndex >= reel.images.Count) return;

        Image symbolImage = reel.images[imageIndex];
        if (symbolImage == null) return;

        var animGO = WinBox(winAnimationColumns, column, row);
        if (animGO == null) return;

        ImageAnimation imageAnim = animGO.GetComponent<ImageAnimation>();
        if (imageAnim == null) return;

        int symbolId = currentDisplayMatrix[column][row];
        if (symbolId < 0 || symbolId >= animationSpriteArrays.Length) return;

        List<Sprite> animSprites = animationSpriteArrays[symbolId];
        if (animSprites == null || animSprites.Count == 0) return;

        imageAnim.textureArray = animSprites;

        Color originalColor = new Color(symbolImage.color.r, symbolImage.color.g, symbolImage.color.b, 1f);

        Sequence seq = DOTween.Sequence();
        
        seq.AppendCallback(() => {
            animGO.SetActive(true);
            Image animRenderer = imageAnim.rendererDelegate;
            if (animRenderer != null)
            {
                animRenderer.DOKill();
                Color c = animRenderer.color;
                animRenderer.color = new Color(c.r, c.g, c.b, 1f);
            }
            symbolImage.DOKill();
            symbolImage.DOFade(0f, 0.2f);
            
            imageAnim.StartAnimation();
        });

        seq.AppendInterval(winSymbolLoopDuration * loopCount);

        seq.AppendCallback(() => {
            Image animRenderer = imageAnim != null ? imageAnim.rendererDelegate : null;

            if (animRenderer != null)
            {
                animRenderer.DOKill();
                Color c = animRenderer.color;
                animRenderer.color = new Color(c.r, c.g, c.b, 1f);
            }

            if (imageAnim != null) imageAnim.StopAnimation();
            if (animGO != null) animGO.SetActive(false);

            if (symbolImage != null)
            {
                symbolImage.DOKill();
                symbolImage.DOFade(originalColor.a, 0.2f);
            }
        });

        winTweens.Add(seq);
    }

    #endregion

    #region Win Line Animation

    internal void ShowWinLineAnimation(List<WinLine> winLines, System.Action onComplete)
    {
        if (winLines == null || winLines.Count == 0)
        {
            onComplete?.Invoke();
            return;
        }

        KillWinTweens();
        winAnimationCoroutine = StartCoroutine(PlayTwoPhaseWinLines(winLines, onComplete));
    }

    private IEnumerator PlayTwoPhaseWinLines(List<WinLine> winLines, System.Action onComplete)
    {
        int rowLimit = (gameManager != null && gameManager.gameConfig != null) ? gameManager.gameConfig.rowCount : 3;

        // ==========================================
        // PHASE 1: Show all winning icons at once
        // ==========================================
        HashSet<int> allWinPositions = new HashSet<int>();
        foreach (var winLine in winLines)
        {
            if (winLine.positions != null)
            {
                foreach (int flatIndex in winLine.positions)
                {
                    allWinPositions.Add(flatIndex);
                }
            }
        }

        Debug.Log($"[PlayTwoPhaseWinLines] Phase 1: Showing all {allWinPositions.Count} winning icons at once for {winLines.Count} win lines");

        // Calculate Phase 1 Total Win Amount
        double totalWinAmount = 0;
        foreach (var winLine in winLines)
        {
            totalWinAmount += winLine.winAmount;
        }
        if (totalWinAmount <= 0 && gameManager != null && gameManager.lastResult != null)
        {
            totalWinAmount = gameManager.lastResult.winAmount;
        }

        // Show Phase 1 Total Win Text with final win value
        ShowPhase1TotalWin(totalWinAmount);

        AudioManager.Instance?.PlayWinLinePhase1Start();

        // Animate all winning symbols and wait for their ImageAnimation loops to complete
        yield return StartCoroutine(AnimateWinPositions(allWinPositions));

        KillWinTweens(false);
        HideAllWinLineTexts();
        HidePhase1TotalWinText();

        // Invoke onComplete immediately after Phase 1 so game logic (Free Spins / Autoplay / Win complete) can proceed
        onComplete?.Invoke();

        // Skip Phase 2 if in Free Spins, Autoplay, or if a Special Feature (USpin, MoneyBag, Scatter trigger) was triggered
        bool hasSpecialFeature = (gameManager != null && gameManager.lastResult != null && (
            (gameManager.lastResult.uSpinData != null && gameManager.lastResult.uSpinData.triggered) ||
            (gameManager.lastResult.moneyBagData != null && gameManager.lastResult.moneyBagData.triggered) ||
            (gameManager.lastResult.freeSpinData != null && gameManager.lastResult.freeSpinData.isTriggered)
        ));

        bool skipPhase2 = (gameManager != null && (gameManager.isInFreeSpins || gameManager.isAutoPlaying)) || hasSpecialFeature;
        if (skipPhase2)
        {
            yield break;
        }

        // ==========================================
        // PHASE 2: Individual Win Line presentation loop
        // ==========================================
        while (true)
        {
            foreach (var winLine in winLines)
            {
                if (winLine.positions == null || winLine.positions.Count == 0) continue;

                KillWinTweens(false);
                HideAllWinLineTexts();

                // Show WinLineText on 1st icon of the active win line only if there are multiple win lines
                if (winLines.Count > 1)
                {
                    int firstFlatIndex = winLine.positions[0];
                    int reelCount = (gameManager != null && gameManager.gameConfig != null) ? gameManager.gameConfig.reelCount : 3;
                    int firstRow = firstFlatIndex / reelCount;
                    int firstCol = firstFlatIndex % reelCount;
                    ShowWinLineTextOnIcon(firstCol, firstRow, winLine.winAmount);
                }

                // Animate win line symbols and wait for their ImageAnimation loops to complete
                yield return StartCoroutine(AnimateWinPositions(winLine.positions));
            }
        }
    }

    private IEnumerator AnimateWinPositions(IEnumerable<int> flatPositions)
    {
        if (flatPositions == null) yield break;

        int reelCount = (gameManager != null && gameManager.gameConfig != null) ? gameManager.gameConfig.reelCount : 3;
        int rowLimit = (gameManager != null && gameManager.gameConfig != null) ? gameManager.gameConfig.rowCount : 3;
        int loopCountTarget = (gameManager != null && (gameManager.isInFreeSpins || gameManager.isAutoPlaying)) ? 1 : winSymbolLoopCount;

        List<ImageAnimation> activeAnims = new List<ImageAnimation>();
        int completedCount = 0;
        bool isCompleted = false;

        foreach (int flatIndex in flatPositions)
        {
            int row = flatIndex / reelCount;
            int col = flatIndex % reelCount;

            if (col < 0 || col >= 5 || row < 0 || row >= rowLimit) continue;

            if (col >= reelImagesList.Count) continue;
            var reel = reelImagesList[col];
            if (reel.images == null || reel.images.Count < 3) continue;

            int imageIndex = 2 + row;
            if (imageIndex >= reel.images.Count) continue;

            Image symbolImage = reel.images[imageIndex];
            if (symbolImage == null) continue;

            var animGO = WinBox(winAnimationColumns, col, row);
            if (animGO == null) continue;

            ImageAnimation imageAnim = animGO.GetComponent<ImageAnimation>();
            if (imageAnim == null) continue;

            if (col >= currentDisplayMatrix.Count || row >= currentDisplayMatrix[col].Count) continue;
            int symbolId = currentDisplayMatrix[col][row];
            if (symbolId < 0 || symbolId >= animationSpriteArrays.Length) continue;

            List<Sprite> animSprites = animationSpriteArrays[symbolId];
            if (animSprites == null || animSprites.Count == 0) continue;

            imageAnim.textureArray = animSprites;
            imageAnim.doLoopAnimation = true;

            animGO.SetActive(true);
            Image animRenderer = imageAnim.rendererDelegate != null ? imageAnim.rendererDelegate : animGO.GetComponent<Image>();
            if (animRenderer != null)
            {
                animRenderer.DOKill();
                Color c = animRenderer.color;
                animRenderer.color = new Color(c.r, c.g, c.b, 1f);
            }

            symbolImage.DOKill();
            symbolImage.DOFade(0f, 0.2f);

            activeAnims.Add(imageAnim);

            imageAnim.onLoopComplete = (currentLoop) =>
            {
                if (currentLoop >= loopCountTarget)
                {
                    imageAnim.onLoopComplete = null;
                    imageAnim.StopAnimation();
                    if (animGO != null) animGO.SetActive(false);

                    if (symbolImage != null)
                    {
                        symbolImage.DOKill();
                        symbolImage.DOFade(1f, 0.2f);
                    }

                    completedCount++;
                    if (completedCount >= activeAnims.Count)
                    {
                        isCompleted = true;
                    }
                }
            };
        }

        if (winLineBoxToAnimationDelay > 0)
        {
            yield return new WaitForSeconds(winLineBoxToAnimationDelay);
        }

        foreach (var imageAnim in activeAnims)
        {
            imageAnim.StartAnimation();
        }

        if (activeAnims.Count > 0)
        {
            yield return new WaitUntil(() => isCompleted);
        }
        else
        {
            yield return new WaitForSeconds(winSymbolLoopDuration);
        }
    }

    private void ShowWinLineTextOnIcon(int col, int row, double winAmount)
    {
        if (col < 0 || col >= reelImagesList.Count) return;
        var reel = reelImagesList[col];
        if (reel.images == null) return;
        int imageIndex = 2 + row;
        if (imageIndex >= reel.images.Count) return;

        Image symbolImage = reel.images[imageIndex];
        if (symbolImage == null) return;

        Transform textTransform = symbolImage.transform.Find("WinLineText");
        if (textTransform != null)
        {
            var tmpText = textTransform.GetComponent<TMPro.TMP_Text>();
            if (tmpText != null)
            {
                tmpText.text = winAmount.ToString("0.###");
            }
            AnimateTextScaleAppear(textTransform);
        }
    }

    private void HideAllWinLineTexts()
    {
        if (reelImagesList == null) return;
        foreach (var reel in reelImagesList)
        {
            if (reel.images != null)
            {
                foreach (var image in reel.images)
                {
                    if (image != null)
                    {
                        Transform textTransform = image.transform.Find("WinLineText");
                        if (textTransform != null)
                        {
                            textTransform.DOKill();
                            textTransform.localScale = Vector3.one;
                            textTransform.gameObject.SetActive(false);
                        }
                    }
                }
            }
        }
    }

    private void ShowPhase1TotalWin(double totalWinAmount)
    {
        if (phase1TotalWinText != null)
        {
            phase1TotalWinText.text = totalWinAmount.ToString("0.###");
            AnimateTextScaleAppear(phase1TotalWinText.transform);
        }
    }

    private void HidePhase1TotalWinText()
    {
        if (phase1TotalWinText != null)
        {
            phase1TotalWinText.transform.DOKill();
            phase1TotalWinText.transform.localScale = Vector3.one;
            phase1TotalWinText.gameObject.SetActive(false);
        }
    }

    private void AnimateTextScaleAppear(Transform textTransform, float popScale = 1.2f, float durationUp = 0.15f, float durationDown = 0.10f)
    {
        if (textTransform == null) return;
        textTransform.DOKill();
        textTransform.localScale = Vector3.zero;
        textTransform.gameObject.SetActive(true);

        Sequence seq = DOTween.Sequence();
        seq.Append(textTransform.DOScale(popScale, durationUp).SetEase(Ease.OutQuad));
        seq.Append(textTransform.DOScale(1.0f, durationDown).SetEase(Ease.InQuad));
        winTweens.Add(seq);
    }
    private void ResetSymbolScale(int col, int row)
    {
        if (col >= reelImagesList.Count) return;
        var reel = reelImagesList[col];
        if (reel.images == null) return;
        int imageIndex = 2 + row;
        if (imageIndex >= reel.images.Count) return;
        if (reel.images[imageIndex] != null)
        {
            reel.images[imageIndex].DOKill();
            reel.images[imageIndex].transform.localScale = Vector3.one;
            // Restore alpha to full opacity
            Color c = reel.images[imageIndex].color;
            reel.images[imageIndex].color = new Color(c.r, c.g, c.b, 1f);
        }

        // Also ensure the corresponding animation object is disabled
        var animGO = WinBox(winAnimationColumns, col, row);
        if (animGO != null)
        {
            ImageAnimation imageAnim = animGO.GetComponent<ImageAnimation>();
            if (imageAnim != null)
            {
                if (imageAnim.rendererDelegate != null) imageAnim.rendererDelegate.DOKill();
                imageAnim.StopAnimation();
            }
            animGO.SetActive(false);
        }
    }


    private void AnimateWinSymbol(int column, int row)
    {

        if (column >= reelImagesList.Count)
        {
            Debug.LogError($"[AnimateWinSymbol] Invalid column {column}, max is {reelImagesList.Count - 1}");
            return;
        }

        var reel = reelImagesList[column];
        if (reel.images == null || reel.images.Count < 3)
        {
            Debug.LogError($"[AnimateWinSymbol] Reel {column} has invalid images list");
            return;
        }

        int imageIndex = 2 + row;
        if (imageIndex >= reel.images.Count)
        {
            Debug.LogError($"[AnimateWinSymbol] Image index {imageIndex} out of range for reel {column}");
            return;
        }

        Image symbolImage = reel.images[imageIndex];
        if (symbolImage == null)
        {
            Debug.LogError($"[AnimateWinSymbol] Symbol image is NULL at col: {column}, row: {row}, imageIndex: {imageIndex}");
            return;
        }



        // Get the animation GameObject for this position
        var animGO = WinBox(winAnimationColumns, column, row);
        if (animGO == null)
        {
            Debug.LogError($"[AnimateWinSymbol] Animation GameObject is NULL at col: {column}, row: {row}");
            return;
        }

        // Get the ImageAnimation component
        ImageAnimation imageAnim = animGO.GetComponent<ImageAnimation>();
        if (imageAnim == null)
        {
            Debug.LogError($"[AnimateWinSymbol] ImageAnimation component not found on animation object at col: {column}, row: {row}");
            return;
        }

        // Get the current symbol ID at this position
        if (column >= currentDisplayMatrix.Count || row >= currentDisplayMatrix[column].Count)
        {
            Debug.LogError($"[AnimateWinSymbol] Invalid matrix position col: {column}, row: {row}");
            return;
        }

        int symbolId = currentDisplayMatrix[column][row];
        
        // Validate symbolId
        if (symbolId < 0 || symbolId >= animationSpriteArrays.Length)
        {
            Debug.LogError($"[AnimateWinSymbol] Invalid symbolId {symbolId} at col: {column}, row: {row}");
            return;
        }

        // Get the animation sprite array for this symbol
        List<Sprite> animSprites = animationSpriteArrays[symbolId];
        if (animSprites == null || animSprites.Count == 0)
        {
            // Expected for most symbols now
            return;
        }

        // Set the sprite array on the ImageAnimation component
        imageAnim.textureArray = animSprites;

        Color originalColor = new Color(symbolImage.color.r, symbolImage.color.g, symbolImage.color.b, 1f);

        Sequence seq = DOTween.Sequence();
        
        seq.AppendCallback(() => {
            animGO.SetActive(true);
            Image animRenderer = imageAnim.rendererDelegate;
            if (animRenderer != null)
            {
                animRenderer.DOKill();
                Color c = animRenderer.color;
                animRenderer.color = new Color(c.r, c.g, c.b, 1f);
            }
            symbolImage.DOKill();
            symbolImage.DOFade(0f, 0.2f);
        });

        if (winLineBoxToAnimationDelay > 0)
        {
            seq.AppendInterval(winLineBoxToAnimationDelay);
        }

        seq.AppendCallback(() => {
            imageAnim.StartAnimation();
        });

        int loopCount = (gameManager != null && (gameManager.isInFreeSpins || gameManager.isAutoPlaying)) ? 1 : winSymbolLoopCount;
        seq.AppendInterval(winSymbolLoopDuration * loopCount);

        seq.AppendCallback(() => {
            Image animRenderer = imageAnim != null ? imageAnim.rendererDelegate : null;

            if (animRenderer != null)
            {
                animRenderer.DOKill();
                Color c = animRenderer.color;
                animRenderer.color = new Color(c.r, c.g, c.b, 1f);
            }

            if (imageAnim != null) imageAnim.StopAnimation();
            if (animGO != null) animGO.SetActive(false);

            if (symbolImage != null)
            {
                symbolImage.DOKill();
                symbolImage.DOFade(originalColor.a, 0.2f);
            }
        });

        winTweens.Add(seq);
    }

    private void KillWinTweens(bool stopCoroutine = true)
    {
        foreach (var tween in winTweens)
        {
            tween?.Kill();
        }
        winTweens.Clear();

        if (stopCoroutine && winAnimationCoroutine != null)
        {
            StopCoroutine(winAnimationCoroutine);
            winAnimationCoroutine = null;
        }

        // Stop all win animations and disable animation GameObjects
        if (winAnimationColumns != null)
        {
            foreach (var col in winAnimationColumns)
            {
                if (col?.rows != null)
                {
                    foreach (var animGO in col.rows)
                    {
                        if (animGO != null)
                        {
                            ImageAnimation imageAnim = animGO.GetComponent<ImageAnimation>();
                            if (imageAnim != null)
                            {
                                imageAnim.onLoopComplete = null;
                                if (imageAnim.rendererDelegate != null)
                                {
                                    imageAnim.rendererDelegate.DOKill();
                                    Color ac = imageAnim.rendererDelegate.color;
                                    imageAnim.rendererDelegate.color = new Color(ac.r, ac.g, ac.b, 1f);
                                }
                                imageAnim.StopAnimation();
                            }
                            if (animGO.activeSelf)
                            {
                                animGO.SetActive(false);
                            }
                        }
                    }
                }
            }
        }

        DisableColumns(winAnimationColumns);
        HideAllWinLineTexts();
        HidePhase1TotalWinText();

        // Restore all symbol image alphas to full opacity
        foreach (var reel in reelImagesList)
        {
            if (reel.images != null)
            {
                foreach (var image in reel.images)
                {
                    if (image != null)
                    {
                        image.DOKill();
                        image.transform.localScale = Vector3.one;
                        Color c = image.color;
                        image.color = new Color(c.r, c.g, c.b, 1f);
                    }
                }
            }
        }
    }

    #endregion


    
    internal List<List<int>> GetCurrentDisplayMatrix()
    {
        return currentDisplayMatrix;
    }

    internal bool IsSpinning()
    {
        return isSpinning;
    }


    private void KillAllTweens()
    {
        foreach (var tween in spinTweens)
        {
            tween?.Kill();
        }
        spinTweens.Clear();

        KillWinTweens();
    }

    #region Blank Symbol Handling (Simplified 2-Case Architecture)

    private Sprite GetRandomNonBlankSprite()
    {
        List<int> nonBlankIds = new List<int>();
        for (int i = 1; i < symbolSprites.Length; i++)
        {
            if (symbolSprites[i] != null)
            {
                nonBlankIds.Add(i);
            }
        }
        if (nonBlankIds.Count == 0) return symbolSprites.Length > 1 ? symbolSprites[1] : symbolSprites[0];
        int randomId = nonBlankIds[Random.Range(0, nonBlankIds.Count)];
        return GetSymbolSprite(randomId);
    }

    #endregion

    #region Cleanup

    private void OnDestroy()
    {
        KillAllTweens();
    }

    #endregion
}

[System.Serializable]
public class ReelImages
{
    public List<Image> images = new List<Image>(16);
}


[System.Serializable]
public class ColumnOverlays
{
    [Tooltip("Row 0 = top, Row 1, Row 2 = bottom")]
    public GameObject[] rows = new GameObject[3];
}