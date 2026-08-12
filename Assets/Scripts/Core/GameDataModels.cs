using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

#region Server Communication Models

[Serializable]
public class InitData
{
    public string id = "initData";
    public ServerGameData gameData;
    public ServerFeatures features;
    public ServerUIData uiData;
    public ServerPlayer player;
    public JackpotData jackpotData;
}

[Serializable]
public class JackpotData
{
    public JackpotValues values;//jackpotfeature

}

[Serializable]
public class JackpotValues
{
    public string miniJackpot;
    public string minorJackpot;
    public string majorJackpot;
    public string grandJackpot;
}

[Serializable]
public class JackpotSyncData
{
    public string gameId;
    public JackpotValues values;
}

[Serializable]
public class ServerGameData
{
    public List<List<int>> lines;
    public List<double> bets;
    public double creditDivisor = 1;
    public int totalLines = 1;
}

[Serializable]
public class ServerFeatures
{
    public double baseCoinValue = 1;
    public DualWheelsFeature dualWheels;
    public AnyPayoutsData anyPayouts;

    // Backward compatibility fields
    public USpinFeature uSpin;
    public MoneyBagFeature moneyBag;
    public FreeGamesFeature freeGames;
    public int betMultiplier;
    public int maxWinMultiplier;
    public int minWinMultiplier;
}

[Serializable]
public class DualWheelsFeature
{
    public bool enabled;
    public List<double> greenWheelValues;
    public List<double> redWheelValues;
}

[Serializable]
public class AnyPayoutsData
{
    public double any7;
    public double anyBar;
    public double wilds2x;
    public double wilds3x;
    public double anyRed3X;
    public double anyWilds;
    public double anyBlue2X;
}

[Serializable]
public class USpinFeature
{
    public bool enabled;
    public int minTrigger;
    public int symbolId;
    public List<USpinSegment> segments;
}

[Serializable]
public class USpinSegment
{
    public int sliceIndex;
    public string type;
    public double multiplier;
    public int freeGames;
}

[Serializable]
public class MoneyBagFeature
{
    public bool enabled;
    public int minTrigger;
    public int symbolId;
    public int bagCount;
}

[Serializable]
public class FreeGamesFeature
{
    public bool enabled;
    public double payMultiplier;
    public int maxTotalFreeGames;
}

[Serializable]
public class ExtraSpinsData
{
    [JsonProperty("2")] public int _2; // Keep for safety/compatibility with UI
    [JsonProperty("3")] public int _3;
    [JsonProperty("4")] public int _4;
    [JsonProperty("5")] public int _5;
}

[Serializable]
public class ServerUIData
{
    public PaylineData paylines;
}

[Serializable]
public class PaylineData
{
    public List<ServerSymbolInfo> symbols;
}

[Serializable]
public class ServerSymbolInfo
{
    public int id;
    public string name;
    public string group;
    public List<double> multiplier; // Keep for fallback compatibility
    public double payout;
    public string description;
    public int minMatch;
}

[Serializable]
public class ServerPlayer
{
    public double balance;
}

// ============================================================================
// Server Response Models - Match actual server JSON structure
// ============================================================================

[Serializable]
public class ServerSpinResponse
{
    public string id = "spinResult";
    public bool success;
    public List<List<string>> matrix; // Root level matrix sent by server (3x3)
    public ServerPlayerBalance player;
    public ServerPayload payload;
}

[Serializable]
public class ServerPlayerBalance
{
    public double? balance; // Nullable because server sends null
}

[Serializable]
public class ServerPayload
{
    public List<List<string>> reels;        // Keep for fallback compatibility
    public double totalWin;                  // Keep for fallback compatibility
    public double winAmount;
    public double grandTotalWin;
    public List<ServerWinLine> winningLines;
    public ServerDualWheelsBonus dualWheelsBonus;

    // Existing payload fields
    public int scatterCount;
    public bool scatterTriggered;
    public bool isRoundOver;
    public double totalRoundWin;
    public double netReturnRatio;
    public List<ServerWaysWin> waysWins;
    public ServerUSpinResult uSpin;
    public ServerMoneyBagResult moneyBag;
    public ServerFreeGamesResult freeGames;
}

