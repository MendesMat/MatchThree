using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MatchThree.Project.Scripts.Core.EventBus;
using MatchThree.Project.Scripts.Core.EventBus.Events;
using MatchThree.Project.Scripts.Core.Input;
using MatchThree.Project.Scripts.Gems;
using MatchThree.Project.Scripts.GridSystems.GridCell;
using UnityEngine;

namespace MatchThree.Project.Scripts.GridSystems
{
    public class GridManager : MonoBehaviour
    {
        [Header("Components")] 
        [SerializeField] private InputReader inputReader;
        
        [Header("Grid Properties")]
        [SerializeField] private float cellSize;
        [SerializeField] private int gridWidth;
        [SerializeField] private int gridHeight;
        [SerializeField] private Vector3 origin = Vector3.zero;
        
        // [Header("Gem Properties")]
        // [SerializeField] private Gem gemPrefab;
        // [SerializeField] private GemSO[] gemTypes;
        
        [Header("Animation Properties")]
        [SerializeField] private Ease ease = Ease.OutSine;
        [SerializeField] private float easeDuration = 0.5f;
        
        [Header("Debug Properties")]
        [SerializeField] private bool displayDebug;
        
        private GridSystem<GridCell<Gem>> _grid;
        private EventBinding<SelectInputEvent> _selectEventBinding;
        private Vector2Int _selectedGem = new Vector2Int(-1, -1); // Inicializa fora da grid para nao selecionar uma gema


        private void Start() => InitializeGrid();

        private void OnEnable()
        {
            _selectEventBinding = new EventBinding<SelectInputEvent>(OnSelectGem);
            EventBus<SelectInputEvent>.Register(_selectEventBinding);
        }

        private void OnDisable() => EventBus<SelectInputEvent>.Unregister(_selectEventBinding);

