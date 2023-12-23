using System;
using System.Collections.Generic;
using Godot;

namespace Organicmatter.Scripts.Internal.Helpers
{
    internal class SeedCoordinates
    {
        private List<Vector2I> _list = new();

        public List<Vector2I> Get() 
        { 
            return _list;
        }

        public void Add(Vector2I coordinates)
        {
            _list.Add(coordinates);
        }

        public void Update(Func<Vector2I, Vector2I> function)
        {
            for (int i = 0; i < _list.Count; ++i)
            {
                _list[i] = function(_list[i]);
            }
        }

        public void Delete(Func<Vector2I, bool> predicate)
        {
            for (int i = _list.Count - 1; i >= 0; --i)
            {
                if (predicate(_list[i]))
                {
                    _list.RemoveAt(i);
                }
            }
        }
    }
}