[Serializable]
public class ServerWinLine
{
    public int lineId = -1;
    public int lineIndex = -1;
    public int symbolId;
    public string symbolName;
    public object positions;
    public List<ServerPosition> matchedPositions;
    public double payout;
    public double winAmount;
    public double multiplier;
    public double wildMultiplier;
}

[Serializable]
public class ServerDualWheelsBonus
{
    public bool isTriggered;
    public double totalWinAmount;
    public double greenWheelValue;
    public double redWheelValue;
    public string wheelType;
}

[Serializable]
public class ServerWaysWin
{
    public int symbolId;
    public int matchCount;
    public int waysCount;
    public List<ServerPosition> matchedPositions;
    public double basePayout;
    public double appliedMultiplier;
    public double winInCredits;
    public double winInCash;
    public string winType;
}

[Serializable]
public class ServerPosition
{
    public int row;
    public int col;
}

[Serializable]
public class ServerUSpinResult
{
    public bool triggered;
    public ServerUSpinResultDetail result;
}

[Serializable]
public class ServerUSpinResultDetail
{
    public int sliceIndex;
    public string type;
    public double multiplierAwarded;
    public int freeGamesAwarded;
    public double winInCash;
}

[Serializable]
public class ServerMoneyBagResult
{
    public bool triggered;
    public ServerMoneyBagResultDetail result;
}

[Serializable]
public class ServerMoneyBagResultDetail
{
    public int pickedIndex;
    public List<int> revealed;
    public int creditsAwarded;
    public double winInCash;
}

[Serializable]
public class ServerFreeGamesResult
{
    public bool triggered;
    public int totalAwarded;
    public int played;
    public double totalFreeGamesWin;
}

// ============================================================================
// Client-Side Spin Request
// ============================================================================

[Serializable]
public class SpinRequest
{
    public string type = "SPIN";
    public SpinPayload payload;
}

[Serializable]
public class SpinPayload
{
    public int betIndex;
    public bool isFreeSpin;
}

#endregion

#region Game Configuration (Client Side Converted)

[Serializable]
public class GameConfig
{
    public int reelCount = 3;
    public int rowCount = 3;
    public int symbolCount = 14;
    public int paylineCount = 1;
    public List<List<int>> paylines;
    public List<double> availableBets;
    public List<SymbolInfo> symbols;

    // Wild configuration
    public int wildSymbolId = 1;      // Red3X (1) or Blue2X (2)

    // Scatter / Feature configuration
    public int scatterSymbolId = 11;   // Spin ID 11

    public double baseCoinValue = 1.0;
    public int betMultiplier = 1;
    public double creditDivisor = 1.0;
    public int maxWinMultiplier = 10000;
    public int minWinMultiplier = 10;
    public int initialFreeSpins = 12;
    public ExtraSpinsData extraSpinsData;

    // Features
    public DualWheelsFeature dualWheels;
    public AnyPayoutsData anyPayouts;
    public List<USpinSegment> uSpinSegments;
}

[Serializable]
public class SymbolInfo
{
    public int id;
    public string name;
    public string group;
    public List<double> multipliers;
    public bool isWild;
    public bool isScatter;
    public int wildMultiplier = 1;
    public int minMatch;
}

#endregion

#region Player & Game State (Client Side)

[Serializable]
public class PlayerData
{
    public double balance;
    public int currentBetIndex;
}

[Serializable]
public class SpinResult
{
    public List<List<int>> resultMatrix;  // Client uses int matrix (3 reels x 3 rows)
    public double winAmount;
    public double grandTotalWin;
    public List<WinLine> winLines;
    public PlayerData playerData;
    public FreeSpinData freeSpinData;
    public ScatterData scatterData;
    public OverlayScatterData overlayScatterData;
    public Dictionary<string, int> stickyWilds;

    // Server-authoritative free spin state
    public int serverSpinsRemaining;
    public int serverSpinsUsed;
    public int serverTotalSpins;
    public double serverTotalRoundWin;
    public bool isRoundOver;
    
    // Server-authoritative bonus feature data
    public DualWheelsBonusData dualWheelsBonusData;
    public USpinResultData uSpinData;
    public MoneyBagResultData moneyBagData;

