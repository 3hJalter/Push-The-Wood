using _Game.DesignPattern.StateMachine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace _Game.GameGrid.Unit.DynamicUnit.Player.PlayerState
{
    public abstract class AbstractPlayerState : IState<Player>
    {
        public abstract StateEnum Id { get; }

        public virtual void OnEnter(Player t){}
        public virtual void OnExecute(Player t){}
        public virtual void OnExit(Player t){}
        //NOTE: Save Input When Player Enter A StateW
        public virtual void UpdateDirection(Player t)
        {
            if (t.CommandCache.Count > 0)
            {
                t.Direction = t.CommandCache.Dequeue();
                SaveCommand(t);
            }
            else
            {
                t.Direction = t.InputDirection;
            }
        }

        public virtual void SaveCommand(Player t)
        {
            if (t.InputDirection != Direction.None && t.InputDetection.InputAction == InputAction.ButtonDown)
            {
                // MAXIMUM COMMAND CACHE is 1
                if (t.CommandCache.Count > 0)
                {
                    return;
                }
                t.CommandCache.Enqueue(t.InputDirection);
            }
        }
    }
}