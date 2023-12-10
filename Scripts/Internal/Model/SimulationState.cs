using System;
using static Godot.HttpRequest;

namespace Organicmatter.Scripts.Internal.Model
{
    internal struct SimulationState
    {
        public CellData[,] CellMatrix;

        public bool[,] CellVerticalConnections;

        public bool[,] CellHorizontalConnections;

        public SimulationParameters Parameters = new();

        private int _spaceWidth;

        private int _spaceHeight;

        public SimulationState(int spaceWidth, int spaceHeight)
        {
            _spaceWidth = spaceWidth;
            _spaceHeight = spaceHeight;

            CellMatrix = new CellData[spaceWidth, spaceHeight];
            CellVerticalConnections = new bool[spaceWidth, spaceHeight - 1];
            CellHorizontalConnections = new bool[spaceWidth - 1, spaceHeight];
        }

        public void AddCellConnections(int x, int y, CellConnection connection)
        {
            if (connection.HasFlag(CellConnection.Left))
            {
                CellHorizontalConnections[x - 1, y] = true;
            }

            if (connection.HasFlag(CellConnection.Right))
            {
                CellHorizontalConnections[x, y] = true;
            }

            if (connection.HasFlag(CellConnection.Bottom))
            {
                CellVerticalConnections[x, y - 1] = true;
            }

            if (connection.HasFlag(CellConnection.Top))
            {
                CellVerticalConnections[x, y] = true;
            }
        }

        public void RemoveCellConnections(int x, int y)
        {
            if (x > 0) CellHorizontalConnections[x - 1, y] = false;
            if (x < _spaceWidth - 1) CellHorizontalConnections[x, y] = false;
            if (y > 0) CellVerticalConnections[x, y - 1] = false;
            if (y < _spaceHeight - 1) CellVerticalConnections[x, y] = false;
        }

        public CellConnection GetCellConnections(int x, int y)
        {
            CellConnection result = CellConnection.None;

            if (x > 0 && CellHorizontalConnections[x - 1, y]) result |= CellConnection.Left;
            if (x < _spaceWidth - 1 && CellHorizontalConnections[x, y]) result |= CellConnection.Right;
            if (y > 0 && CellVerticalConnections[x, y - 1]) result |= CellConnection.Bottom;
            if (y < _spaceHeight - 1 && CellVerticalConnections[x, y]) result |= CellConnection.Top;

            return result;
        }

        public void ForEachCell(ForEachCellAction action)
        {
            for (int x = 0; x < CellMatrix.GetLength(0); ++x)
            {
                for (int y = 0; y < CellMatrix.GetLength(1); ++y)
                {
                    action(ref CellMatrix[x, y], x, y);
                }
            }
        }

        public void ForEachCell(ForEachCellActionShort action)
        {
            ForEachCell((ref CellData cellData, int x, int y) => action(ref cellData));
        }

        public void ForEachCell(ForEachCellActionCoordinates action)
        {
            ForEachCell((ref CellData cellData, int x, int y) => action(x, y));
        }
    }

    delegate void ForEachCellAction(ref CellData cellData, int x, int y);

    delegate void ForEachCellActionShort(ref CellData cellData);

    delegate void ForEachCellActionCoordinates(int x, int y);
}