    public double GetDualWheelsWin()
    {
        return (dualWheelsBonusData != null && dualWheelsBonusData.isTriggered) ? dualWheelsBonusData.totalWinAmount : 0;
    }

    public double GetMoneyBagWin()
    {
        return (moneyBagData != null && moneyBagData.triggered) ? moneyBagData.winInCash : 0;
    }

    public double GetUSpinCashWin()
    {
        return (uSpinData != null && uSpinData.triggered && uSpinData.type == "MULTIPLIER") ? uSpinData.winInCash : 0;
    }

    public double GetTotalFeatureDeferredWins()
    {
        return GetDualWheelsWin() + GetMoneyBagWin() + GetUSpinCashWin();
    }
}

[Serializable]
public class DualWheelsBonusData
{
    public bool isTriggered;
    public double totalWinAmount;
    public double greenWheelValue;
    public double redWheelValue;
    public string wheelType;
}

[Serializable]
public class WinLine
{
    public int lineId;
    public int symbolId;
    public List<int> positions;  // Flat list: [row * 3 + col]
    public double winAmount;
}

[Serializable]
public class FreeSpinData
{
    public bool isTriggered;
    public int spinsAwarded;
    public int remainingSpins;
    public bool isBought;
}

[Serializable]
public class ScatterData
{
    public bool isTriggered;
    public int scatterCount;
    public double winAmount;
}

[Serializable]
public class OverlayScatterData
{
    public bool isTriggered;
    public int count;
    public int extraSpins;
    public List<List<int>> positions;
}

[Serializable]
public class USpinResultData
{
    public bool triggered;
    public int sliceIndex;
    public string type;
    public double multiplierAwarded;
    public int freeGamesAwarded;
    public double winInCash;
}

[Serializable]
public class MoneyBagResultData
{
    public bool triggered;
    public int pickedIndex;
    public List<int> revealed;
    public int creditsAwarded;
    public double winInCash;
}

#endregion

#region Platform Communication

[Serializable]
public class AuthData
{
    public string token;
    public string socketURL;
    public string nameSpace;
}

#endregion

#region Enums

public enum GameState
{
    Initializing,
    Idle,
    Spinning,
    Stopping,
    ShowingWin,
    FreeSpinMode
}

public enum SpinSpeed
{
    Normal,
    Turbo,
    QuickSpin
}

public enum WinPopupType
{
    RegularWin,         // Normal credit win
    BigWin,             // Big win
    FreeSpinTrigger,    // Free spins awarded
    MoneyBagCollect,    // Feature collect
    FreeSpinComplete    // All free spins completed
}

#endregion

#region Helper Classes for Conversion

/// <summary>
/// Converts server data to client GameConfig
/// </summary>
public static class InitDataConverter
{
    internal static GameConfig ConvertToGameConfig(InitData serverData)
    {
        var config = new GameConfig
        {
            reelCount = 3,
            rowCount = 3,
            symbolCount = (serverData?.uiData?.paylines?.symbols != null) ? serverData.uiData.paylines.symbols.Count : 14,
            paylineCount = (serverData?.gameData != null) ? serverData.gameData.totalLines : 1,
            paylines = serverData?.gameData?.lines,
            availableBets = serverData?.gameData?.bets,
            creditDivisor = (serverData?.features != null && serverData.features.baseCoinValue > 0) ? serverData.features.baseCoinValue : 1.0,
            baseCoinValue = (serverData?.features != null && serverData.features.baseCoinValue > 0) ? serverData.features.baseCoinValue : 1.0,
            symbols = new List<SymbolInfo>()
        };

        if (serverData?.uiData?.paylines?.symbols != null)
        {
            foreach (var serverSymbol in serverData.uiData.paylines.symbols)
            {
                var symbolInfo = new SymbolInfo
                {
                    id = serverSymbol.id,
                    name = serverSymbol.name,
                    group = serverSymbol.group,
                    multipliers = new List<double>(),
                    isWild = (serverSymbol.group != null && serverSymbol.group.Contains("wild")) || serverSymbol.id == 1 || serverSymbol.id == 2,
                    isScatter = (serverSymbol.name != null && (serverSymbol.name.ToLower().Contains("wheel") || serverSymbol.name.ToLower().Contains("spin"))) || serverSymbol.id >= 11,
                    minMatch = serverSymbol.minMatch > 0 ? serverSymbol.minMatch : 3
                };

                symbolInfo.multipliers.Add(serverSymbol.payout);
                config.symbols.Add(symbolInfo);

                if (symbolInfo.isWild && config.wildSymbolId <= 0)
                {
                    config.wildSymbolId = symbolInfo.id;
                }
                if (symbolInfo.isScatter && config.scatterSymbolId <= 0)
                {
                    config.scatterSymbolId = symbolInfo.id;
                }
            }
        }

        if (serverData?.features != null)
        {
            config.dualWheels = serverData.features.dualWheels;
            config.anyPayouts = serverData.features.anyPayouts;
            config.betMultiplier = serverData.features.betMultiplier > 0 ? serverData.features.betMultiplier : 1;
            config.maxWinMultiplier = serverData.features.maxWinMultiplier;
            config.minWinMultiplier = serverData.features.minWinMultiplier;

            if (serverData.features.freeGames != null)
            {
                config.initialFreeSpins = serverData.features.freeGames.maxTotalFreeGames;
            }

            if (serverData.features.uSpin != null && serverData.features.uSpin.segments != null)
            {
                config.uSpinSegments = serverData.features.uSpin.segments;
            }
        }

        return config;
    }

