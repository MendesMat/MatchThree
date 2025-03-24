namespace MatchThree.Project.Scripts.GridSystems
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

        public T GetCellValue() => _gem;
        public void SetCellValue(T gem) => _gem = gem;
    }
}