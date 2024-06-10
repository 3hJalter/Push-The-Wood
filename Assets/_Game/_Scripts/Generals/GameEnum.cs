namespace MapEnum
{
    public enum GridPlane
    {
        XY = 0,
        XZ = 1,
        YZ = 2
    }
}

public enum Direction
{
    None = -1,
    Left = 0,
    Right = 1,
    Forward = 2,
    Back = 3
}
public enum InputAction
{
    None = -1,
    ButtonDown = 0,
    ButtonUp = 1,
    ButtonHold = 2,
}

public enum CONDITION
{
    NONE = -1,
    BE_BLOCKED_BY_TREE_ROOT = 0,
    ROLL_AROUND_BLOCK_CHUMP = 1,
    SIT_DOWN = 2,
    PEEK = 3,
    RUN_ABOVE_CHUMP = 4,
}

public enum UI_POSITION
{
    NONE = -1,
    MAIN_MENU = 0,
    IN_GAME = 1,
}
namespace TweenTypeEnum
{
    public enum EasingType
    {
        Linear = 1,
        InSine,
        OutSine,
        InOutSine,
        InQuad,
        OutQuad,
        InOutQuad,
        InCubic,
        OutCubic,
        InOutCubic,
        InQuart,
        OutQuart,
        InOutQuart,
        InQuint,
        OutQuint,
        InOutQuint,
        InExpo,
        OutExpo,
        InOutExpo,
        InCirc,
        OutCirc,
        InOutCirc,
        InElastic,
        OutElastic,
        InOutElastic,
        InBack,
        OutBack,
        InOutBack,
        InBounce,
        OutBounce,
        InOutBounce,
        Flash,
        InFlash,
        OutFlash,
        InOutFlash,
        Custom
    }
}

namespace GameGridEnum
{
    public enum HeightLevel
    {
        None = -1,
        Zero = 0,
        ZeroPointFive = 1,
        One = 2,
        OnePointFive = 3,
        Two = 4,
        TwoPointFive = 5,
        Three = 6,
        ThreePointFive = 7,
        Four = 8,
        FourPointFive = 9
    }

    public enum GridSurfaceType
    {
        // Default value of all GameGridCell is Water, so no need to create a WaterGridSurface
        // Other GridSurfaceType must be created when needed (e.g. GroundGridSurface) to overwrite the default value
        Water = 0,
        Ground = 1
    }

    public enum GridUnitStaticType
    {
        None = -1,
        RockShort = 0,
        RockHigh = 1,
        TreeRoot = 2,
        TreeShort = 3,
        TreeHigh = 4,
        FinalPointFruit = 5,
        BonusChest = 6, 
        ChestButtonUnit = 7,
        RockUnderwaterSmall = 8,
        RockUnderWaterBig = 9,
        TreeBeeShort = 10,
        TreeBeeHigh = 11,
        TreeSeed = 12,
        TreeShortAfterBee = 13,
        FinalPointChickenBbq = 16,
        ChestCompassGround = 17,
        ChestCompassWater = 18,
    }

    public enum GridUnitDynamicType
    {
        None = -1,
        Player = 0,
        ChumpShort = 1,
        ChumpHigh = 2,
        Raft = 3,
        RaftLong = 4,
        Box = 5,
        Bomb = 6,
        ButtonUnit = 7,
        Archer_Enemy = 100,
        Mage_Enemy = 101,
    }
    
    public enum VFXType
    {
        Dust = 0,
        WaterSplash = 1,
        LeafExplosion = 2,
        DangerIndicator = 3,
        BombExplosion = 4,
        SmokeExplosion = 5,
        MageSkill1Explosion = 6,
        SleepingzZz = 7,
        MusicalNotes = 8,
        WaterSplash_Continuous = 9,
        Confetti = 10,
        FlameWarning = 11,
    }
    
    public enum TutorialType
    {
        None = -1,
        Test = 0,
    }
}

namespace _Game.Resource
{
    public enum RewardType
    {
        None = -1,
        Booster = 0,
        Currency = 1,
        Character = 2,
    }

    public enum BoosterType
    {
        None = -1,
        Undo = 0,
        PushHint = 1,
        GrowTree = 2,
        ResetIsland = 3,
    }
    
    public enum CurrencyType
    {
        None = -1,
        Gold = 0,
        Heart = 1,
        SecretMapPiece = 2,
        RandomBooster = 3,
        RewardKey = 4,
        LevelStar = 5
    }

    public enum CharacterType
    {
        None = -1,
        Beeny = 0,
        Weeny = 1,
        Fuzzy = 2,
        Millie = 3,
        Gus = 4,
        Ollie = 5,
    }

    public enum Placement
    {
        None = -1,
    }
}

namespace _Game.Ads
{
    public enum Placement
    {
        None = -1,
        Booster_Popup = 0,
        In_Game = 1,
        Collection = 2,
        Win_Popup = 3,
    }
}