    internal static PlayerData ConvertToPlayerData(ServerPlayer serverPlayer, int defaultBetIndex = 0)
    {
        return new PlayerData
        {
            balance = serverPlayer != null ? serverPlayer.balance : 0,
            currentBetIndex = defaultBetIndex
        };
    }

    /// <summary>
    /// Converts server response to client SpinResult
    /// </summary>
    internal static SpinResult ConvertServerResponseToSpinResult(ServerSpinResponse serverResponse, double currentBalance, double betAmount, GameConfig gameConfig)
    {
        double winAmountVal = 0;
        if (serverResponse.payload != null)
        {
            winAmountVal = serverResponse.payload.winAmount > 0 ? serverResponse.payload.winAmount : serverResponse.payload.totalWin;
        }

        double totalPay = (gameConfig != null && gameConfig.creditDivisor > 0) ? betAmount * gameConfig.creditDivisor : betAmount;
        double newBalance = serverResponse.player?.balance ?? CalculateNewBalance(currentBalance, totalPay, winAmountVal);

        int spinsRemaining = 0;
        int spinsUsed = 0;
        int totalSpins = 0;
        double totalRoundWin = 0;
        bool isRoundOver = false;

        if (serverResponse.payload?.freeGames != null)
        {
            spinsRemaining = serverResponse.payload.freeGames.totalAwarded - serverResponse.payload.freeGames.played;
            spinsUsed = serverResponse.payload.freeGames.played;
            totalSpins = serverResponse.payload.freeGames.totalAwarded;
            totalRoundWin = serverResponse.payload.freeGames.totalFreeGamesWin;
            isRoundOver = serverResponse.payload.freeGames.played >= serverResponse.payload.freeGames.totalAwarded && serverResponse.payload.freeGames.totalAwarded > 0;
        }
        else if (serverResponse.payload != null)
        {
            isRoundOver = serverResponse.payload.isRoundOver;
            totalRoundWin = serverResponse.payload.totalRoundWin;
        }

        double featureWins = 0;
        if (serverResponse.payload != null)
        {
            if (serverResponse.payload.dualWheelsBonus != null && serverResponse.payload.dualWheelsBonus.isTriggered)
            {
                featureWins += serverResponse.payload.dualWheelsBonus.totalWinAmount;
            }
            if (serverResponse.payload.moneyBag?.result != null)
            {
                featureWins += serverResponse.payload.moneyBag.result.winInCash;
            }
            if (serverResponse.payload.uSpin?.result != null)
            {
                featureWins += serverResponse.payload.uSpin.result.winInCash;
            }
        }

        double grandTotalWinVal = (serverResponse.payload != null && serverResponse.payload.grandTotalWin > 0)
            ? serverResponse.payload.grandTotalWin 
            : (winAmountVal + featureWins);

        var result = new SpinResult
        {
            resultMatrix = ConvertReelsToMatrix(serverResponse.payload?.reels, serverResponse.matrix, serverResponse.payload?.waysWins, gameConfig),
            winAmount = winAmountVal,
            grandTotalWin = grandTotalWinVal,
            winLines = ConvertWinningLines(serverResponse.payload?.winningLines, serverResponse.payload?.waysWins, gameConfig),

            playerData = new PlayerData
            {
                balance = newBalance,
                currentBetIndex = 0
            },

            freeSpinData = (serverResponse.payload?.freeGames != null && serverResponse.payload.freeGames.triggered)
                ? new FreeSpinData
                {
                    isTriggered = true,
                    spinsAwarded = serverResponse.payload.freeGames.totalAwarded,
                    remainingSpins = serverResponse.payload.freeGames.totalAwarded - serverResponse.payload.freeGames.played,
                    isBought = false
                }
                : null,

            scatterData = (serverResponse.payload != null && serverResponse.payload.scatterTriggered)
                ? new ScatterData
                {
                    isTriggered = true,
                    scatterCount = serverResponse.payload.scatterCount,
                    winAmount = 0
                }
                : null,

            overlayScatterData = null,
            stickyWilds = null,

            serverSpinsRemaining = spinsRemaining,
            serverSpinsUsed = spinsUsed,
            serverTotalSpins = totalSpins,
            serverTotalRoundWin = totalRoundWin,
            isRoundOver = isRoundOver,
            
            dualWheelsBonusData = (serverResponse.payload?.dualWheelsBonus != null && serverResponse.payload.dualWheelsBonus.isTriggered)
                ? new DualWheelsBonusData
                {
                    isTriggered = true,
                    totalWinAmount = serverResponse.payload.dualWheelsBonus.totalWinAmount,
                    greenWheelValue = serverResponse.payload.dualWheelsBonus.greenWheelValue,
                    redWheelValue = serverResponse.payload.dualWheelsBonus.redWheelValue,
                    wheelType = serverResponse.payload.dualWheelsBonus.wheelType
                }
                : null,

            uSpinData = (serverResponse.payload?.uSpin != null && serverResponse.payload.uSpin.triggered && serverResponse.payload.uSpin.result != null)
                ? new USpinResultData
                {
                    triggered = true,
                    sliceIndex = serverResponse.payload.uSpin.result.sliceIndex,
                    type = serverResponse.payload.uSpin.result.type,
                    multiplierAwarded = serverResponse.payload.uSpin.result.multiplierAwarded,
                    freeGamesAwarded = serverResponse.payload.uSpin.result.freeGamesAwarded,
                    winInCash = serverResponse.payload.uSpin.result.winInCash
                }
                : null,
                
            moneyBagData = (serverResponse.payload?.moneyBag != null && serverResponse.payload.moneyBag.triggered && serverResponse.payload.moneyBag.result != null)
                ? new MoneyBagResultData
                {
                    triggered = true,
                    pickedIndex = serverResponse.payload.moneyBag.result.pickedIndex,
                    revealed = serverResponse.payload.moneyBag.result.revealed,
                    creditsAwarded = serverResponse.payload.moneyBag.result.creditsAwarded,
                    winInCash = serverResponse.payload.moneyBag.result.winInCash
                }
                : null
        };

        return result;
    }

