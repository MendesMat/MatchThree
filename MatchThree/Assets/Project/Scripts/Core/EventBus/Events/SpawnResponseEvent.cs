using MatchThree.Project.Scripts.Gems;
using MatchThree.Project.Scripts.GridSystems;
using MatchThree.Project.Scripts.GridSystems.GridCell;

namespace MatchThree.Project.Scripts.Core.EventBus.Events
{
    public class SpawnResponseEvent<T> : IEvent
    {
        public GridSystem<GridCell<Gem>> Grid { get; }
        public T Value { get; }
        public int X { get; }
        public int Y { get; }

        public SpawnResponseEvent(GridSystem<GridCell<Gem>> grid, T value, int x, int y)
        {
            Grid = grid;
            Value = value;
            X = x;
            Y = y;
        }
    }
}