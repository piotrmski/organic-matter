namespace Organicmatter.Scripts.Internal.Model
{
    internal struct SimulationParameters
    {
        public int NutrientsCriticalSoilDistribution = 50;

        public int DirectLightEnergy = 16;

        public int LightToConvertNutrientToEnergy = 500;

        public int EnergyInPlantCellStructure = 5;

        public int EnergyToSynthesizeFruitCell = 10;

        public int AgeToSynthesizeFruitCell = 1000;

        public int EnergyInPlantSeed = 250;

        public int WasteToKillPlantCell = 50;

        public int WasteToKillFruitOrSeed = 10;

        public int PlantEnergyConsumptionPeriod = 1000;

        public int SoilNutrientReclamationPeriod = 1000;

        public SimulationParameters() { }
    }
}