    private static List<List<int>> ConvertReelsToMatrix(List<List<string>> serverReels, List<List<string>> serverMatrix, List<ServerWaysWin> waysWins, GameConfig gameConfig)
    {
        var sourceReels = serverMatrix ?? serverReels;
        int rowCount = gameConfig != null ? gameConfig.rowCount : 3;
        int reelCount = gameConfig != null ? gameConfig.reelCount : 3;

        if (sourceReels == null || sourceReels.Count == 0)
        {
            UnityEngine.Debug.LogError("Invalid server reels/matrix: sourceReels is null or empty");
            return GenerateDefaultMatrix(rowCount, reelCount);
        }

        int totalRows = sourceReels.Count;
        int totalCols = sourceReels[0].Count;

        var matrix = new List<List<int>>();

        for (int col = 0; col < totalCols; col++)
        {
            var column = new List<int>();
            for (int row = 0; row < totalRows; row++)
            {
                if (col < sourceReels[row].Count)
                {
                    string symbolStr = sourceReels[row][col];
                    if (int.TryParse(symbolStr, out int symbolId))
                    {
                        column.Add(symbolId);
                    }
                    else
                    {
                        column.Add(0);
                    }
                }
                else
                {
                    column.Add(0);
                }
            }
            matrix.Add(column);
        }

        return matrix;
    }