        private void InitializeGrid()
        {
            _grid = new GridSystem<GridCell<Gem>>(cellSize, gridWidth, gridHeight, origin, displayDebug);
            
            if (_grid == null)
            {
                Debug.Log("Falha ao criar o grid em: InitializeGrid() em GridManager.cs");
                return;
            }
            
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    EventBus<SpawnGridCellEvent>.Publish(new SpawnGridCellEvent(_grid, x, y));
                } 
            }
        }

        // Chamado através do evento de input
        private void OnSelectGem()
        {
            if (Camera.main == null) return;
            var selectedCell = Camera.main.ScreenToWorldPoint(inputReader.MousePosition);
            var gridPosition = _grid.GetGridPosition(selectedCell);
            
            if(!_grid.IsValidPosition(gridPosition.x, gridPosition.y) 
               || _grid.IsEmptyPosition(gridPosition.x, gridPosition.y)) return;

            // Se selecionar a mesma gema duas vezes, desselecionar
            if (_selectedGem == gridPosition) DeselectGem();
            
            // Se selecionar uma gema e não há outra gema selecionada, selecionar
            else if (_selectedGem == Vector2Int.one * -1) SelectGem(gridPosition);
            
            // Se selecionar duas gemes diferentes, executar lógica
            else StartCoroutine(RunGameLoop(_selectedGem, gridPosition));
        }

        private void SelectGem(Vector2Int gridPosition) => _selectedGem = gridPosition;
        private void DeselectGem() => _selectedGem = new Vector2Int(-1, -1);
        
        private IEnumerator RunGameLoop(Vector2Int gridPositionA, Vector2Int gridPositionB)
        {
            // A linha abaixo retorna corretamente as coordenadas
            Debug.Log(gridPositionA + ", " + gridPositionB);
            
            yield return StartCoroutine(SwapGems(gridPositionA, gridPositionB));
            
            var matches = FindMatches();
            yield return StartCoroutine(ExplodeGems(matches));

            yield return StartCoroutine(MakeGemsFall());

            yield return StartCoroutine(FillEmptySpots());
            
            DeselectGem();
        }

        private IEnumerator SwapGems(Vector2Int gridPositionA, Vector2Int gridPositionB)
        {
            var gridCellA = _grid.TryGetGridCell(gridPositionA.x, gridPositionA.y);
            var gridCellB = _grid.TryGetGridCell(gridPositionB.x, gridPositionB.y);
            
            // A linha abaixo esta retornando null
            Debug.Log(_grid.TryGetGridCell(gridPositionA.x, gridPositionA.y).GetCellValue());
                
            if (gridCellA == null || gridCellB == null)
            {
                Debug.Log("Gema A ou B é nula");
                yield break;
            }
            
            var sequence = DOTween.Sequence();
            if(sequence == null) yield break;

            sequence.Append(gridCellA.GetCellValue().transform
                .DOLocalMove(_grid.GetWorldPositionCenter(gridPositionB.x, gridPositionB.y), easeDuration)
                .SetEase(ease));

            sequence.Join(gridCellB.GetCellValue().transform
                .DOLocalMove(_grid.GetWorldPositionCenter(gridPositionA.x, gridPositionA.y), easeDuration)
                .SetEase(ease));

            // Aguardando a sequência ser completada
            yield return sequence.WaitForCompletion();
           
            _grid.SetCoordinateValue(gridPositionA.x, gridPositionA.y, gridCellB);
            _grid.SetCoordinateValue(gridPositionB.x, gridPositionB.y, gridCellA);
        }
        
        private List<Vector2Int> FindMatches()
        {
            var matches = new HashSet<Vector2Int>();
            
            // Horinzontal
            for (int y = 0; y < gridHeight; y++)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    var gemA = _grid.TryGetGridCell(x, y);
                    var gemB = _grid.TryGetGridCell(x+1, y);
                    var gemC = _grid.TryGetGridCell(x+2, y);
                    
                    if(gemA == null || gemB == null || gemC == null) continue;

                    if (gemA.GetCellValue().GetGemType() != gemB.GetCellValue().GetGemType()
                        || gemB.GetCellValue().GetGemType() != gemC.GetCellValue().GetGemType()) continue;
                    
                    matches.Add(new Vector2Int(x, y));
                    matches.Add(new Vector2Int(x+1, y));
                    matches.Add(new Vector2Int(x+2, y));
                }
            }
            
            //Vertical
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    var gemA = _grid.TryGetGridCell(x, y);
                    var gemB = _grid.TryGetGridCell(x, y+1);
                    var gemC = _grid.TryGetGridCell(x, y+2);
                    
                    if(gemA == null || gemB == null || gemC == null) continue;

                    if (gemA.GetCellValue().GetGemType() != gemB.GetCellValue().GetGemType()
                        || gemB.GetCellValue().GetGemType() != gemC.GetCellValue().GetGemType()) continue;
                    
                    matches.Add(new Vector2Int(x, y));
                    matches.Add(new Vector2Int(x, y+1));
                    matches.Add(new Vector2Int(x, y+2));
                }
            }
            
            return new List<Vector2Int>(matches);
        }

        private IEnumerator ExplodeGems(List<Vector2Int> matches)
        {
            // Evento para tocar SFX

            foreach (var match in matches)
            {
                var gem = _grid.TryGetGridCell(match.x, match.y).GetCellValue();
                _grid.SetCoordinateValue(match.x, match.y, null);
            
                const float animationDuration = 0.1f;
                gem.transform.DOPunchScale(Vector3.one * 0.5f, animationDuration, 1, 0.5f);
                
                // Evento para tocar VFX
                
                yield return new WaitForSeconds(animationDuration);
            
                gem.DestroyGem();
            }
        }

        private IEnumerator MakeGemsFall() {
            // TODO: Make this more efficient
            for (var x = 0; x < gridWidth; x++) {
                for (var y = 0; y < gridHeight; y++)
                {
                    if (_grid.TryGetGridCell(x, y) != null) continue;
                    
                    for (var i = y + 1; i < gridHeight; i++)
                    {
                        if (_grid.TryGetGridCell(x, i) == null) continue;
                        
                        var gem = _grid.TryGetGridCell(x, i).GetCellValue();
                        _grid.SetCoordinateValue(x, y, _grid.TryGetGridCell(x, i));
                        _grid.SetCoordinateValue(x, i, null);
                                
                        gem.transform
                            .DOLocalMove(_grid.GetWorldPositionCenter(x, y), 0.05f)
                            .SetEase(ease);
                                
                        // Play SFX
                        yield return new WaitForSeconds(0.05f);
                        break;
                    }
                }
            }
        }

        private IEnumerator FillEmptySpots()
        {
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    if(_grid.TryGetGridCell(x, y) != null) continue;

                    //CreateGem(x, y);
                    // Player SFX
                    yield return new WaitForSeconds(0.05f);
                }
            }
        }
    }
}