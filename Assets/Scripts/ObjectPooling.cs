using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ObjectPooling : MonoBehaviour
{
    [Serializable]
    public struct MultiVariable                                                         //관리 용이하기위한 구조체
    {
        public GameObject prefab;
        public int poolingCount;
        public Queue<GameObject> poolingObjectQueue ;
    }
    public MultiVariable[] poolingItem = new MultiVariable[6];

    private static ObjectPooling instance = null;
    
    public static ObjectPooling Instance()                                          //싱글톤 인스턴스
    {
        if (instance == null)
        {
            GameObject obj = new GameObject("ObjectPooling Manager");
            obj.AddComponent<ObjectPooling>();
            instance = obj.GetComponent<ObjectPooling>();
        }
        return instance;
    }
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

        Init();
    }
    private void OnDestroy()
    {
        instance = null;
    }

   
    private void Init()
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

    private GameObject CreateObject(GameObject _prefab)                                             //오브젝트 생성
    {
        GameObject obj = Instantiate(_prefab);
        obj.gameObject.SetActive(false);
        obj.transform.SetParent(transform);
        return obj;
    }

    public GameObject GetObject(int _poolingObjectNum,Vector2 _pos)                             //생성해둔 오브젝트 가져오는 함수
    {
        if (instance.poolingItem[_poolingObjectNum].poolingCount > 0)
        {
            var obj = instance.poolingItem[_poolingObjectNum].poolingObjectQueue.Dequeue();
            obj.transform.position = Vector2.zero;
            obj.transform.position = _pos;
            obj.transform.SetParent(null);
            obj.gameObject.SetActive(true);
            return obj;
        }
        else
        {
            var newObj = instance.CreateObject(instance.poolingItem[_poolingObjectNum].prefab);
            newObj.gameObject.SetActive(true);
            newObj.transform.SetParent(null);
            return newObj;
        }
    }
    public void ReturnObject(int _poolingObjectNum, GameObject _obj)                            //사용끝난 오브젝트 반환하는 함수
    {
        _obj.SetActive(false);
        _obj.transform.position = Vector2.zero;
        _obj.transform.name = "";
        _obj.transform.SetParent(instance.transform);
        
        instance.poolingItem[_poolingObjectNum].poolingObjectQueue.Enqueue(_obj);
    }
}
