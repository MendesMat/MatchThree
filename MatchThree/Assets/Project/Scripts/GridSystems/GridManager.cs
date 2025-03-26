using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
            
            EnsureNoInitialMatches();
        }

        private void EnsureNoInitialMatches()
        {
            List<Vector2Int> matches;
        
            do
            {
                matches = FindMatches(); // Encontre todas as combinações
                if (matches.Count > 0) RefactorGems();
            } while (matches.Count > 0); // Refaça até não encontrar mais combinações
        }

        private void RefactorGems()
        {
            var gemFactory = gameObject.GetComponent<GemFactory>();
            if (gemFactory == null)
            {
                Debug.Log("GemFactory não encontrado em RefactorGems() em GridManager.cs");
                return;
            }
            
            var gemTypes = gemFactory.gemTypes;
            if (gemTypes == null || gemTypes.Length == 0)
            {
                Debug.LogError("Nenhum tipo de gema disponível no GemFactory.");
                return;
            }
            
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    var gem = _grid.TryGetGridCell(x, y)?.GetCellValue();
                    if (gem == null) continue;

                    var possibleTypes = new HashSet<GemSO>(gemTypes);

                    // Remover tipos que criam um match horizontal
                    if (x > 1)
                    {
                        var gemLeft1 = _grid.TryGetGridCell(x - 1, y)?.GetCellValue()?.GetGemType();
                        var gemLeft2 = _grid.TryGetGridCell(x - 2, y)?.GetCellValue()?.GetGemType();
                        if (gemLeft1 != null && gemLeft1 == gemLeft2)
                            possibleTypes.Remove(gemLeft1);
                    }

                    // Remover tipos que criam um match vertical
                    if (y > 1)
                    {
                        var gemBelow1 = _grid.TryGetGridCell(x, y - 1)?.GetCellValue()?.GetGemType();
                        var gemBelow2 = _grid.TryGetGridCell(x, y - 2)?.GetCellValue()?.GetGemType();
                        if (gemBelow1 != null && gemBelow1 == gemBelow2)
                            possibleTypes.Remove(gemBelow1);
                    }

                    // Escolher um novo tipo de gema evitando matches
                    if (possibleTypes.Count > 0)
                    {
                        var index = Random.Range(0, possibleTypes.Count);
                        var newGemType = possibleTypes.ElementAt(index);
                        gem.SetGemType(newGemType);
                    }
                    
                    else gem.SetGemType(gemTypes[Random.Range(0, gemTypes.Length)]);
                }
            }
        }

        private void OnSelectGem()
        {
            if (Camera.main == null) return;
            var selectedCell = Camera.main.ScreenToWorldPoint(inputReader.MousePosition);
            var gridPosition = _grid.GetGridPosition(selectedCell);
            
            if(!_grid.IsValidPosition(gridPosition.x, gridPosition.y) 
               || _grid.IsEmptyPosition(gridPosition.x, gridPosition.y)) return;

            // Se selecionar a mesma gema duas vezes, desselecionar
            if (_selectedGem == gridPosition)
            {
                DeselectGem();
                return;
            }
            
            // Se selecionar uma gema e não há outra gema selecionada, selecionar
            if (_selectedGem == Vector2Int.one * -1)
            {
                SelectGem(gridPosition);
                return;
            }
            
            if (!IsAdjacent(_selectedGem, gridPosition))
            {
                DeselectGem();
                return;
            }
            
            // Se selecionar duas gemas adjacentes, executar lógica
            StartCoroutine(RunGameLoop(_selectedGem, gridPosition));
        }
        
        // Compara se as gemas são adjacentes
        private static bool IsAdjacent(Vector2Int selectedGem, Vector2Int gridPosition)
        {
            var difference = selectedGem - gridPosition;
            
            var isAdjacent = (Mathf.Abs(difference.x) == 1 && difference.y == 0) 
                             || (Mathf.Abs(difference.y) == 1 && difference.x == 0);

            return isAdjacent;
        }

        private void SelectGem(Vector2Int gridPosition) => _selectedGem = gridPosition;

        private void DeselectGem() => _selectedGem = new Vector2Int(-1, -1);

        private IEnumerator RunGameLoop(Vector2Int gridPositionA, Vector2Int gridPositionB)
        {
            yield return StartCoroutine(SwapGems(gridPositionA, gridPositionB));
            DeselectGem();

            while (true)
            {
                var matches = FindMatches();
                if(matches.Count == 0) break;
                
                yield return StartCoroutine(ExplodeGems(matches));
                yield return StartCoroutine(MakeGemsFall());
                yield return StartCoroutine(FillEmptySpots());
            }
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
            foreach (var match in matches)
            {
                var gem = _grid.TryGetGridCell(match.x, match.y).GetCellValue();
                _grid.SetCoordinateValue(match.x, match.y, null);
            
                const float animationDuration = 0.1f;
                gem.transform.DOPunchScale(Vector3.one * 0.5f, animationDuration, 1, 0.5f);
                
                yield return new WaitForSeconds(animationDuration);
            
                gem.DestroyGem();
            }
        }

        private IEnumerator MakeGemsFall() 
        {
            for (int x = 0; x < gridWidth; x++) {
                for (int y = 0; y < gridHeight; y++)
                {
                    if (_grid.TryGetGridCell(x, y) != null) continue;
                    
                    for (int i = y + 1; i < gridHeight; i++)
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
            var filledAny = false;
            
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    if(_grid.TryGetGridCell(x, y) != null) continue;

                    EventBus<SpawnGemEvent>.Publish(new SpawnGemEvent(_grid, x, y));
                    filledAny = true;
                    yield return new WaitForSeconds(0.05f);
                }
            }

            if (filledAny) yield return StartCoroutine(FillEmptySpots());
        }
    }
}