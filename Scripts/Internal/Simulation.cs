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

            PrepareInitialState();

            _steps = new ISimulationStep[]
            {
                new Gravity(SimulationState),
                new Diffusion(SimulationState),
                new PlantGrowth(SimulationState),
                new Lighting(SimulationState),
                new Metabolism(SimulationState)
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

        private void PrepareInitialState()
        {
            SimulationState.ForEachCell((ref CellData cellData, int x, int y) =>
            {
                if (y < 30)
                {
                    cellData.Type = CellType.Soil;
                    cellData.NutrientContent = SimulationState.Parameters.NutrientsCriticalSoilDistribution;
                }
                else
                {
                    cellData.Type = CellType.Air;
                }
            });

            int x1 = SimulationState.CellMatrix.GetLength(0) / 3;
            int x2 = x1 * 2;

            SimulationState.CellMatrix[x1, 29].Type = CellType.PlantRoot;
            SimulationState.CellMatrix[x1, 30].Type = CellType.PlantGreen;
            SimulationState.AddCellConnections(x1, 29, Direction.Up);

            SimulationState.CellMatrix[x2, 29].Type = CellType.PlantRoot;
            SimulationState.CellMatrix[x2, 30].Type = CellType.PlantGreen;
            SimulationState.AddCellConnections(x2, 29, Direction.Up);
        }
    }
}
