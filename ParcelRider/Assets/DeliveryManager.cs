using System;
using System.Collections.Generic;
using System.Linq;
using DataModel;
using UnityEngine;
using Views;
using Random = System.Random;

public class DeliveryManager : MonoBehaviour
{
    private static Random Random { get; } = new Random();
    [SerializeField] private GameObject LoginPage;
    [SerializeField] private View _newPackagePage;
    [SerializeField] private View _menuPage;
    [SerializeField] private View _orderViewPage;
    [SerializeField] private OrderField[] _fields;
    private NewPackagePage NewPackagePage { get; set; }
    private MenuPage MenuPage { get; set; }
    private OrderViewPage OrderViewPage { get; set; }
    private float Kg { get; set; }
    private float Km { get; set; }

    private OrderField[] Fields => _fields;
    private List<DeliveryOrder> Orders { get; set; } = new List<DeliveryOrder>();
    private List<UiBase> Pages { get; set; }
    private void Start()
    {
        for (int i = 0; i < Fields.Length; i++)
        {
            var field = Fields[i];
            Orders.Add(GetOrderFromField(field));
        }
        Init();
    }

    public void OpenMenuPage()
    {
        MenuPage.SetOrders(Orders.ToArray());
        OpenPage(MenuPage);
    }

    public void OpenNewPackagePage() => OpenPage(NewPackagePage);
    public void OpenOrderViewPage() => OpenPage(OrderViewPage);

    private void OpenPage(UiBase uiPage)
    {
        uiPage.Show();
        foreach (var page in Pages)
        {
            if(page == uiPage) continue;
            page.ResetUi();
            page.Hide();
        }
    }
    private DeliveryOrder GetOrderFromField(OrderField f)
    {
        var o = EntityBase.Instance<DeliveryOrder>();
        o.Distance = f.Distance;
        o.EndPoint = f.EndPoint;
        o.StartPoint = f.StartPoint;
        o.Status = f.Status;
        o.UserId = f.UserId;
        o.Price = GetCost(f.Weight, f.Distance);
        o.Id = 123582 + UnityEngine.Random.Range(0, 500);
        return o;
    }

    public void Init()
    {
        NewPackagePage = new NewPackagePage(v: _newPackagePage,
            onKgChanged: kg => Kg = kg,
            onKmChanged: km => Km = km,
            getCost: () => GetCost(Kg, Km));
        MenuPage = new MenuPage(_menuPage,OnOrderSelected);
        OrderViewPage = new OrderViewPage(_orderViewPage);
        NewPackagePage.Hide();
        MenuPage.Hide();
        OrderViewPage.Hide();
        LoginPage.gameObject.SetActive(true);
        Pages = new List<UiBase>
        {
            NewPackagePage,
            OrderViewPage
        };
    }

    private void OnOrderSelected(int orderId)
    {
        var order = Orders.FirstOrDefault(o => o.Id == orderId);
        OrderViewPage.Set(order);
        OpenOrderViewPage();
    }

    private float GetCost(float kg, float km)
    {
        if (kg <= 0 || km <= 0) return 0;

        var min = 5;
        var kgFare = kg switch
        {
            > 15 => 8 + 1.5f * kg,
            >= 5 => 8 + 1 * kg,
            _ => 8
        };
        var kmFare = km switch
        {
            > 10 => 5 + 0.8f * km,
            >= 5 => 5 + 1 * km,
            _ => 5
        };
        var fare = Math.Max(kgFare, kmFare);
        return Math.Max(fare, min);
    }

    public void ResetNewPackagePage() => NewPackagePage.ResetUi();

    public void PayAndGenerateOrder()
    {
        var order = NewPackagePage.GenerateOrder();
        order.Price = GetCost(Kg, Km);
        order.Weight = Kg;
        order.Distance = Km;
        Orders.Insert(0, order);
        MenuPage.SetOrders(Orders.ToArray());
    }

    [Serializable]private class OrderField
    {
        [SerializeField] private string from;
        [SerializeField] private string to;
        public int UserId { get; } = 128865;
        public string StartPoint => from;
        public string EndPoint => to;
        public float Distance { get; } = Random.Next(5, 80);
        public float Weight { get; } = Random.Next(1, 50);
        public float Price { get; }
        public int DeliveryManId { get;  }
        public int Status { get; } = Random.Next(0, 4);
    }
}