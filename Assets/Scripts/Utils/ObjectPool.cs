using UnityEngine;
using System.Collections.Generic;

namespace PoliceChase.Utils
{
    public class ObjectPool<T> where T : MonoBehaviour
    {
        private Queue<T> pool = new Queue<T>();
        private T prefab;
        private Transform parent;

        public ObjectPool(T prefab, int initialSize, Transform parent = null)
        {
            this.prefab = prefab;
            this.parent = parent;
            for (int i = 0; i < initialSize; i++)
            {
                var obj = GameObject.Instantiate(prefab, parent);
                obj.gameObject.SetActive(false);
                pool.Enqueue(obj);
            }
        }

        public T Get(Vector3 pos, Quaternion rot)
        {
            T obj;
            if (pool.Count > 0)
            {
                obj = pool.Dequeue();
            }
            else
            {
                obj = GameObject.Instantiate(prefab, parent);
            }
            obj.transform.position = pos;
            obj.transform.rotation = rot;
            obj.gameObject.SetActive(true);
            return obj;
        }

        public void Return(T obj)
        {
            obj.gameObject.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    public class SimpleObjectPool : MonoBehaviour
    {
        public GameObject Prefab;
        public int InitialSize = 20;
        private Queue<GameObject> pool = new Queue<GameObject>();

        private void Awake()
        {
            for (int i = 0; i < InitialSize; i++)
            {
                var obj = Instantiate(Prefab, transform);
                obj.SetActive(false);
                pool.Enqueue(obj);
            }
        }

        public GameObject Get(Vector3 pos, Quaternion rot)
        {
            GameObject obj;
            if (pool.Count > 0)
                obj = pool.Dequeue();
            else
                obj = Instantiate(Prefab, transform);

            obj.transform.position = pos;
            obj.transform.rotation = rot;
            obj.SetActive(true);
            return obj;
        }

        public void Return(GameObject obj)
        {
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }
}
