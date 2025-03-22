using MatchThree.Project.Scripts.Gems;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace MatchThree.Project.Scripts.BoardSystems
{
    public class GridManager : MonoBehaviour
    {
        [SerializeField] private bool displayDebug;
        
        [Header("Grid Properties")]
        [SerializeField] private float cellSize;
        [SerializeField] private int gridWidth;
        [SerializeField] private int gridHeight;
        [SerializeField] private Vector3 origin = Vector3.zero;

        [SerializeField] private Gem gemPrefab;
        [SerializeField] private GemSO[] gemTypes;
        
        private GridSystem<GridCell<Gem>> _grid;

        private void Awake() => InitializeGrid();

        private void InitializeGrid()
        {
            _grid = new GridSystem<GridCell<Gem>>(cellSize, gridWidth, gridHeight, origin, displayDebug);
            
            if (_grid == null)
            {
                Debug.Log("Falha ao criar o grid");
                return;
            }
            
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    CreateGridCell(x, y, _grid);
                }
            }
        }
        
        private void CreateGridCell(int x, int y, GridSystem<GridCell<Gem>> grid)
        {
            var gem = CreateGem(x, y);
            
            var gridCell = new GridCell<Gem>(grid, x, y);
            if(gem != null) gridCell.SetValue(gem);
            
            _grid.SetValue(x, y, gridCell);
        }

        private Gem CreateGem(int x, int y)
        {
            if (gemPrefab == null)
            {
                Debug.Log("Prefab de Gem não foi atribuído");
                return null;
            }
            
            var gem = Instantiate(gemPrefab, _grid.GetWorldPositionCenter(x, y), Quaternion.identity, transform);
            gem.SetGemType(gemTypes[Random.Range(0, gemTypes.Length)]);

            return gem;
        }
    }
}