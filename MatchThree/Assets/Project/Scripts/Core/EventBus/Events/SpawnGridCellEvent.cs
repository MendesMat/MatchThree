using MatchThree.Project.Scripts.Gems;
using MatchThree.Project.Scripts.GridSystems;
using MatchThree.Project.Scripts.GridSystems.GridCell;
using UnityEngine;

namespace MatchThree.Project.Scripts.Core.EventBus.Events
{
    public class SpawnGridCellEvent : IEvent
    {
        public GridSystem<GridCell<Gem>> Grid { get; }
        public int X { get; }
        public int Y { get; }
        
        public SpawnGridCellEvent(GridSystem<GridCell<Gem>> grid, int x, int y)
        {
            Grid = grid;
            X = x;
            Y = y;
        }
    }
}