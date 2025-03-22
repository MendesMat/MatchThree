using System;

namespace MatchThree.Project.Scripts.Core.EventBus
{
    public class EventBinding<T> : IEventBinding<T> 
        where T : IEvent
    {
        // Inicializo os eventos com métodos vazios para evitar null reference exceptions
        private Action<T> _onEvent = _ => { };
        private Action _onEventNoArgs = () => { };
        
        // Implementação explícita da interface: 
        // - Esses membros só podem ser acessados por meio de uma referência do tipo IEventBinding<T>.
        // - Isso evita que a classe exponha esses eventos diretamente, forçando o uso da interface.
        Action<T> IEventBinding<T>.OnEvent 
        { 
            get => _onEvent; 
            set => _onEvent = value;
        }
        
        Action IEventBinding<T>.OnEventNoArgs 
        { 
            get => _onEventNoArgs; 
            set => _onEventNoArgs = value; 
        }
        
        // Construtores para facilitar a criação de EventBindings
        public EventBinding(Action<T> onEvent) => _onEvent = onEvent;
        public EventBinding(Action onEventNoArgs) => _onEventNoArgs = onEventNoArgs;
        
        // Métodos para adicionar e remover eventos dinamicamente
        public void Add(Action<T> onEvent) => _onEvent += onEvent;
        public void Remove(Action<T> onEvent) => _onEvent -= onEvent;
        
        public void Add(Action onEvent) => _onEventNoArgs += onEvent;
        public void Remove(Action onEvent) => _onEventNoArgs -= onEvent;

    }
}