namespace HackedDesign
{
    public static class ItemStatKeys
    {
        public const string PhysicalShootDamageMin = "physicalShootDamageMin";
        public const string PhysicalShootDamageMax = "physicalMaxShootDamageMax";
        public const string PhysicalMeleeDamageMin = "physicalMeleeDamageMin";
        public const string PhysicalMeleeDamageMax = "physicalMeleeDamageMax";

        public const string BarrierRotationSpeed = "rotationSpeed";

        public const string BarrierFaces = "barrierFaces";
        // Seconds a broken edge stays open, then seconds it takes to refill. Fall back to the Barrier asset.
        public const string BarrierRegrowDelay = "barrierRegrowDelay";
        public const string BarrierRegrowTime = "barrierRegrowTime";
        public const string PhysicalDamageReduction = "physicalDamageReduction";
    }
}
