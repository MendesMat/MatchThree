using TMPro;
using UnityEngine;

namespace MatchThree.Project.Scripts.GridSystems
{
    public class GridSystem<T>
    {
        public float CellSize { get; }
        private readonly int _width;
        private readonly int _height;
        
        private readonly Vector2 _origin;
        private readonly T[,] _gridCell;

        #region Construtor
        public GridSystem(float cellSize, int width, int height, Vector2 origin, bool debug = false)
        {
            CellSize = cellSize;
            _width = width;
            _height = height;
            _origin = origin;
            
            _gridCell = new T[width, height];

            if(debug) EnableDebug();
        }

        #endregion

        #region Conversão de Coordenadas
        public Vector2 GetWorldPosition(int x, int y) 
            => CoordinateConverter.BoardToWorld(x, y, CellSize, _origin);

        public Vector2 GetWorldPositionCenter(int x, int y) 
            => CoordinateConverter.BoardToWorldCenter(x, y, CellSize, _origin);

        public Vector2Int GetGridPosition(Vector2 worldPosition) 
            => CoordinateConverter.WorldToBoard(worldPosition, CellSize, _origin);
        #endregion
        
        #region Manipulação do Board
        // Valida posicionamento dentro do board
        public bool IsValidPosition(int x, int y) => x >= 0 && y >= 0 && x < _width && y < _height;
        public bool IsEmptyPosition(int x, int y) => TryGetGridCell(x, y) == null;
        
        // Obter valor da célula do board
        public T TryGetGridCell(Vector2 worldPosition)
        {
            var position = GetGridPosition(worldPosition);
            var result = TryGetGridCell(position.x, position.y);
            return result;
        }

        public T TryGetGridCell(int x, int y) => IsValidPosition(x,y) ? _gridCell[x,y] : default;

        // Definir valor da célula do grid
        public void SetCoordinateValue(Vector2 worldPosition, T value)
        {
            var position = GetGridPosition(worldPosition);
            SetCoordinateValue(position.x, position.y, value);
        }
        
        public void SetCoordinateValue(int x, int y, T value)
        {
            if (!IsValidPosition(x, y)) return;
            _gridCell[x, y] = value;
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
                        DrawDebugCoordinates(parent, $"{x},{y}", GetWorldPositionCenter(x, y));
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

        private static TextMeshPro DrawDebugCoordinates(GameObject parent, string text, Vector2 position,
            int fontSize = 2, Color color = default, TextAlignmentOptions textAlignment = TextAlignmentOptions.Center, int sortingOrder = 0)
        {
            var myObject = new GameObject("DebugText_" + text, typeof(TextMeshPro));
            myObject.transform.SetParent(parent.transform);
            myObject.transform.position = position;
            
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