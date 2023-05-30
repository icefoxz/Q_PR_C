using Controllers;
using Core;
using UnityEngine;
using Views;
using Visual.Pages.Rider;

public class RiderUiManager : UiManagerBase
{
    private enum ActivityPages
    {
        ListPage,
        OrderPage,
        HistoryPage,
        ExceptionPage
    }
    [SerializeField] private View _accountSect;
    [SerializeField] private View _pageButtons;
    [SerializeField] private Page _orderListPage;
    [SerializeField] private Page _orderViewPage;
    [SerializeField] private Page _riderLoginPage;
    [SerializeField] private Page _orderPage;
    [SerializeField] private Page _orderHistoryPage;
    [SerializeField] private Page _orderExceptionPage;

    private View_AccountSect AccountSect { get; set; }
    private View_pageButtons View_pageButtons { get; set; }

    private RiderLoginPage RiderLoginPage { get; set; }
    private DoListPage OrderListPage { get; set; }
    private DoListPage OrderHistoryPage { get; set; }
    //private OrderViewPage OrderViewPage { get; set; }
    private OrderPage OrderPage { get; set; }
    private OrderExceptionPage OrderExceptionPage { get; set; }

    private RiderController RiderController => App.GetController<RiderController>();
    private OrderController OrderController => App.GetController<OrderController>();

    public override void Init(bool startUi)
    {
        AccountSect = new View_AccountSect(_accountSect, AccountBtnClick_ShowAccountSect, Logout);
        View_pageButtons = new View_pageButtons(v: _pageButtons, 
            onHomePageAction: ()=>ActivityPageSwitch(ActivityPages.ListPage),
            onHistoryPageAction: () => ActivityPageSwitch(ActivityPages.HistoryPage));

        OrderHistoryPage = new DoListPage(_orderHistoryPage,
            onOrderSelectedAction: id =>
            {
                OrderPage.Set(OrderController.GetOrder(id));
                ActivityPageSwitch(ActivityPages.OrderPage);
            }, this);
        OrderListPage = new DoListPage(_orderListPage, id => OrderPage.Set(OrderController.GetOrder(id)), this);
        RiderLoginPage = new RiderLoginPage(_riderLoginPage, LoggedIn_InitRiderPage, this);
        //OrderViewPage = new OrderViewPage(_orderViewPage, this);
        OrderPage = new OrderPage(_orderPage,
            onTakeOrderAction: TakeOrder_ApiReq,
            onPickItemAction: PickItem_ApiReq,
            onCollectionAction: Collection_ApiReq,
            onCompletedAction: Complete_ApiReq,
            onExceptionAction: OrderException,this);
        OrderExceptionPage = new OrderExceptionPage(_orderExceptionPage, ExceptionOptionSelected, this);
        if(startUi) RiderLoginPage.Show();
    }

    private void ActivityPageSwitch(ActivityPages page)
    {
        Display(OrderListPage, page == ActivityPages.ListPage);
        Display(OrderPage, page == ActivityPages.OrderPage);
        Display(OrderHistoryPage, page == ActivityPages.HistoryPage);
        Display(OrderExceptionPage, page == ActivityPages.ExceptionPage);

        void Display(PageUiBase p, bool display)
        {
            if (display) p.Show();
            else p.Hide();
        }
    }

    private void AccountBtnClick_ShowAccountSect() => AccountSect.Show();

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
        RiderController.OrderException(orderId, options => OrderExceptionPage.SetOptions(orderId, options));
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