namespace Organicmatter.Scripts.Internal.Model
{
    internal struct SimulationParameters
    {
        public int NutrientsCriticalSoilDistribution = 50;

        public int DirectLightEnergy = 16;

        public int LightToConvertNutrientToEnergy = 1000;

        public int EnergyInPlantCellStructure = 2;

        public int EnergyToSynthesizeFruitCell = 20;

        public int AgeToSynthesizeFruitCell = 1000;

        public int EnergyInPlantSeed = 250;

        public int WasteToKillPlantCell = 50;

        public int PlantEnergyConsumptionPeriod = 500;

        public int SoilNutrientReclamationPeriod = 500;

        public SimulationParameters() { }
    }
}
