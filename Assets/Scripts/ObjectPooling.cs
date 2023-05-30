using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ObjectPooling : MonoBehaviour
{
    [Serializable]
    public struct MultiVariable                                                        
    {
        public GameObject prefab;                                   //생성할 object
        public int poolingCount;                                      //object의 개수
        public Queue<GameObject> poolingObjectQueue ;   //생성한 object를 담을 큐
    }
    public int DefaultTileCount;                                      //움직일 타일의 수
    public MultiVariable[] poolingItem = new MultiVariable[0];
    
    #region 인스턴스
    private static ObjectPooling instance = null;
    
    public static ObjectPooling Instance() 
    {
        if (instance == null)
        {
            GameObject obj = new GameObject("Object Manager");
            obj.AddComponent<ObjectPooling>();
            instance = obj.GetComponent<ObjectPooling>();
        }
        return instance;
    }
    #endregion
    private void Awake()
    {
        if(instance == null)
        {
            DontDestroyOnLoad(this.gameObject);
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }

        Initialization();
    }
    private void OnDestroy()
    {
        instance = null;
    }

    //타일을 생성하고 풀링 큐에 등록
    private void Initialization()                                                                                                 
    {
        for(int i=0; i<poolingItem.Length; i++)
        {
            poolingItem[i].poolingObjectQueue = new Queue<GameObject>();
            for (int j=0; j<poolingItem[i].poolingCount; j++)
            {
                poolingItem[i].poolingObjectQueue.Enqueue(CreateObject(poolingItem[i].prefab));
            }
        }
    }

    //생성해둔 오브젝트 사용하는 함수
    public GameObject GetObject(int _poolingObjectNum, Vector2 _pos)                                        
    {
        if (instance.poolingItem[_poolingObjectNum].poolingCount > 0)
        {
            var obj = instance.poolingItem[_poolingObjectNum].poolingObjectQueue.Dequeue();
            obj.transform.position = Vector2.zero;
            obj.transform.position = _pos;
            obj.transform.localScale = new Vector2(1.2f, 1.2f);
            obj.transform.SetParent(null);
            obj.gameObject.SetActive(true);
            return obj;
        }
        else
        {
            var newObj = instance.CreateObject(instance.poolingItem[_poolingObjectNum].prefab);
            newObj.gameObject.SetActive(true);
            newObj.transform.localScale = new Vector2(1.2f, 1.2f);
            newObj.transform.SetParent(null);
            return newObj;
        }
    }

    //사용끝난 오브젝트 반환하는 함수
    public void ReturnObject(int _poolingObjectNum, GameObject _obj)
    {
        _obj.SetActive(false);
        _obj.transform.position = Vector2.zero;
        _obj.transform.name = "";
        _obj.transform.SetParent(instance.transform);

        instance.poolingItem[_poolingObjectNum].poolingObjectQueue.Enqueue(_obj);
    }

    //오브젝트 생성 함수
    private GameObject CreateObject(GameObject _prefab)                                                         
    {
        GameObject obj = Instantiate(_prefab);
        obj.gameObject.SetActive(false);
       
        obj.transform.SetParent(transform);
        return obj;
    }

    
}
