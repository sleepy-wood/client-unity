using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using JetBrains.Annotations;

[Serializable]
public class ArrayMarketData
{
    public List<MarketData> marketData;
}

[Serializable]
public class MarketData
{
    public int id;
    public int amount;
    public string payment;
    public int userId;
    public string createdAt;
    public string updatedAt;
    public List<OrderDetails> orderDetails;
}

[Serializable]
public class OrderDetails
{
    public int id;
    public int orderId;
    public int productId;
    public string createdAt;
    public string updatedAt;
    public Product product;
}
[Serializable]
public class Product
{
    public int id;
    public string name;
    public string price;
    public int discount;
    public int stock;
    public string detail;
    public int hit;
    public int sell;
    public string category;
    public int userId;
    public string createdAt;
    public string updatedAt;
    public List<ProductImages> productImages;
}
[Serializable]
public class ProductImages
{
    public int id;
    public string filename;
    public string originalName;
    public string path;
    public string mimeType;
    public int size;
    public int productId;
    public string createdAt;
    public string updatedAt;
}