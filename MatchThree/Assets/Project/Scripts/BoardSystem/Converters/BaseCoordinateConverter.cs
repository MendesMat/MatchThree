using UnityEngine;

namespace MatchThree.Project.Scripts.BoardSystem.Converters
{
    public abstract class BaseCoordinateConverter : MonoBehaviour
    {
        public abstract Vector3 BoardToWorld(int x, int y, float cellSize, Vector3 origin);
        public abstract Vector3 BoardToWorldCenter(int x, int y, float cellSize, Vector3 origin);
        public abstract Vector2Int WorldToBoard(Vector3 worldPosition, float cellSize, Vector3 origin);
        public abstract Vector3 Forward { get; }
    }
}