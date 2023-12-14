﻿namespace Organicmatter.Scripts.Internal.Model
{
    internal struct SimulationParameters
    {
        public int WaterMoleculesStartingDistribution = 100;

        public int CarbonDioxydeStartingAmount = 100000;

        public int OxygenStartingAmount = 100000;

        public int DirectLightEnergy = 16;

        public int EnergyInGlucose = 100;

        public int GlucoseInCellulose = 10; // Must be even

        public int WaterRequiredToSynthesizeGreen = 50;

        public int EnergyRequiredToSynthesizeRoot = 500;

        public int EnergyToDecomposeCellulose = 500;

        public int PlantEnergyConsumptionPerTick = 1;

        public int BacteriaEnergyConsumptionPerTick = 1;

        public SimulationParameters() { }
    }
}
