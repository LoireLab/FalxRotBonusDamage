namespace FalxRotBonusDamage
{
    public class FalxRotBonusDamageConfig
    {
        public float RustCreatureDamageMultiplier { get; set; } = 1.25f;

        public FalxRotBonusDamageConfig EnsureValid()
        {
            if (RustCreatureDamageMultiplier < 1f)
            {
                RustCreatureDamageMultiplier = 1f;
            }

            return this;
        }
    }
}
