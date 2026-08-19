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
    [SerializeField] private float winSymbolLoopDuration = 1.2f;

    [Header("Phase 1 Total Win Presentation")]
    [SerializeField] private TMPro.TMP_Text phase1TotalWinText;
    private Coroutine winTextDisableCoroutine;

    [Header("Win Animation Objects — Col 0..4  (each has 2 rows, contains ImageAnimation component)")]
    [SerializeField] private GameObject winAnimationParent;
    [Tooltip("GameObject references for win animations. Each should have an ImageAnimation component attached.")]
    [SerializeField] private ColumnOverlays[] winAnimationColumns = new ColumnOverlays[5];


    [Header("Symbol Info Card")]
    [SerializeField] private SymbolInfoCard symbolInfoCard;

    [Header("Cylindrical Spin Effect Settings")]
    [SerializeField] private bool enableCylindricalEffect = true;
    [Tooltip("Optional parent RectTransform reference (e.g. reel viewport frame) to automatically measure visible half height from parent rect height.")]
    [SerializeField] private RectTransform visibleAreaRectTransform;
    [SerializeField] private float leftReelEdgeX = 70f;
    [SerializeField] private float rightReelEdgeX = -70f;
    [SerializeField] private float leftReelOuterX = 105f;
    [SerializeField] private float rightReelOuterX = -105f;
    [SerializeField] private float edgeScale = 0.94f;
    [SerializeField] private float outerScale = 0.90f;
    [SerializeField] private float visibleHalfHeight = 145f;
    [SerializeField] private float outerHalfHeight = 220f;

    private float[] reelCurveIntensity = new float[3] { 1f, 1f, 1f };
    private Tween[] reelSettleCurveTweens = new Tween[3];
    private Coroutine cylindricalEffectCoroutine;


    private float middlePosition = 0f;
    private float cycleDistance;


    private List<Tween> spinTweens = new List<Tween>();
    private List<Tween> winTweens = new List<Tween>();
    private List<int> reelCycleCount = new List<int>();
    private Coroutine winAnimationCoroutine;


    internal List<List<int>> currentDisplayMatrix;

    private bool isSpinning;

    #region Initialization

    private Dictionary<GameObject, Vector3> originalWinBoxLocalPositions;

    private void CacheOriginalWinBoxPositions()
    {
        if (winAnimationColumns == null) return;
        if (originalWinBoxLocalPositions == null)
            originalWinBoxLocalPositions = new Dictionary<GameObject, Vector3>();
        else
            originalWinBoxLocalPositions.Clear();

        foreach (var colOverlay in winAnimationColumns)
        {
            if (colOverlay != null && colOverlay.rows != null)
            {
                foreach (var go in colOverlay.rows)
                {
                    if (go != null && !originalWinBoxLocalPositions.ContainsKey(go))
                    {
                        originalWinBoxLocalPositions[go] = go.transform.localPosition;
                    }
                }
            }
        }
    }

    private Vector3 GetOriginalWinBoxPosition(GameObject go)
    {
        if (go != null && originalWinBoxLocalPositions != null && originalWinBoxLocalPositions.TryGetValue(go, out Vector3 origPos))
        {
            return origPos;
        }
        return go != null ? go.transform.localPosition : Vector3.zero;
    }

    private void ResetWinBoxPosition(GameObject go)
    {
        if (go != null && originalWinBoxLocalPositions != null && originalWinBoxLocalPositions.TryGetValue(go, out Vector3 origPos))
        {
            go.transform.localPosition = origPos;
        }
    }

    private void Start()
    {
        BuildSymbolSpriteArray();
        InitializeReels();
        CacheOriginalWinBoxPositions();
        DisableAllOverlays();
        SetupSymbolButtons();
    }

    private void DisableAllOverlays()
    {
        DisableColumns(winAnimationColumns);
        if (winAnimationParent) winAnimationParent.SetActive(false);
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

    internal void OnBetChanged()
    {
        if (symbolInfoCard != null && symbolInfoCard.gameObject.activeSelf)
        {
            symbolInfoCard.RefreshCard(gameManager);
        }
    }

    private Dictionary<Image, int> imageToSymbolIdMap = new Dictionary<Image, int>();

    private void SetImageSymbol(Image img, int symbolId)
    {
        if (img == null) return;
        img.sprite = GetSymbolSprite(symbolId);
        imageToSymbolIdMap[img] = symbolId;
    }

    private int GetRandomNonBlankSymbolId(List<int> nonBlankIds = null)
    {
        if (nonBlankIds == null || nonBlankIds.Count == 0)
        {
            nonBlankIds = new List<int>();
            if (symbolSprites != null)
            {
                for (int i = 0; i < symbolSprites.Length; i++)
                {
                    if (symbolSprites[i] != null && i != 0) nonBlankIds.Add(i);
                }
            }
        }
        if (nonBlankIds.Count == 0) return 1;
        return nonBlankIds[Random.Range(0, nonBlankIds.Count)];
    }

    internal void OnSymbolClicked(int col, int row, RectTransform symbolRect)
    {
        if (isSpinning)
        {
            if (symbolInfoCard != null) symbolInfoCard.HideCard();
            return;
        }

        if (col >= reelImagesList.Count) return;

        var reel = reelImagesList[col];
        if (reel == null || reel.images == null) return;

        float customYOffset = 0f;
        if (reelTransforms != null && col < reelTransforms.Length && reelTransforms[col] != null)
        {
            float reelY = reelTransforms[col].localPosition.y;
            bool isCase1ReelPos = Mathf.Abs(reelY - (-160f)) < 30f;
            bool isCase1Matrix = (currentDisplayMatrix != null && col < currentDisplayMatrix.Count && 
                                  currentDisplayMatrix[col] != null && currentDisplayMatrix[col].Count >= 3 && 
                                  currentDisplayMatrix[col][1] != 0);

            if (isCase1ReelPos || isCase1Matrix)
            {
                if (row == 0) customYOffset = -10f;      // 7th image: -10 Y offset
                else if (row == 2) customYOffset = 10f;  // 9th image: +10 Y offset
            }
        }

        int imageIndex = 6 + row;
        if (imageIndex < reel.images.Count && reel.images[imageIndex] != null)
        {
            Image clickedImage = reel.images[imageIndex];
            if (imageToSymbolIdMap.TryGetValue(clickedImage, out int symbolId))
            {
                if (symbolInfoCard != null)
                {
                    symbolInfoCard.ShowCard(symbolId, col, row, symbolRect, gameManager, customYOffset);
                }
                return;
            }
        }

        if (currentDisplayMatrix != null && col < currentDisplayMatrix.Count && row < currentDisplayMatrix[col].Count)
        {
            int fallbackId = currentDisplayMatrix[col][row];
            if (symbolInfoCard != null)
            {
                symbolInfoCard.ShowCard(fallbackId, col, row, symbolRect, gameManager, customYOffset);
            }
        }
    }

    private void DisableColumns(ColumnOverlays[] cols)
    {
        if (cols == null) return;
        foreach (var col in cols)
        {
            if (col?.rows != null)
            {
                foreach (var go in col.rows)
                {
                    if (go)
                    {
                        ResetWinBoxPosition(go);
                        go.SetActive(false);
                    }
                }
            }
        }
    }

    private GameObject GetWinBoxObject(int col, int row)
    {
        if (winAnimationColumns == null || col < 0 || col >= winAnimationColumns.Length) return null;
        var overlay = winAnimationColumns[col];
        if (overlay == null || overlay.rows == null || overlay.rows.Length == 0) return null;

        if (winAnimationParent && !winAnimationParent.activeSelf)
        {
            winAnimationParent.SetActive(true);
        }

        GameObject animGO = null;

        if (row == 0)
        {
            // Case 2 Top icon -> 1st object (rows[0])
            animGO = overlay.rows[0];
            ResetWinBoxPosition(animGO);
        }
        else if (row == 2)
        {
            // Case 2 Bottom icon -> 2nd object (rows[1] if present, fallback rows[0])
            animGO = overlay.rows.Length > 1 ? overlay.rows[1] : overlay.rows[0];
            ResetWinBoxPosition(animGO);
        }
        else if (row == 1)
        {
            // Case 1 Middle icon -> 1st object (rows[0]), set y to 6.5f before enabling
            animGO = overlay.rows[0];
            if (animGO != null)
            {
                Vector3 basePos = GetOriginalWinBoxPosition(animGO);
                animGO.transform.localPosition = new Vector3(basePos.x, 6.5f, basePos.z);
            }
        }

        return animGO;
    }

    private GameObject WinBox(ColumnOverlays[] cols, int col, int row)
    {
        if (cols == winAnimationColumns)
        {
            return GetWinBoxObject(col, row);
        }
        return (col >= 0 && col < cols?.Length && cols[col]?.rows != null && row >= 0 && row < cols[col].rows.Length)
            ? cols[col].rows[row] : null;
    }

    private void BuildSymbolSpriteArray()
    {
        // Build the symbol sprite array from named sprite fields (size 15 for IDs 0..14)
        symbolSprites = new Sprite[15];
        symbolSprites[1] = spriteRed3X;        // ID 1: Red3X (Wild)
        symbolSprites[2] = spriteBlue2X;       // ID 2: Blue2X (Wild)
        symbolSprites[3] = spriteBlue7;        // ID 3: Blue7
        symbolSprites[4] = spriteWhite7;       // ID 4: White7
        symbolSprites[5] = spriteWhite7Bar;    // ID 5: White7Bar
        symbolSprites[6] = spriteRed7;         // ID 6: Red7
        symbolSprites[7] = spriteTripleBar;    // ID 7: TripleBar
        symbolSprites[8] = spriteDoubleBar;    // ID 8: DoubleBar
        symbolSprites[9] = spriteSingleBar;    // ID 9: SingleBar
        symbolSprites[10] = spriteSpin;        // ID 10: Spin (Wheel)
        symbolSprites[11] = spriteGreenWheel;  // ID 11: Green Wheel (Wheel)
        symbolSprites[12] = spriteDoubleWheel; // ID 12: Double Wheel (Wheel)
        symbolSprites[13] = spriteRedWheel;    // ID 13: Red Wheel (Wheel)
        symbolSprites[14] = spriteRedWheel;    // ID 14: Red Wheel (Wheel)

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
        animationSpriteArrays[10] = animSpritesSpin;
        animationSpriteArrays[11] = animSpritesGreenWheel;
        animationSpriteArrays[12] = animSpritesDoubleWheel;
        animationSpriteArrays[13] = animSpritesRedWheel;
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
            if (col < reelCurveIntensity.Length && matrix[col] != null && matrix[col].Count >= 3)
            {
                bool isCase1 = matrix[col][1] != 0;
                reelCurveIntensity[col] = isCase1 ? 1f : 0f;
            }

            if (col < reelImagesList.Count)
            {
                SetReelSymbols(col, matrix[col], true);
            }
        }

        UpdateCylindricalSpinEffect();
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
        List<int> nonBlankIds = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 };

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

            int midId = (visibleSymbolIds != null && visibleSymbolIds.Count > 1) ? visibleSymbolIds[1] : 1;
            int topId = GetRandomNonBlankSymbolId(nonBlankIds);
            int botId = GetRandomNonBlankSymbolId(nonBlankIds);

            SetImageSymbol(reel.images[7], midId); // 8th element (mid icon)
            SetImageSymbol(reel.images[6], topId); // 7th element (random top)
            SetImageSymbol(reel.images[8], botId); // 9th element (random bottom)
        }
        else
        {
            // Case 2: icon, blank, icon -> Stop at 0f
            // Visible elements are 7th, 8th (0-based array indices 6, 7)
            reservedIndices.Add(6);
            reservedIndices.Add(7);

            int topSymbolId = (visibleSymbolIds != null && visibleSymbolIds.Count > 0) ? visibleSymbolIds[0] : 1;
            int botSymbolId = (visibleSymbolIds != null && visibleSymbolIds.Count > 2) ? visibleSymbolIds[2] : 1;

            SetImageSymbol(reel.images[6], topSymbolId); // 7th element (1st row result)
            SetImageSymbol(reel.images[7], botSymbolId); // 8th element (3rd row result)
        }

        // Populate all other buffer images (outside reserved indices) with non-blank symbols
        for (int i = 0; i < reel.images.Count; i++)
        {
            if (reservedIndices.Contains(i)) continue;

            int symId = nonBlankIds[bufferIndex % nonBlankIds.Count];
            bufferIndex++;
            SetImageSymbol(reel.images[i], symId);
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

        for (int i = 0; i < reelCurveIntensity.Length; i++)
        {
            if (reelSettleCurveTweens[i] != null)
            {
                reelSettleCurveTweens[i].Kill();
                reelSettleCurveTweens[i] = null;
            }
        }

        DisableAllOverlays();

        StartCylindricalEffectCoroutine();

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

        if (columnIndex < reelCurveIntensity.Length)
        {
            if (reelSettleCurveTweens[columnIndex] != null)
            {
                reelSettleCurveTweens[columnIndex].Kill();
                reelSettleCurveTweens[columnIndex] = null;
            }
            reelCurveIntensity[columnIndex] = 1f;
        }

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

        // Explicitly kill and clear all spin & settle tweens after reel stop sequence completes
        foreach (var tween in spinTweens)
        {
            tween?.Kill();
        }
        spinTweens.Clear();

        if (reelSettleCurveTweens != null)
        {
            for (int i = 0; i < reelSettleCurveTweens.Length; i++)
            {
                if (reelSettleCurveTweens[i] != null)
                {
                    reelSettleCurveTweens[i].Kill();
                    reelSettleCurveTweens[i] = null;
                }
            }
        }

        if (cylindricalEffectCoroutine != null)
        {
            StopCoroutine(cylindricalEffectCoroutine);
            cylindricalEffectCoroutine = null;
        }

        UpdateCylindricalSpinEffect(force: true);

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
        slotTransform.DOKill();

        float targetY = GetTargetYForResult(targetSymbols);

        // Set target symbols & non-blank buffer images for active case
        SetReelSymbols(columnIndex, targetSymbols, false);

        bool isCase1 = targetSymbols != null && targetSymbols.Count >= 3 && targetSymbols[1] != 0;
        if (isCase1)
        {
            if (columnIndex < reelCurveIntensity.Length)
            {
                if (reelSettleCurveTweens[columnIndex] != null) reelSettleCurveTweens[columnIndex].Kill();
                reelCurveIntensity[columnIndex] = 1f;
            }
        }
        else
        {
            // Case 2: every icon at stop needs scale=1 and x=0
            if (columnIndex < reelCurveIntensity.Length)
            {
                if (reelSettleCurveTweens[columnIndex] != null) reelSettleCurveTweens[columnIndex].Kill();
                float settleDuration = isQuickStop ? (quickStopDuration * 0.7f) : stopSettleDuration;
                int colIdx = columnIndex;
                reelSettleCurveTweens[colIdx] = DOVirtual.Float(reelCurveIntensity[colIdx], 0f, settleDuration, (val) => {
                    if (colIdx < reelCurveIntensity.Length) reelCurveIntensity[colIdx] = val;
                });
            }
            StartCylindricalEffectCoroutine();
        }

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
                }
            }
            
            onComplete?.Invoke();
            return;
        }

        StartCoroutine(StopSpinSequence(resultMatrix, onComplete, true));
    }

    #endregion

    internal void AnimateDualWheelWin(System.Action onComplete = null)
    {
        if (currentDisplayMatrix == null)
        {
            onComplete?.Invoke();
            return;
        }

        KillWinTweens();
        AudioManager.Instance?.PlayWinLinePhase1Start();

        List<ImageAnimation> activeWheelAnims = new List<ImageAnimation>();
        int completedCount = 0;
        int targetLoops = 2;

        for (int col = 0; col < 5; col++)
        {
            if (col >= currentDisplayMatrix.Count) continue;
            for (int row = 0; row < currentDisplayMatrix[col].Count; row++)
            {
                int symId = currentDisplayMatrix[col][row];
                if (symId >= 10 && symId <= 13) // Wheel Symbol IDs (10..13: Spin, Green, Double, Red Wheel)
                {
                    var animGO = WinBox(winAnimationColumns, col, row);
                    if (animGO != null)
                    {
                        ImageAnimation imageAnim = animGO.GetComponentInChildren<ImageAnimation>();
                        int imageIndex = 6 + row;
                        Image symbolImage = (col < reelImagesList.Count && reelImagesList[col].images != null && imageIndex < reelImagesList[col].images.Count)
                            ? reelImagesList[col].images[imageIndex]
                            : null;

                        if (imageAnim != null)
                        {
                            activeWheelAnims.Add(imageAnim);

                            List<Sprite> animSprites = (animationSpriteArrays != null && symId >= 0 && symId < animationSpriteArrays.Length) ? animationSpriteArrays[symId] : null;
                            if (animSprites != null && animSprites.Count > 0)
                            {
                                imageAnim.textureArray = animSprites;
                            }
                            imageAnim.animationMode = ImageAnimation.AnimationMode.SINGLE_PHASE;
                            imageAnim.useDynamicFramerate = true;
                            imageAnim.dynamicLoopDuration = winSymbolLoopDuration;
                            imageAnim.doLoopAnimation = true;
                            imageAnim.delayBetweenLoop = 0f;

                            animGO.SetActive(true);
                            Image animRenderer = imageAnim.rendererDelegate != null ? imageAnim.rendererDelegate : imageAnim.GetComponent<Image>();
                            if (animRenderer == null && animGO != null) animRenderer = animGO.GetComponentInChildren<Image>();
                            if (animRenderer != null)
                            {
                                animRenderer.DOKill();
                                Color c = animRenderer.color;
                                animRenderer.color = new Color(c.r, c.g, c.b, 1f);
                                animRenderer.enabled = true;
                                animRenderer.gameObject.SetActive(true);
                            }
                            if (symbolImage != null)
                            {
                                symbolImage.DOKill();
                                Color c = symbolImage.color;
                                symbolImage.color = new Color(c.r, c.g, c.b, 0f);
                                symbolImage.enabled = false;
                                symbolImage.gameObject.SetActive(false);
                            }

                            imageAnim.onLoopComplete = (loopCount) =>
                            {
                                if (loopCount >= targetLoops)
                                {
                                    imageAnim.onLoopComplete = null;
                                    imageAnim.StopAnimation();
                                    if (animGO != null)
                                    {
                                        ResetWinBoxPosition(animGO);
                                        animGO.SetActive(false);
                                    }

                                    if (symbolImage != null)
                                    {
                                        symbolImage.DOKill();
                                        Color c = symbolImage.color;
                                        symbolImage.color = new Color(c.r, c.g, c.b, 1f);
                                        symbolImage.enabled = true;
                                        symbolImage.gameObject.SetActive(true);
                                    }

                                    completedCount++;
                                    if (completedCount >= activeWheelAnims.Count)
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

        if (activeWheelAnims.Count == 0)
        {
            onComplete?.Invoke();
        }
    }



    private void AnimateSymbolSingleLoop(int column, int row, int loopCount = 1)
    {
        if (column >= reelImagesList.Count) return;

        var reel = reelImagesList[column];
        if (reel.images == null || reel.images.Count < 3) return;

        int imageIndex = 6 + row;
        if (imageIndex >= reel.images.Count) return;

        Image symbolImage = reel.images[imageIndex];
        if (symbolImage == null) return;

        var animGO = WinBox(winAnimationColumns, column, row);
        if (animGO == null) return;

        ImageAnimation imageAnim = animGO.GetComponent<ImageAnimation>();
        if (imageAnim == null) return;

        int symbolId = currentDisplayMatrix[column][row];
        if (symbolId < 0 || symbolId >= animationSpriteArrays.Length) return;
        if (symbolId >= 10 && symbolId <= 13) return;

        List<Sprite> animSprites = animationSpriteArrays[symbolId];
        if (animSprites == null || animSprites.Count == 0) return;

        imageAnim.textureArray = animSprites;
        imageAnim.useDynamicFramerate = true;
        imageAnim.dynamicLoopDuration = winSymbolLoopDuration;

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
            if (symbolImage != null)
            {
                symbolImage.DOKill();
                symbolImage.gameObject.SetActive(false);
            }
            
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
            if (animGO != null)
            {
                ResetWinBoxPosition(animGO);
                animGO.SetActive(false);
            }

            if (symbolImage != null)
            {
                symbolImage.DOKill();
                Color c = symbolImage.color;
                symbolImage.color = new Color(c.r, c.g, c.b, originalColor.a);
                symbolImage.gameObject.SetActive(true);
            }
        });

        winTweens.Add(seq);
    }

    #region Win Line Animation

    internal void ShowWinLineAnimation(List<WinLine> winLines, System.Action onComplete)
    {
        if (winLines == null || winLines.Count == 0)
        {
            onComplete?.Invoke();
            return;
        }

        KillWinTweens();
        winAnimationCoroutine = StartCoroutine(PlaySingleWinLineAnimation(winLines, onComplete));
    }

    private IEnumerator PlaySingleWinLineAnimation(List<WinLine> winLines, System.Action onComplete)
    {
        WinLine winLine = winLines[0];
        if (winLine == null || winLine.positions == null || winLine.positions.Count == 0)
        {
            onComplete?.Invoke();
            yield break;
        }

        int reelCount = (gameManager != null && gameManager.gameConfig != null) ? gameManager.gameConfig.reelCount : 3;

        // Show win line text with win amount using scene winLineText
        ShowPhase1TotalWin(winLine.winAmount);

        AudioManager.Instance?.PlayWinLinePhase1Start();

        bool isAutoPlaying = (gameManager != null && gameManager.isAutoPlaying);

        if (isAutoPlaying)
        {
            // Autoplay mode: Only 1 loop of animation
            yield return StartCoroutine(AnimateWinPositionsSingleLoop(winLine.positions));
            HidePhase1TotalWinText();
            onComplete?.Invoke();
        }
        else
        {
            // Normal mode: Enable animation objects, show winline text, and loop continuously
            StartContinuousWinAnimation(winLine.positions);
            onComplete?.Invoke();
        }
    }

    private IEnumerator AnimateWinPositionsSingleLoop(IEnumerable<int> flatPositions)
    {
        if (flatPositions == null) yield break;

        int reelCount = (gameManager != null && gameManager.gameConfig != null) ? gameManager.gameConfig.reelCount : 3;
        int rowLimit = (gameManager != null && gameManager.gameConfig != null) ? gameManager.gameConfig.rowCount : 3;

        // Check if this winning line contains Red 3X (ID 1) or Blue 2X (ID 2)
        bool hasWild3X2X = false;
        if (currentDisplayMatrix != null)
        {
            foreach (int flatIndex in flatPositions)
            {
                int r = flatIndex / reelCount;
                int c = flatIndex % reelCount;
                if (c >= 0 && c < currentDisplayMatrix.Count && r >= 0 && r < currentDisplayMatrix[c].Count)
                {
                    int symId = currentDisplayMatrix[c][r];
                    if (symId == 1 || symId == 2)
                    {
                        hasWild3X2X = true;
                        break;
                    }
                }
            }
        }

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

            int imageIndex = 6 + row;
            if (imageIndex >= reel.images.Count) continue;

            Image symbolImage = reel.images[imageIndex];
            if (symbolImage == null) continue;

            var animGO = WinBox(winAnimationColumns, col, row);
            if (animGO == null) continue;

            ImageAnimation imageAnim = animGO.GetComponentInChildren<ImageAnimation>();
            if (imageAnim == null) continue;

            if (currentDisplayMatrix == null || col >= currentDisplayMatrix.Count || row >= currentDisplayMatrix[col].Count) continue;
            int symbolId = currentDisplayMatrix[col][row];
            if (symbolId < 0 || symbolId >= animationSpriteArrays.Length) continue;

            // If win line contains Red 3X (1) or Blue 2X (2), animate ONLY Red 3X / Blue 2X; otherwise animate all normal symbols except wheels
            if (hasWild3X2X)
            {
                if (symbolId != 1 && symbolId != 2) continue;
            }
            else
            {
                if (symbolId >= 10 && symbolId <= 13) continue;
            }

            List<Sprite> animSprites = animationSpriteArrays[symbolId];
            if (animSprites == null || animSprites.Count == 0) continue;

            imageAnim.textureArray = animSprites;
            imageAnim.animationMode = ImageAnimation.AnimationMode.SINGLE_PHASE;
            imageAnim.useDynamicFramerate = true;
            imageAnim.dynamicLoopDuration = winSymbolLoopDuration;
            imageAnim.doLoopAnimation = true;
            imageAnim.delayBetweenLoop = 0f;

            animGO.SetActive(true);
            Image animRenderer = imageAnim.rendererDelegate != null ? imageAnim.rendererDelegate : imageAnim.GetComponent<Image>();
            if (animRenderer == null && animGO != null) animRenderer = animGO.GetComponentInChildren<Image>();
            if (animRenderer != null)
            {
                animRenderer.DOKill();
                Color c = animRenderer.color;
                animRenderer.color = new Color(c.r, c.g, c.b, 1f);
                animRenderer.enabled = true;
                animRenderer.gameObject.SetActive(true);
            }

            if (symbolImage != null)
            {
                symbolImage.DOKill();
                Color c = symbolImage.color;
                symbolImage.color = new Color(c.r, c.g, c.b, 0f);
                symbolImage.enabled = false;
                symbolImage.gameObject.SetActive(false);
            }

            activeAnims.Add(imageAnim);

            imageAnim.onLoopComplete = (currentLoop) =>
            {
                if (currentLoop >= 1)
                {
                    imageAnim.onLoopComplete = null;
                    imageAnim.StopAnimation();
                    if (animGO != null)
                    {
                        ResetWinBoxPosition(animGO);
                        animGO.SetActive(false);
                    }

                    if (symbolImage != null)
                    {
                        symbolImage.DOKill();
                        Color c = symbolImage.color;
                        symbolImage.color = new Color(c.r, c.g, c.b, 1f);
                        symbolImage.enabled = true;
                        symbolImage.gameObject.SetActive(true);
                    }

                    completedCount++;
                    if (completedCount >= activeAnims.Count)
                    {
                        isCompleted = true;
                    }
                }
            };
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

    private void StartContinuousWinAnimation(IEnumerable<int> flatPositions)
    {
        if (flatPositions == null) return;

        int reelCount = (gameManager != null && gameManager.gameConfig != null) ? gameManager.gameConfig.reelCount : 3;
        int rowLimit = (gameManager != null && gameManager.gameConfig != null) ? gameManager.gameConfig.rowCount : 3;

        // Check if this winning line contains Red 3X (ID 1) or Blue 2X (ID 2)
        bool hasWild3X2X = false;
        if (currentDisplayMatrix != null)
        {
            foreach (int flatIndex in flatPositions)
            {
                int r = flatIndex / reelCount;
                int c = flatIndex % reelCount;
                if (c >= 0 && c < currentDisplayMatrix.Count && r >= 0 && r < currentDisplayMatrix[c].Count)
                {
                    int symId = currentDisplayMatrix[c][r];
                    if (symId == 1 || symId == 2)
                    {
                        hasWild3X2X = true;
                        break;
                    }
                }
            }
        }

        if (winAnimationParent && !winAnimationParent.activeSelf)
        {
            winAnimationParent.SetActive(true);
        }

        foreach (int flatIndex in flatPositions)
        {
            int row = flatIndex / reelCount;
            int col = flatIndex % reelCount;

            if (col < 0 || col >= 5 || row < 0 || row >= rowLimit) continue;

            if (col >= reelImagesList.Count) continue;
            var reel = reelImagesList[col];
            if (reel.images == null || reel.images.Count < 3) continue;

            int imageIndex = 6 + row;
            if (imageIndex >= reel.images.Count) continue;

            Image symbolImage = reel.images[imageIndex];
            if (symbolImage == null) continue;

            var animGO = WinBox(winAnimationColumns, col, row);
            if (animGO == null) continue;

            ImageAnimation imageAnim = animGO.GetComponentInChildren<ImageAnimation>();
            if (imageAnim == null) continue;

            if (currentDisplayMatrix == null || col >= currentDisplayMatrix.Count || row >= currentDisplayMatrix[col].Count) continue;
            int symbolId = currentDisplayMatrix[col][row];
            if (symbolId < 0 || symbolId >= animationSpriteArrays.Length) continue;

            // If win line contains Red 3X (1) or Blue 2X (2), animate ONLY Red 3X / Blue 2X; otherwise animate all normal symbols except wheels
            if (hasWild3X2X)
            {
                if (symbolId != 1 && symbolId != 2) continue;
            }
            else
            {
                if (symbolId >= 10 && symbolId <= 13) continue;
            }

            List<Sprite> animSprites = animationSpriteArrays[symbolId];
            if (animSprites == null || animSprites.Count == 0) continue;

            imageAnim.textureArray = animSprites;
            imageAnim.animationMode = ImageAnimation.AnimationMode.SINGLE_PHASE;
            imageAnim.useDynamicFramerate = true;
            imageAnim.dynamicLoopDuration = winSymbolLoopDuration;
            imageAnim.doLoopAnimation = true;
            imageAnim.delayBetweenLoop = 0f;
            imageAnim.onLoopComplete = null;

            animGO.SetActive(true);

            Image animRenderer = imageAnim.rendererDelegate != null ? imageAnim.rendererDelegate : imageAnim.GetComponent<Image>();
            if (animRenderer == null && animGO != null) animRenderer = animGO.GetComponentInChildren<Image>();
            if (animRenderer != null)
            {
                animRenderer.DOKill();
                Color c = animRenderer.color;
                animRenderer.color = new Color(c.r, c.g, c.b, 1f);
                animRenderer.enabled = true;
                animRenderer.gameObject.SetActive(true);
            }

            if (symbolImage != null)
            {
                symbolImage.DOKill();
                Color c = symbolImage.color;
                symbolImage.color = new Color(c.r, c.g, c.b, 0f);
                symbolImage.enabled = false;
                symbolImage.gameObject.SetActive(false);
            }

            imageAnim.StartAnimation();
        }
    }

    private void ShowWinLineTextOnIcon(int col, int row, double winAmount)
    {
        if (col < 0 || col >= reelImagesList.Count) return;
        var reel = reelImagesList[col];
        if (reel.images == null) return;
        int imageIndex = 6 + row;
        if (imageIndex >= reel.images.Count) return;

        Image symbolImage = reel.images[imageIndex];
        if (symbolImage == null) return;

        Transform textTransform = symbolImage.transform.Find("WinLineText");
        if (textTransform != null)
        {
            var tmpText = textTransform.GetComponent<TMPro.TMP_Text>();
            if (tmpText != null)
            {
                tmpText.text = FormatSpriteText(winAmount);
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
            phase1TotalWinText.text = FormatSpriteText(totalWinAmount);
            AnimateTextScaleAppear(phase1TotalWinText.transform);

            if (winTextDisableCoroutine != null)
            {
                StopCoroutine(winTextDisableCoroutine);
            }
            winTextDisableCoroutine = StartCoroutine(DisableWinLineTextAfterDelay(1.0f));
        }
    }

    private IEnumerator DisableWinLineTextAfterDelay(float delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);
        HidePhase1TotalWinText();
    }

    /// <summary>
    /// Converts input string/number into TextMeshPro sprite asset tags based on mapping:
    /// 0..9 -> <sprite=0>..<sprite=9>
    /// '='  -> <sprite=10>
    /// '.'  -> <sprite=11>
    /// </summary>
    public static string FormatSpriteText(string input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        foreach (char c in input)
        {
            if (c >= '0' && c <= '9')
            {
                sb.Append("<sprite=").Append(c - '0').Append(">");
            }
            else if (c == '=')
            {
                sb.Append("<sprite=10>");
            }
            else if (c == '.' || c == ',')
            {
                sb.Append("<sprite=11>");
            }
            else
            {
                sb.Append(c);
            }
        }
        return sb.ToString();
    }

    public static string FormatSpriteText(double amount)
    {
        return FormatSpriteText(amount.ToString("0.###"));
    }

    private void HidePhase1TotalWinText()
    {
        if (winTextDisableCoroutine != null)
        {
            StopCoroutine(winTextDisableCoroutine);
            winTextDisableCoroutine = null;
        }

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
        int imageIndex = 6 + row;
        if (imageIndex >= reel.images.Count) return;
        if (reel.images[imageIndex] != null)
        {
            var img = reel.images[imageIndex];
            img.DOKill();
            img.transform.localScale = Vector3.one;
            // Restore alpha to full opacity and re-enable component & GameObject
            Color c = img.color;
            img.color = new Color(c.r, c.g, c.b, 1f);
            img.enabled = true;
            img.gameObject.SetActive(true);
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
                            ImageAnimation imageAnim = animGO.GetComponentInChildren<ImageAnimation>();
                            if (imageAnim != null)
                            {
                                imageAnim.onLoopComplete = null;
                                Image animRenderer = imageAnim.rendererDelegate != null ? imageAnim.rendererDelegate : imageAnim.GetComponent<Image>();
                                if (animRenderer == null) animRenderer = animGO.GetComponentInChildren<Image>();
                                if (animRenderer != null)
                                {
                                    animRenderer.DOKill();
                                    Color ac = animRenderer.color;
                                    animRenderer.color = new Color(ac.r, ac.g, ac.b, 1f);
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
        if (winAnimationParent) winAnimationParent.SetActive(false);
        HideAllWinLineTexts();
        HidePhase1TotalWinText();

        // Restore all symbol image alphas to full opacity and re-enable active state
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
                        image.enabled = true;
                        if (!image.gameObject.activeSelf)
                        {
                            image.gameObject.SetActive(true);
                        }
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

        if (reelSettleCurveTweens != null)
        {
            for (int i = 0; i < reelSettleCurveTweens.Length; i++)
            {
                if (reelSettleCurveTweens[i] != null)
                {
                    reelSettleCurveTweens[i].Kill();
                    reelSettleCurveTweens[i] = null;
                }
            }
        }

        if (cylindricalEffectCoroutine != null)
        {
            StopCoroutine(cylindricalEffectCoroutine);
            cylindricalEffectCoroutine = null;
        }

        KillWinTweens();
    }

    #region Cylindrical Spin Effect Coroutine

    private bool IsAnySettleTweenActive()
    {
        if (reelSettleCurveTweens == null) return false;
        for (int i = 0; i < reelSettleCurveTweens.Length; i++)
        {
            if (reelSettleCurveTweens[i] != null && reelSettleCurveTweens[i].IsActive() && reelSettleCurveTweens[i].IsPlaying())
                return true;
        }
        return false;
    }

    private void StartCylindricalEffectCoroutine()
    {
        if (!enableCylindricalEffect) return;
        if (cylindricalEffectCoroutine != null)
        {
            StopCoroutine(cylindricalEffectCoroutine);
            cylindricalEffectCoroutine = null;
        }
        cylindricalEffectCoroutine = StartCoroutine(CylindricalSpinEffectRoutine());
    }

    private IEnumerator CylindricalSpinEffectRoutine()
    {
        while (isSpinning || IsAnySettleTweenActive())
        {
            UpdateCylindricalSpinEffect(force: false);
            yield return null;
        }
        // Set final resting positions once when spin & settling finish
        UpdateCylindricalSpinEffect(force: true);
        cylindricalEffectCoroutine = null;
    }

    private void UpdateCylindricalSpinEffect(bool force = false)
    {
        if (!enableCylindricalEffect || reelTransforms == null || reelImagesList == null) return;

        int maxCols = Mathf.Min(reelTransforms.Length, reelImagesList.Count);

        float effectiveVisibleHalfHeight = visibleHalfHeight;
        if (visibleAreaRectTransform != null && visibleAreaRectTransform.rect.height > 0)
        {
            effectiveVisibleHalfHeight = visibleAreaRectTransform.rect.height * 0.5f;
        }
        float effectiveOuterHalfHeight = Mathf.Max(outerHalfHeight, effectiveVisibleHalfHeight * 1.5f);

        // Precalculate reciprocals to replace division with fast multiplication in loop
        float invVisibleHalfHeight = 1f / Mathf.Max(1f, effectiveVisibleHalfHeight);
        float invOuterRange = 1f / Mathf.Max(1f, effectiveOuterHalfHeight - effectiveVisibleHalfHeight);

        for (int col = 0; col < maxCols; col++)
        {
            Transform slotTransform = reelTransforms[col];
            if (slotTransform == null) continue;

            var reel = reelImagesList[col];
            if (reel == null || reel.images == null) continue;

            float intensity = (col < reelCurveIntensity.Length) ? reelCurveIntensity[col] : 1f;

            // Reference center image is 8th element (index 7)
            float centerImageLocalY = (reel.images.Count > 7 && reel.images[7] != null) ? reel.images[7].rectTransform.localPosition.y : -305.5f;
            float slotOffsetFromCase1 = slotTransform.localPosition.y - case1StopY;

            int imgCount = reel.images.Count;
            for (int i = 0; i < imgCount; i++)
            {
                Image img = reel.images[i];
                if (img == null) continue;

                RectTransform rect = img.rectTransform;
                if (rect == null) continue;

                // Vertical offset relative to Case 1 center position (Row 1)
                float yRel = (rect.localPosition.y - centerImageLocalY) + slotOffsetFromCase1;
                float absY = Mathf.Abs(yRel);

                float targetX = 0f;
                float targetScale = 1f;

                if (absY <= effectiveVisibleHalfHeight)
                {
                    // Inside visible area (0 to effectiveVisibleHalfHeight)
                    float t = absY * invVisibleHalfHeight;
                    float curveFactor = t * t * intensity; // Multiply by intensity for Case 2 stop settling

                    if (col == 0) // Left reel: curve outward to +70
                    {
                        targetX = Mathf.Lerp(0f, leftReelEdgeX, curveFactor);
                    }
                    else if (col == 2) // Right reel: curve outward to -70
                    {
                        targetX = Mathf.Lerp(0f, rightReelEdgeX, curveFactor);
                    }

                    targetScale = Mathf.Lerp(1f, edgeScale, curveFactor);
                }
                else
                {
                    // Outside visible area (entering diagonally from top / exiting to bottom)
                    float extraT = Mathf.Clamp01((absY - effectiveVisibleHalfHeight) * invOuterRange);

                    if (col == 0) // Left reel
                    {
                        targetX = Mathf.Lerp(leftReelEdgeX, leftReelOuterX, extraT) * intensity;
                    }
                    else if (col == 2) // Right reel
                    {
                        targetX = Mathf.Lerp(rightReelEdgeX, rightReelOuterX, extraT) * intensity;
                    }

                    targetScale = Mathf.Lerp(1f, Mathf.Lerp(edgeScale, outerScale, extraT), intensity);
                }

                Vector2 anchoredPos = rect.anchoredPosition;
                if (force || !Mathf.Approximately(anchoredPos.x, targetX))
                {
                    rect.anchoredPosition = new Vector2(targetX, anchoredPos.y);
                }

                Vector3 localScale = rect.localScale;
                if (force || !Mathf.Approximately(localScale.x, targetScale))
                {
                    rect.localScale = new Vector3(targetScale, targetScale, targetScale);
                }
            }
        }
    }

    #endregion

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
    [Tooltip("Row 0 = top (Case 2) / middle (Case 1 y=6.5), Row 1 = bottom (Case 2)")]
    public GameObject[] rows = new GameObject[2];
}