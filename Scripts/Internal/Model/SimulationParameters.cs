namespace Organicmatter.Scripts.Internal.Model
{
    internal struct SimulationParameters
    {
        public int WaterMoleculesStartingDistribution = 1000;

        public int DirectLightEnergy = 16;

        public int EnergyInSugar = 100;

        public int GlucoseInCellulose = 10; // Must be even

        public int EnergyToSynthesizeCellulose = 500;

        public int EnergyToDecomposeCellulose = 500;

        public int PlantEnergyConsumptionPerTick = 1;

        public int BacteriaEnergyConsumptionPerTick = 1;

        public SimulationParameters() { }
    }
}
