using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct ManagerAct
{
    public int version;
    public int chainId;
    public List<Collection> collections;
}

[System.Serializable]
public struct Artwork
{
    public string id;
    public string title;
    public string description;
    public Urls urls;
    public Creator creator;
    public Owner owner;
    public Attributes attributes;
    public string price;
    public string priceType;
    public PurchaseTransactionData purchaseTransactionData;
}

[System.Serializable]
public struct Attributes
{
    public Position position;
    public Rotation rotation;
    public Scale scale;
}

[System.Serializable]
public class Collection
{
    public Collection collection;
    public List<Artwork> artworks;
}

[System.Serializable]
public struct Collection2
{
    public string title;
    public string cover;
    public string icon;
    public string description;
    public string creator;
}

[System.Serializable]
public struct Creator
{
    public string name;
    public string icon;
}

[System.Serializable]
public struct Owner
{
    public string name;
}

[System.Serializable]
public struct PurchaseTransactionData
{
    public string data;
    public string to;
    public object value;
}

[System.Serializable]
public struct Position
{
    public double x;
    public double? y;
    public double? z;
}

[System.Serializable]
public struct Rotation
{
    public double x;
    public double y;
    public double z;
}

[System.Serializable]
public struct Scale
{
    public double x;
    public double y;
    public double z;
}

[System.Serializable]
public struct Urls
{
    public string glb;
    public string thumbnail;
}