    private static List<List<int>> GenerateDefaultMatrix(int rowCount = 3, int reelCount = 3)
    {
        var matrix = new List<List<int>>();
        for (int col = 0; col < reelCount; col++)
        {
            var column = new List<int>();
            for (int row = 0; row < rowCount; row++)
            {
                column.Add(0);
            }
            matrix.Add(column);
        }
        return matrix;
    }

    private static List<WinLine> ConvertWinningLines(List<ServerWinLine> serverWinLines, List<ServerWaysWin> serverWaysWins, GameConfig gameConfig)
    {
        var winLines = new List<WinLine>();
        int reelCount = gameConfig != null ? gameConfig.reelCount : 3;

        if (serverWinLines != null && serverWinLines.Count > 0)
        {
            int index = 0;
            foreach (var line in serverWinLines)
            {
                var flatPositions = new List<int>();
                if (line.matchedPositions != null && line.matchedPositions.Count > 0)
                {
                    foreach (var pos in line.matchedPositions)
                    {
                        int flatIndex = pos.row * reelCount + pos.col;
                        flatPositions.Add(flatIndex);
                    }
                }
                else if (line.positions != null)
                {
                    if (line.positions is Newtonsoft.Json.Linq.JArray jArr)
                    {
                        foreach (var item in jArr)
                        {
                            if (item is Newtonsoft.Json.Linq.JArray subArr && subArr.Count >= 2)
                            {
                                int row = (int)subArr[0];
                                int col = (int)subArr[1];
                                flatPositions.Add(row * reelCount + col);
                            }
                            else if (item is Newtonsoft.Json.Linq.JArray subArr1 && subArr1.Count == 1)
                            {
                                flatPositions.Add((int)subArr1[0]);
                            }
                            else if (int.TryParse(item.ToString(), out int pVal))
                            {
                                flatPositions.Add(pVal);
                            }
                        }
                    }
                    else if (line.positions is List<List<int>> list2D)
                    {
                        foreach (var pair in list2D)
                        {
                            if (pair.Count >= 2)
                            {
                                flatPositions.Add(pair[0] * reelCount + pair[1]);
                            }
                            else if (pair.Count == 1)
                            {
                                flatPositions.Add(pair[0]);
                            }
                        }
                    }
                    else if (line.positions is List<int> list1D)
                    {
                        flatPositions.AddRange(list1D);
                    }
                }

                int effectiveLineId = line.lineIndex >= 0 ? line.lineIndex : (line.lineId >= 0 ? line.lineId : index++);
                double effectiveWinAmount = line.payout > 0 ? line.payout : line.winAmount;

                winLines.Add(new WinLine
                {
                    lineId = effectiveLineId,
                    symbolId = line.symbolId,
                    positions = flatPositions,
                    winAmount = effectiveWinAmount
                });
            }
            return winLines;
        }

        if (serverWaysWins != null && serverWaysWins.Count > 0)
        {
            int index = 0;
            foreach (var waysWin in serverWaysWins)
            {
                var flatPositions = new List<int>();
                if (waysWin.matchedPositions != null)
                {
                    foreach (var pos in waysWin.matchedPositions)
                    {
                        int flatIndex = pos.row * reelCount + pos.col;
                        flatPositions.Add(flatIndex);
                    }
                }

                winLines.Add(new WinLine
                {
                    lineId = index++,
                    symbolId = waysWin.symbolId,
                    positions = flatPositions,
                    winAmount = waysWin.winInCash
                });
            }
        }

        return winLines;
    }

    private static double CalculateNewBalance(double currentBalance, double totalPay, double winAmount)
    {
        return currentBalance + winAmount;
    }
}

#endregion