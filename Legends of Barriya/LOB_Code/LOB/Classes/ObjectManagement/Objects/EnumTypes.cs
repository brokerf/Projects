namespace LOB.Classes.ObjectManagement.Objects
{
    internal enum ResourceType
    {
        Wood,
        Iron,
        Gold,
        Mana,
        Population,
        MaxPopulation,
        HeroAlive,
        MainBuildingAlive,
        KilledEnemies,
        LostUnits,
        None
    }

    // ORDER IS IMPORTANT
    // The objects above Builder are Objects that fight
    // The objects below NoneObject are resource points
    internal enum ObjectType
    {
        Knight,
        Archer,
        Horseman,
        Mage,
        Human1Hero,
        Orc1Hero,
        Dwarf1Hero,
        Axeman,
        Arbalist,
        Wolf1Rider,
        Phalanx,
        Puncher,
        Slingshot,
        Shaman,
        Troll,
        Builder, //below here are enums that don't fight
        Main1Building,
        Mine,
        House,
        Military1Camp,
        Wall,
        Tower,
        Gate,
        Mage1Tower,
        New1Building,
        Grass,
        BarrierLeft,
        BarrierMiddle,
        BarrierRight,
        Barrier,
        Rock,
        NoneObject,
        Tree,
        Iron1Source,
        Gold1Vein,
        Mana1Source
    }

    internal enum PotionType
    {
        Heal1Potion,
        Damage1Potion,
        Speed1Potion
    }

    internal enum ObjectState
    {
        Idle,
        Gathering,
        Building,
    }
}