static public class Layers
{    
    public const int Default = 0;
    public const int DefaultMask = 1 << 0;
    
    public const int TransparentFX = 1;
    public const int TransparentFXMask = 1 << 1;
    
    public const int Ignore_Raycast = 2;
    public const int Ignore_RaycastMask = 1 << 2;
    
    public const int Water = 4;
    public const int WaterMask = 1 << 4;
    
    public const int UI = 5;
    public const int UIMask = 1 << 5;
    
    public const int Terrain = 6;
    public const int TerrainMask = 1 << 6;
    
    public const int Enemy = 7;
    public const int EnemyMask = 1 << 7;
    
    public const int Player = 8;
    public const int PlayerMask = 1 << 8;
    
    public const int EnemyAttack = 9;
    public const int EnemyAttackMask = 1 << 9;
    
    public const int PlayerAttack = 10;
    public const int PlayerAttackMask = 1 << 10;
    
    public const int XP = 11;
    public const int XPMask = 1 << 11;
}
