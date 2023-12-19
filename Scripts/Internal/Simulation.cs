using Godot;
using Organicmatter.Scripts.Internal.Model;
using Organicmatter.Scripts.Internal.SimulationStrategy;
using System.Diagnostics;
using System.Collections.Generic;
using System;

namespace Organicmatter.Scripts.Internal
{
    internal class Simulation
    {
        public SimulationState SimulationState { get; private set; }

        public int Iteration { get; private set; } = 0;

        private List<ISimulationStrategy> _strategies;

        private Stopwatch _watch = new();

        public Simulation(int spaceWidth, int spaceHeight) 
        {
            SimulationState = new SimulationState(spaceWidth, spaceHeight);

            PrepareInitialState();

            _strategies = new List<ISimulationStrategy>()
            {
                new Gravity(SimulationState),
                new Diffusion(SimulationState),
                new PlantGrowth(SimulationState),
                new Lighting(SimulationState),
                new PlantMetabolism(SimulationState)
            };
        }

        public TimeSpan[] Advance()
        {
            TimeSpan[] executionTimes = new TimeSpan[_strategies.Count];
            int i = 0;

            _strategies.ForEach(strategy =>
            {
                _watch.Restart();
                strategy.Advance();
                _watch.Stop();

                executionTimes[i++] = _watch.Elapsed;
            });

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
                    cellData.NurtientContent = SimulationState.Parameters.NurtientsCriticalSoilDistribution;
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
