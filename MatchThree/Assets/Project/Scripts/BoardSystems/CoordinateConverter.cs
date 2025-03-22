using UnityEngine;

namespace MatchThree.Project.Scripts.BoardSystems
{
    public static class CoordinateConverter
    {
        public static Vector3 BoardToWorld(int x, int y, float cellSize, Vector3 origin)
        {
            var result = new Vector3(x, y, 0) * cellSize + origin;
            return result;
        }

        public static Vector3 BoardToWorldCenter(int x, int y, float cellSize, Vector3 origin)
        {
            var newX = x * cellSize + cellSize * 0.5f;
            var newY = y * cellSize + cellSize * 0.5f;
            var result = new Vector3(newX, newY, 0) + origin;
            return result;
        }

        public static Vector2Int WorldToBoard(Vector3 worldPosition, float cellSize, Vector3 origin)
        {
            var boardPosition = (worldPosition - origin) / cellSize;
            var newX = Mathf.FloorToInt(boardPosition.x);
            var newY = Mathf.FloorToInt(boardPosition.y);
            var result = new Vector2Int(newX, newY);
            return result;
        }

        public static Vector3 Forward => Vector3.forward;
    }
}