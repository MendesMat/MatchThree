using MatchThree.Project.Scripts.Core.EventBus;
using MatchThree.Project.Scripts.Core.EventBus.Events;
using UnityEngine;

namespace MatchThree.Project.Scripts.Gems
{
    public class GemFactory : MonoBehaviour
    {
        [SerializeField] private Gem gemPrefab;
        public GemSO[] gemTypes;

        private EventBinding<SpawnGemEvent> _spawnEventBinding;

        private void OnEnable()
        {
            _spawnEventBinding = new EventBinding<SpawnGemEvent>(SpawnGem);
            EventBus<SpawnGemEvent>.Register(_spawnEventBinding);
        }

        private void OnDisable() => EventBus<SpawnGemEvent>.Unregister(_spawnEventBinding);

        private void SpawnGem(SpawnGemEvent eventData)
        {
            var x = eventData.X;
            var y = eventData.Y;
            var grid = eventData.Grid;
            
            if (gemPrefab == null)
            {
                Debug.LogError("GemFactory: Gem prefab não atribuído. Impossível criar gema.");
                return;
            }
            
            var gem = Instantiate(gemPrefab, grid.GetWorldPositionCenter(x, y), Quaternion.identity, transform);
            gem.SetGemType(gemTypes[Random.Range(0, gemTypes.Length)]);
            
            EventBus<SpawnResponseEvent<Gem>>.Publish(new SpawnResponseEvent<Gem>(grid, gem, x, y));
        }
    }
}