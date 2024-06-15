using UnityEngine;

namespace LB
{
    [System.Serializable]
    public class Tile
    {
        public GameObject prefab;
        [Range(0,3)]
        public int Rotations;        
    }
}