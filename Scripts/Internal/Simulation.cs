using Organicmatter.Scripts.Internal.Helpers;
using Organicmatter.Scripts.Internal.Model;
using Organicmatter.Scripts.Internal.SimulationStep;
using System.Diagnostics;
using System.Linq;

namespace Organicmatter.Scripts.Internal
{
    internal class Simulation
    {
        public SimulationState SimulationState { get; private set; }

        public int Iteration { get; private set; } = 0;

        private ISimulationStep[] _steps;

        private string[] _stepNames;

        private Stopwatch _watch = new();

        public Simulation(int spaceWidth, int spaceHeight) 
        {
            SimulationState = new SimulationState(spaceWidth, spaceHeight);

            SpecialCoordinates specialCoordinates = new();

            PrepareInitialState(specialCoordinates);

            _steps = new ISimulationStep[]
            {
                new Gravity(SimulationState, specialCoordinates),
                new Diffusion(SimulationState),
                new PlantGrowth(SimulationState, specialCoordinates),
                new Lighting(SimulationState),
                new Metabolism(SimulationState, specialCoordinates),
                new FruitDevelopment(SimulationState, specialCoordinates),
                new Germination(SimulationState, specialCoordinates),
            };

            _stepNames = _steps.Select(x => x.GetType().Name).ToArray();
        }

        public SimulationStepExecutionTime[] Advance()
        {
            SimulationStepExecutionTime[] executionTimes = new SimulationStepExecutionTime[_steps.Length];

            for (int i = 0; i < _steps.Length; ++i)
            {
                _watch.Restart();
                _steps[i].Advance();
                _watch.Stop();

                executionTimes[i].StepName = _stepNames[i];
                executionTimes[i].ExecutionTime = _watch.Elapsed;
            }

            ++Iteration;

            return executionTimes;
        }

        private void PrepareInitialState(SpecialCoordinates specialCoordinates)
        {
            SimulationState.ForEachCell((ref CellData cellData, int x, int y) =>
            {
                if (y < 30)
                {
                    cellData.Type = CellType.Soil;
                    cellData.NutrientContent = SimulationState.Parameters.NutrientsInitialSoilDistribution;
                }
                else
                {
                    cellData.Type = CellType.Air;
                }
            });

            int x1 = SimulationState.CellMatrix.GetLength(0) / 3;
            int x2 = x1 * 2;
            int y = 50;

            SimulationState.CellMatrix[x1, y].Type = CellType.PlantSeed;
            SimulationState.CellMatrix[x1, y].EnergyContent = SimulationState.Parameters.EnergyInPlantSeed;

            SimulationState.CellMatrix[x2, y].Type = CellType.PlantSeed;
            SimulationState.CellMatrix[x2, y].EnergyContent = SimulationState.Parameters.EnergyInPlantSeed;

            specialCoordinates.Seeds.Add(new(x1, y));
            specialCoordinates.Seeds.Add(new(x2, y));
        }
    }
}
