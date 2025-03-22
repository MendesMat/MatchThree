namespace MatchThree.Project.Scripts.Core.EventBus.Events
{
    public class CellValueChangeEvent<T> : IEvent
    {
        public int X { get; set; }
        public int Y { get; set; }
        public T Type { get; set; }
    }
}