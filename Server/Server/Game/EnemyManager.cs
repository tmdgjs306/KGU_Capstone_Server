using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game
{
    class EnemyManager
    {
        public static EnemyManager Instance { get; } = new EnemyManager();
        object _lock = new object();

        public Dictionary<int, Enemy> _enemys = new Dictionary<int, Enemy>();

        public int _enemyId = 1; //TODO

        public void Add()
        {
            Enemy enemy = new Enemy();
            lock (_lock)
            {
                _enemys.Add(_enemyId, enemy);
                enemy.enemyInfo.EnemyId = _enemyId++;
            }
            
        }

        public void Add(Enemy enemy)
        {
            lock (_lock)
            {
                _enemys.Add(_enemyId, enemy);
            }
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
