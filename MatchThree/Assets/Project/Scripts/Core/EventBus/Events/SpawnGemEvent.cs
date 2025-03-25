using MatchThree.Project.Scripts.Gems;
using MatchThree.Project.Scripts.GridSystems;
using MatchThree.Project.Scripts.GridSystems.GridCell;

namespace MatchThree.Project.Scripts.Core.EventBus.Events
{
    public class SpawnGemEvent : IEvent
    {
        public GridSystem<GridCell<Gem>> Grid { get; set; }
        public int X { get; }
        public int Y { get; }

        public SpawnGemEvent(GridSystem<GridCell<Gem>> grid, int x, int y)
        {
            Grid = grid;
            X = x;
            Y = y;
        }
    }
}