using System.Collections.Generic;
using UnityEngine;

public class GameObjectPool
{
    public static GameObject PoolParent;
    protected readonly Queue<GameObject> pool;
    private readonly GameObject prefab;
    protected GameObject parent;
    private int parity;

    public GameObjectPool( GameObject prefab, int preAllocateCount )
    {
        GameObjectPool.CheckPoolParent();
        this.parent = GameObjectPool.CreateGameObjectsParent( prefab );
        this.prefab = prefab;
        this.pool = new Queue<GameObject>( preAllocateCount );
        for ( int i = 0; i < preAllocateCount; i++ )
        {
            GameObject item = this.CreateNew();
            if ( item != null )
            {
                this.pool.Enqueue( item );
            }
        }
    }

    public bool AllRecycled => this.parity == 0;

    public static void CheckPoolParent()
    {
        if ( GameObjectPool.PoolParent == null )
        {
            GameObjectPool.PoolParent = new GameObject( "GameObjects Pools" );
            Object.DontDestroyOnLoad( GameObjectPool.PoolParent );
        }
    }

    public static GameObject CreateGameObjectsParent( GameObject prefab )
    {
        GameObject pool = new GameObject( prefab.name + " Pool" );
        pool.transform.SetParent( GameObjectPool.PoolParent.transform );
        return pool;
    }

    private GameObject CreateNew()
    {
        GameObject gameObject = Object.Instantiate( this.prefab );
        if ( gameObject != null )
        {
            gameObject.SetActive( false );
            gameObject.transform.SetParent( this.parent.transform );
            return gameObject;
        }

        return null;
    }

    public GameObject Get()
    {
        this.parity++;
        if ( this.pool.Count > 0 )
        {
            GameObject item = this.pool.Dequeue();
            item.SetActive( true );
            return item;
        }

        return this.CreateNew();
    }

    public void Recycle( GameObject item )
    {
        this.parity--;
        item.transform.SetParent( this.parent.transform );
        item.SetActive( false );
        this.pool.Enqueue( item );
    }
}

public class GameObjectPool<T> where T : Component
{
    protected readonly Queue<T> pool;
    protected readonly GameObject prefab;
    protected GameObject parent;
    protected int parity;

    public GameObjectPool( GameObject prefab )
    {
        GameObjectPool.CheckPoolParent();
        this.parent = GameObjectPool.CreateGameObjectsParent( prefab );
        this.prefab = prefab;
        this.pool = new Queue<T>();
    }

    public GameObjectPool( GameObject prefab, int preAllocateCount )
    {
        GameObjectPool.CheckPoolParent();
        this.parent = GameObjectPool.CreateGameObjectsParent( prefab );

        this.prefab = prefab;
        this.pool = new Queue<T>( preAllocateCount );
        for ( int i = 0; i < preAllocateCount; i++ )
        {
            // ReSharper disable once VirtualMemberCallInConstructor
            T item = this.CreateNew();
            if ( item != null )
            {
                this.pool.Enqueue( item );
            }
        }
    }

    public bool AllRecycled => this.parity == 0;

    protected virtual T CreateNew()
    {
        GameObject gameObject = Object.Instantiate( this.prefab );
        if ( gameObject != null )
        {
            T item = gameObject.GetComponent<T>();
            if ( item != null )
            {
                gameObject.SetActive( false );
                gameObject.transform.SetParent( this.parent.transform );
                return item;
            }

            Object.Destroy( gameObject );
        }

        return null;
    }

    public virtual T Get( bool enable = true )
    {
        this.parity++;
        T item = this.pool.Count > 0 ? this.pool.Dequeue() : this.CreateNew();
        if ( enable )
        {
            item.gameObject.SetActive( true );
        }

        return item;
    }

    public virtual void Recycle( T item )
    {
        this.parity--;
        item.gameObject.transform.SetParent( this.parent.transform );
        item.gameObject.SetActive( false );
        this.pool.Enqueue( item );
    }
}