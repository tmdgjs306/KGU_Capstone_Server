using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Managers : MonoBehaviour
{
    static Managers s_instance;
    static Managers Instance { get { Init(); return s_instance; } } 

    #region Contents
    
    ObjectManager _obj = new ObjectManager();
    public static ObjectManager Object { get { return Instance._obj; } }
    
    NetworkManager _network = new NetworkManager();
    public static NetworkManager Network { get { return Instance._network; } }
    
    ResourceManager _resource = new ResourceManager();
    public static ResourceManager Resource { get { return Instance._resource; } }
    
    SceneManagerEx _scene = new SceneManagerEx();
    public static SceneManagerEx Scene { get { return Instance._scene; } }
    
    PoolManager _pool = new PoolManager();
    public static PoolManager Pool { get { return Instance._pool; } }
    #endregion
    
	void Start()
    {
        Init();
	}

    void Update()
    {
        _network.Update();
    }

    static void Init()
    {
        if (s_instance == null)
        {
			GameObject gameObject = GameObject.Find("@Managers");
            if (gameObject == null)
            {
                gameObject = new GameObject { name = "@Managers" };
                gameObject.AddComponent<Managers>();
            }
            DontDestroyOnLoad(gameObject);
            s_instance = gameObject.GetComponent<Managers>();
            s_instance._network.Init();
        }		
	}
    
    public static void Clear()
    {
        Scene.Clear();
        Pool.Clear();
    }
}
