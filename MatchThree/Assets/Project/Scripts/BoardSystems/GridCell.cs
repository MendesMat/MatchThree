namespace MatchThree.Project.Scripts.BoardSystems
{
    public class GridCell<T>
    {
        private GridSystem<GridCell<T>> _gridSystem;
        private int _x;
        private int _y;
        private T _gem;

        public GridCell(GridSystem<GridCell<T>> gridSystem, int x, int y)
        {
            _gridSystem = gridSystem;
            _x = x;
            _y = y;
        }

        public T GetValue() => _gem;
        public void SetValue(T gem) => _gem = gem;
    }
}