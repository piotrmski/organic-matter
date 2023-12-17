using Godot;
using Organicmatter.Scripts.Internal.Model;
using Organicmatter.Scripts.Internal.SimulationStrategy;
using System;
using System.Collections.Generic;

namespace Organicmatter.Scripts.Internal
{
    internal class Simulation
    {
        public SimulationState SimulationState { get; private set; }

        private List<ISimulationStrategy> _strategies;

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

        public void Advance()
        {
            _strategies.ForEach(strategy => strategy.Advance());
        }

        private void PrepareInitialState()
        {
            SimulationState.ForEachCell((ref CellData cellData, int x, int y) =>
            {
                if (y < 30)
                {
                    cellData.Type = CellType.Soil;
                    cellData.MineralContent = SimulationState.Parameters.MineralsCriticalSoilDistribution;
                }
                else
                {
                    cellData.Type = CellType.Air;
                }
            });

            int x = SimulationState.CellMatrix.GetLength(0) / 2;

            SimulationState.CellMatrix[x, 29].Type = CellType.PlantRoot;
            SimulationState.CellMatrix[x, 30].Type = CellType.PlantGreen;

            SimulationState.AddCellConnections(x, 29, Direction.Top);
        }
    }
}
