using System.Collections;
using DG.Tweening;
using MatchThree.Project.Scripts.Core.EventBus;
using MatchThree.Project.Scripts.Core.EventBus.Events;
using MatchThree.Project.Scripts.Core.Input;
using MatchThree.Project.Scripts.Gems;
using UnityEngine;

namespace MatchThree.Project.Scripts.BoardSystems
{
    public class GridManager : MonoBehaviour
    {
        [Header("Grid Properties")]
        [SerializeField] private float cellSize;
        [SerializeField] private int gridWidth;
        [SerializeField] private int gridHeight;
        [SerializeField] private Vector3 origin = Vector3.zero;
        
        [Header("Gem Properties")]
        [SerializeField] private Gem gemPrefab;
        [SerializeField] private GemSO[] gemTypes;
        
        [Header("Animation Properties")]
        [SerializeField] private Ease ease = Ease.InQuad;
        
        [Header("Debug Properties")]
        [SerializeField] private bool displayDebug;
        
        private GridSystem<GridCell<Gem>> _grid;
        private EventBinding<FireInputEvent> _eventBinding;
        private Vector2Int _selectedGem;

        private void Awake() => InitializeGrid();

        private void OnEnable()
        {
            _eventBinding = new EventBinding<FireInputEvent>(OnSelectGem);
            EventBus<FireInputEvent>.Register(_eventBinding);
        }

        private void OnDisable() => EventBus<FireInputEvent>.Unregister(_eventBinding);

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
                    CreateGridCell(x, y);
                }
            }
        }
        
        private void CreateGridCell(int x, int y)
        {
            var gem = CreateGem(x, y);
            
            var gridCell = new GridCell<Gem>(_grid, x, y);
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
        
        private void OnSelectGem(FireInputEvent obj)
        {
            if (Camera.main == null) return;
            var mousePosition = Camera.main.ScreenToWorldPoint(InputReader.Selected);
            var gridPosition = _grid.GetGridPosition(mousePosition);

            if (_selectedGem == gridPosition) DeselectGem();
            else if (_selectedGem == Vector2Int.one * -1) SelectGem(gridPosition);
            else StartCoroutine(RunGameLoop(_selectedGem, gridPosition));
        }

        private void SelectGem(Vector2Int gridPosition) => _selectedGem = gridPosition;
        private void DeselectGem() => _selectedGem = new Vector2Int(-1, -1);

        private IEnumerator SwapGems(Vector2Int gridPositionA, Vector2Int gridPositionB)
        {
            var gridCellA = _grid.GetValue(gridPositionA.x, gridPositionA.y);
            var gridCellB = _grid.GetValue(gridPositionB.x, gridPositionB.y);

            gridCellA.GetValue().transform
                .DOLocalMove(_grid.GetWorldPositionCenter(gridPositionB.x, gridPositionB.y), 0.5f)
                .SetEase(ease);

            gridCellB.GetValue().transform
                .DOLocalMove(_grid.GetWorldPositionCenter(gridPositionB.x, gridPositionB.y), 0.5f)
                .SetEase(ease);
           
            _grid.SetValue(gridPositionA.x, gridPositionA.y, gridCellB);
            _grid.SetValue(gridPositionB.x, gridPositionB.y, gridCellA);
            
            yield return new WaitForSeconds(0.5f);
        }
        
        private IEnumerator RunGameLoop(Vector2Int gridPositionA, Vector2Int gridPositionB)
        {
            yield return StartCoroutine(SwapGems(gridPositionA, gridPositionB));
            yield return null;
        }
    }
}