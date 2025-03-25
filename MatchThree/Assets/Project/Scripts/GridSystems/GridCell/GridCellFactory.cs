using MatchThree.Project.Scripts.Core.EventBus;
using MatchThree.Project.Scripts.Core.EventBus.Events;
using MatchThree.Project.Scripts.Gems;
using UnityEngine;

namespace MatchThree.Project.Scripts.GridSystems.GridCell
{
    public class GridCellFactory : MonoBehaviour
    {
        private EventBinding<SpawnGridCellEvent> _spawnEventBinding;
        private EventBinding<SpawnResponseEvent<Gem>> _spawnResponseEventBinding;

        private void OnEnable()
        {
            _spawnEventBinding = new EventBinding<SpawnGridCellEvent>(GemSetup);
            _spawnResponseEventBinding = new EventBinding<SpawnResponseEvent<Gem>>(CreateGridCell);
            
            EventBus<SpawnGridCellEvent>.Register(_spawnEventBinding);
            EventBus<SpawnResponseEvent<Gem>>.Register(_spawnResponseEventBinding);
        }

        private void OnDisable()
        {
            EventBus<SpawnGridCellEvent>.Unregister(_spawnEventBinding);
            EventBus<SpawnResponseEvent<Gem>>.Unregister(_spawnResponseEventBinding);
        }

        private void GemSetup(SpawnGridCellEvent eventData)
        {
            Debug.Log("GemSetup");
            var grid = eventData.Grid;
            var x = eventData.X;
            var y = eventData.Y;
            
            EventBus<SpawnGemEvent>.Publish(new SpawnGemEvent(grid, x, y));
        }

        private void CreateGridCell(SpawnResponseEvent<Gem> eventData)
        {
            Debug.Log("CreateGridCell");
            var grid = eventData.Grid;
            var gem = eventData.Value;
            var x = eventData.X;
            var y = eventData.Y;
            
            var gridCell = new GridCell<Gem>(grid, x, y);
            
            if(gem != null) gridCell.SetCellValue(gem);
            grid.SetCoordinateValue(x, y, gridCell);
        }
    }
}