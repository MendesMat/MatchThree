using MatchThree.Project.Scripts.Core.EventBus;
using MatchThree.Project.Scripts.Core.EventBus.Events;
using TMPro;
using UnityEngine;

namespace MatchThree.Project.Scripts.BoardSystems
{
    public class GridSystem<T>
    {
        [Header("GridCell Properties")] 
        private readonly float _cellSize;
        private readonly int _width;
        private readonly int _height;
        
        private readonly Vector3 _origin;
        private readonly T[,] _gridCells;
        
        #region Construtor
        public GridSystem(float cellSize, int width, int height, Vector3 origin, bool debug = false)
        {
            _cellSize = cellSize;
            _width = width;
            _height = height;
            _origin = origin;
            
            _gridCells = new T[width, height];

            if(debug) EnableDebug();
        }
        #endregion

        #region Conversão de Coordenadas
        private Vector3 GetWorldPosition(int x, int y) 
            => CoordinateConverter.BoardToWorld(x, y, _cellSize, _origin);

        public Vector3 GetWorldPositionCenter(int x, int y) 
            => CoordinateConverter.BoardToWorldCenter(x, y, _cellSize, _origin);
        
        private Vector2Int GetGridPosition(Vector3 worldPosition) 
            => CoordinateConverter.WorldToBoard(worldPosition, _cellSize, _origin);
        #endregion
        
        #region Manipulação do Board
        // Valida posicionamento dentro do board
        private bool IsValidPosition(int x, int y) => x >= 0 && y >= 0 && x < _width && y < _height;
        
        // Obter valor da célula do board
        public T GetValue(Vector3 worldPosition)
        {
            var position = GetGridPosition(worldPosition);
            var result = GetValue(position.x, position.y);
            return result;
        }

        public T GetValue(int x, int y) => IsValidPosition(x,y) ? _gridCells[x,y] : default;

        // Definir valor da célula do grid
        public void SetValue(Vector3 worldPosition, T value)
        {
            var position = GetGridPosition(worldPosition);
            SetValue(position.x, position.y, value);
        }
        
        public void SetValue(int x, int y, T value)
        {
            if (!IsValidPosition(x, y)) return;
            _gridCells[x, y] = value;
            
            // Publica um evento de mudança de valor
            EventBus<CellValueChangeEvent<T>>.Publish(new CellValueChangeEvent<T>
            {
                X = x,
                Y = y,
                Type = value
            });
        }
        #endregion
        
        #region Debug
        private GameObject _debugParent;

        private void EnableDebug()
        {
            if(_debugParent != null) Object.Destroy(_debugParent);
            if(_debugParent == null) _debugParent = new GameObject("DebugBoard");
            
            DrawDebugGrid();
        }
        
        private void DrawDebugGrid()
        {
            const float duration = 100f;
            var parent = new GameObject("Debugger");

            for (int x = 0; x <= _width; x++)
            {
                for (int y = 0; y <= _height; y++)
                {
                    // Textos de coordenadas
                    if (x < _width && y < _height)
                    {
                        DrawDebugCoordinates(parent, $"{x},{y}", GetWorldPositionCenter(x, y), CoordinateConverter.Forward);
                    }

                    // Linhas horizontais
                    if (x < _width)
                    {
                        Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x + 1, y), Color.white, duration);
                    }

                    // Linhas verticais
                    if (y < _height)
                    {
                        Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x, y + 1), Color.white, duration);
                    }
                }
            }
        }

        private static TextMeshPro DrawDebugCoordinates(GameObject parent, string text, Vector3 position, Vector3 direction,
            int fontSize = 2, Color color = default, TextAlignmentOptions textAlignment = TextAlignmentOptions.Center, int sortingOrder = 0)
        {
            var myObject = new GameObject("DebugText_" + text, typeof(TextMeshPro));
            myObject.transform.SetParent(parent.transform);
            myObject.transform.position = position;
            myObject.transform.forward = direction;
            
            var myText = myObject.GetComponent<TextMeshPro>();
            myText.text = text;
            myText.fontSize = fontSize;
            myText.color = color == default ? Color.white : color;
            myText.alignment = textAlignment;
            myText.GetComponent<MeshRenderer>().sortingOrder = sortingOrder;

            return myText;
        }
        #endregion
    }
}