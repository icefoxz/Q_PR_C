using Controllers;
using Core;
using UnityEngine;
using Views;
using Visual.Pages.Rider;

public class RiderUiManager : UiManagerBase
{
    [SerializeField] private View _accountSect;
    [SerializeField] private View _pageButtons;
    [SerializeField] private Page _orderListPage;
    [SerializeField] private Page _orderViewPage;
    [SerializeField] private Page _riderLoginPage;
    [SerializeField] private Page _orderPage;
    [SerializeField] private Page _orderHistoryPage;

    private View_AccountSect AccountSect { get; set; }
    private View_pageButtons View_pageButtons { get; set; }

    private RiderLoginPage RiderLoginPage { get; set; }
    private DoListPage OrderListPage { get; set; }
    private DoListPage OrderHistoryPage { get; set; }
    private OrderViewPage OrderViewPage { get; set; }
    private OrderPage OrderPage { get; set; }

    private RiderController RiderController => App.GetController<RiderController>();
    private OrderController OrderController => App.GetController<OrderController>();

    public override void Init(bool startUi)
    {
        AccountSect = new View_AccountSect(_accountSect, ()=> AccountSect.Show(), Logout);
        View_pageButtons = new View_pageButtons(v: _pageButtons, onHomePageAction: () => OrderListPage.Show(),
            onHistoryPageAction: () => OrderHistoryPage.Show());

        OrderHistoryPage = new DoListPage(_orderHistoryPage, id => OrderPage.Set(OrderController.GetOrder(id)), this);
        OrderListPage = new DoListPage(_orderListPage, id => OrderPage.Set(OrderController.GetOrder(id)), this);
        RiderLoginPage = new RiderLoginPage(_riderLoginPage, LoggedIn_InitRiderPage, this);
        OrderViewPage = new OrderViewPage(_orderViewPage, this);
        OrderPage = new OrderPage(_orderPage,
            onTakeOrderAction: TakeOrder_ApiReq,
            onPickItemAction: PickItem_ApiReq,
            onCollectionAction: Collection_ApiReq,
            onCompletedAction: Complete_ApiReq,
            onExceptionAction: OrderException,
            onExceptionOpSelectedAction: ExceptionOptionSelected, this);
        if(startUi) RiderLoginPage.Show();
    }

    private void LoggedIn_InitRiderPage() => OrderListPage.Show();
    private void Logout()=> RiderLoginPage.Show();

    private void ExceptionOptionSelected(string orderId, int optionIndex)
    {
        RiderController.SetException(orderId, optionIndex, () =>
        {
            var o = OrderController.GetOrder(orderId);
            OrderPage.Set(o);
        });
    }

    private void OrderException(string orderId)
    {
        RiderController.OrderException(orderId, options => OrderPage.SetExceptionOption(options));
    }

    private void Complete_ApiReq(string orderId)
    {
        RiderController.Complete(orderId, () =>
        {
            var o = OrderController.GetOrder(orderId);
            OrderPage.Set(o);
        });
    }

    private void Collection_ApiReq(string orderId)
    {
        RiderController.ItemCollection(orderId, () =>
        {
            var o = OrderController.GetOrder(orderId);
            OrderPage.Set(o);
        });
    }

    private void PickItem_ApiReq(string orderId)
    {
        RiderController.PickItem(orderId, () =>
        {
            var o = OrderController.GetOrder(orderId);
            OrderPage.Set(o);
        });
    }

    private void TakeOrder_ApiReq(string orderId)
    {
        RiderController.TakeOrder(orderId, () =>
        {
            OrderController.AssignRider(orderId, success =>
            {
                if (success)
                {
                    var o = OrderController.GetOrder(orderId);
                    OrderPage.Set(o);
                    return;
                }

                Debug.LogError("AssignRider failed");
            });
        });
    }
}