using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game
{
    class EnemyManager
    {

        object _lock = new object();

        public Dictionary<int, Enemy> _enemys = new Dictionary<int, Enemy>();

        public int _enemyId = 1; //TODO

        public Enemy Add()
        {
            Enemy enemy = new Enemy();
            lock (_lock)
            {
                enemy.enemyInfo.EnemyId = _enemyId++;
                _enemys.Add(_enemyId, enemy);
            }
            return enemy;
        }

        public bool Remove(int enemyId)
        {
            lock (_lock)
            {
                return _enemys.Remove(enemyId);
            }
        }

        public Enemy Find(int enemyId)
        {
            lock (_lock)
            {
                Enemy enemy = null;
                if (_enemys.TryGetValue(enemyId, out enemy))
                    return enemy;
                return null;
            }
        }
    }
}
