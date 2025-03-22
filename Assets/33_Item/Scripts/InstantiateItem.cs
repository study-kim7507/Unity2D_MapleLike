using System;
using System.Linq;
using Google.Protobuf.Protocol;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Random = UnityEngine.Random;


public class InstantiateItem : MonoBehaviour
{
    [SerializeField] private Transform SpwanPos;
    [SerializeField] private AssetLabelReference Ref;
    [SerializeField] ItemType DropItemType;
    
    
    void Start()
    {

        DropItem();
    }

    public  void  DropItem()
    {
        Array values = Enum.GetValues(typeof(ItemType));
        DropItemType = (ItemType)Random.Range(0, values.Length);
        //라벨을 가지고 있는 어드레서블을 가져오기
        Addressables.LoadResourceLocationsAsync(Ref).Completed +=
            (handle) =>
            {
                var locations = handle.Result;
                locations = locations.OrderBy(loc => loc.PrimaryKey).ToList();
                Addressables.InstantiateAsync(locations[(int)DropItemType],SpwanPos.position,SpwanPos.rotation); 
            };
    }



}
