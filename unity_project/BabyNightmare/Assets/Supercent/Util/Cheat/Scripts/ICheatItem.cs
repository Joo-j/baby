using System;

namespace Supercent.Util.Cheat
{
    public interface ICheatItem
    {
        public int Id { get; }
        public string Name { get; }
        public string Description  { get; }
        public Delegate OnExecute  { get; }
        public E_CheatType CheatType  { get; }
        public void Init(int id, string name, string description, E_CheatType cheatType, Delegate onExecute, params string[] param);
    }
}