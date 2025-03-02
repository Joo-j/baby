using System;

namespace Supercent.Util.Cheat
{
    public class CheatItem<TValue> : ICheatItem

    {
        //------------------------------------------------------------------------------
        // variables
        //------------------------------------------------------------------------------
        private int _id;
        private string _name;
        private string _description;
        private Delegate _onExecute;
        private E_CheatType _cheatType;
        //------------------------------------------------------------------------------
        // properties
        //------------------------------------------------------------------------------
        public int Id => _id;
        public string Name => _name;
        public string Description => _description;
        public Delegate OnExecute => _onExecute;
        public E_CheatType CheatType => _cheatType;
        
        public void Init(int id, string name, string description, E_CheatType cheatType, Delegate onExecute, params string[] param)
        {
            _id = id;
            _name = name;
            _description = description;
            _onExecute = onExecute;
            _cheatType = cheatType;
        }
    }
}