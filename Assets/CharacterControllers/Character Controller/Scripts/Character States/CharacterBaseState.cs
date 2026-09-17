
using UnityEngine;
namespace CHController
{
    public abstract class CharacterBaseState : BaseState
    {
        protected CharacterStateMachine StateMachine { get; private set; }

        public CharacterBaseState() : base() { }

        public override void Initialization(BaseStateMachine stateMachine, StateFactory characterStateFactory)
        {
            base.Initialization(stateMachine, characterStateFactory);
            StateMachine = StateMachine == null ? (CharacterStateMachine)stateMachine : StateMachine;
        }
    }
}
