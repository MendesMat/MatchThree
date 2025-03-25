using UnityEngine;

namespace MatchThree.Project.Scripts.GridSystems
{
    public static class CoordinateConverter
    {
        public static Vector2 BoardToWorld(int x, int y, float cellSize, Vector2 origin)
        {
            return new Vector2(x, y) * cellSize + origin;
        }

        public static Vector2 BoardToWorldCenter(int x, int y, float cellSize, Vector2 origin)
        {
            var newX = x * cellSize + cellSize * 0.5f;
            var newY = y * cellSize + cellSize * 0.5f;
            var result = new Vector2(newX, newY) + origin;
            return result;
        }

        public static Vector2Int WorldToBoard(Vector2 worldPosition, float cellSize, Vector2 origin)
        {
            var boardPosition = (worldPosition - origin) / cellSize;
            var newX = Mathf.FloorToInt(boardPosition.x);
            var newY = Mathf.FloorToInt(boardPosition.y);
            var result = new Vector2Int(newX, newY);
            return result;
        }
    }
}