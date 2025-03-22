using MatchThree.Project.Scripts.BoardSystem.Converters;
using MatchThree.Project.Scripts.Core.EventBus;
using MatchThree.Project.Scripts.Core.EventBus.Events;
using TMPro;
using UnityEngine;

namespace MatchThree.Project.Scripts.BoardSystem
{
    public class BoardSystem<T>
    {
        [Header("Board Properties")] 
        private readonly float _cellSize;
        private readonly int _width;
        private readonly int _height;
        
        private readonly Vector3 _origin;
        private readonly T[,] _boardTiles;
        private readonly BaseCoordinateConverter _coordinateConverter;

        // Construtor
        public BoardSystem(float cellSize, int width, int height, 
            Vector3 origin, T[,] boardTiles, BaseCoordinateConverter coordinateConverter,
            bool debug = false)
        {
            _cellSize = cellSize;
            _width = width;
            _height = height;
            _origin = origin;
            _boardTiles = new T[width, height];
            _coordinateConverter = coordinateConverter ?? new VerticalConverter();

            if (debug) DrawDebugLines();
        }

        #region Metodos

        // Validar entrada
        private bool IsValidPosition(int x, int y) => x >= 0 && y >= 0 && x < _width && y < _height;
        
        // Obter coordenadas de uma celula do board
        private Vector2Int GetXY(Vector3 worldPosition) => _coordinateConverter.WorldToBoard(worldPosition, _cellSize, _origin);
        
        // Obter valor da célula do board
        private T GetValue(Vector3 worldPosition)
        {
            var position = GetXY(worldPosition);
            var result = GetValue(position.x, position.y);
            return result;
        }

        private T GetValue(int x, int y) => IsValidPosition(x,y) ? _boardTiles[x,y] : default;

        // Definir valor da célula do grid
        private void SetValue(Vector3 worldPosition, T value)
        {
            var position = GetXY(worldPosition);
            SetValue(position.x, position.y, value);
        }
        
        private void SetValue(int x, int y, T value)
        {
            if (!IsValidPosition(x, y)) return;
            _boardTiles[x, y] = value;
            
            // Publica evento de mudança de valor
            EventBus<CellValueChangeEvent<T>>.Publish(new CellValueChangeEvent<T>
            {
                X = x,
                Y = y,
                Type = value
            });
        }
        #endregion
        
        #region Debug
        private void DrawDebugLines()
        {
            const float duration = 100f;
            var parent = new GameObject("Debugger");

            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    CreateWorldText(parent, x + "," + y, GetWorldPositionCenter(x, y), _coordinateConverter.Forward);
                    Debug.DrawLine(GetWorldPosition(x,y), GetWorldPosition(x,y+1), Color.green, duration);
                    Debug.DrawLine(GetWorldPosition(_width, 0), GetWorldPosition(_width, _height), Color.green, duration);
                }
            }
        }

        private TextMeshPro CreateWorldText(GameObject parent, string text, Vector3 position, Vector3 direction,
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
        
        private Vector3 GetWorldPosition(int x, int y) 
            => _coordinateConverter.BoardToWorld(x, y, _cellSize, _origin);
        
        private Vector3 GetWorldPositionCenter(int x, int y) 
            => _coordinateConverter.BoardToWorldCenter(x, y, _cellSize, _origin);
        
        private Vector2Int GetBoardPosition(Vector3 worldPosition) 
            => _coordinateConverter.WorldToBoard(worldPosition, _cellSize, _origin);
        #endregion
    }
}