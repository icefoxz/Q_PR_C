using AOT.BaseUis;
using AOT.Controllers;
using AOT.Core;
using AOT.Views;
using UnityEngine;
using Visual.Pages.Rider;
using Visual.Sects;

namespace Visual.Pages
{
    public class Rider_UiManager : UiManagerBase
    {
        public enum ActivityPages
        {
            HomePage,
            ListPage,
            OrderPage,
            HistoryPage,
            ExceptionPage
        }
        [SerializeField] private View _accountSect;
        [SerializeField] private View _pageButtons;
        [SerializeField] private Page _orderListPage;
        //[SerializeField] private Page _orderViewPage;
        [SerializeField] private Page _riderLoginPage;
        [SerializeField] private Page _orderPage;
        [SerializeField] private Page _orderHistoryPage;
        [SerializeField] private Page _orderExceptionPage;
        [SerializeField] private Page _riderHomePage;
        [SerializeField] private Page _win_confirm;
        [SerializeField] private Page _win_message;
        [SerializeField] private Page _win_image;
        [SerializeField] private Page _win_account;

        private ConfirmWindow ConfirmWindow { get; set; }
        private MessageWindow MessageWindow { get; set; }
        private ImageWindow ImageWindow { get; set; }
        private RiderAccountWindow AccountWindow { get; set; }

        private View_AccountSect AccountSect { get; set; }
        private View_pageButtons View_pageButtons { get; set; }

        private RiderLoginPage RiderLoginPage { get; set; }
        private RiderJobListPage RiderJobListPage { get; set; }
        private RiderHistoryPage RiderHistoryPage { get; set; }
        private RiderHomePage RiderHomePage { get; set; }
    
        private RiderOrderViewPage RiderOrderViewPage { get; set; }
        private OrderExceptionPage OrderExceptionPage { get; set; }

        private RiderOrderController RiderOrderController => App.GetController<RiderOrderController>();

        public override void Init(bool startUi)
        {
            ConfirmWindow = new ConfirmWindow(_win_confirm, this);
            MessageWindow = new MessageWindow(_win_message, this);
            ImageWindow = new ImageWindow(_win_image, this);
            AccountWindow = new RiderAccountWindow(_win_account, this);

            AccountSect = new View_AccountSect(_accountSect, ShowAccountWindow, Logout);
            View_pageButtons = new View_pageButtons(v: _pageButtons,
                onJobsPageAction: () => ActivityPageSwitch(ActivityPages.ListPage),
                onHomePageAction: () => ActivityPageSwitch(ActivityPages.HomePage),
                onHistoryPageAction: () => ActivityPageSwitch(ActivityPages.HistoryPage));
            RiderHomePage = new RiderHomePage(_riderHomePage, 
                OrderCurrentSelected,
                () => ActivityPageSwitch(ActivityPages.ListPage),
                this);

            RiderHistoryPage = new RiderHistoryPage(_orderHistoryPage,
                onOrderSelectedAction: id =>
                {
                    OrderCurrentSelected(id);
                    ActivityPageSwitch(ActivityPages.OrderPage);
                }, this);
            RiderJobListPage = new RiderJobListPage(_orderListPage, OrderCurrentSelected, this);
            RiderLoginPage = new RiderLoginPage(_riderLoginPage, LoggedIn_InitHomePage, this);
            //OrderViewPage = new OrderViewPage(_orderViewPage, this);
            RiderOrderViewPage = new RiderOrderViewPage(_orderPage,
                onExceptionAction: OrderException, this);
            OrderExceptionPage = new OrderExceptionPage(_orderExceptionPage, this);
            if(startUi) RiderLoginPage.Show();
        }

        // Display OrderViewPage
        private void OrderCurrentSelected(string orderId)
        {
            RiderOrderController.ViewOrder(orderId);
            RiderOrderViewPage.ShowCurrentOrder();
        }

        // bottom page buttons
        private void ActivityPageSwitch(ActivityPages page)
        {
            Display(RiderHomePage, page == ActivityPages.HomePage);
            Display(RiderJobListPage, page == ActivityPages.ListPage);
            Display(RiderOrderViewPage, page == ActivityPages.OrderPage);
            Display(RiderHistoryPage, page == ActivityPages.HistoryPage);
            Display(OrderExceptionPage, page == ActivityPages.ExceptionPage);
            View_pageButtons.SetSelected(page);
            void Display(IUiBase p, bool display)
            {
                if (display) p.Show();
                else p.Hide();
            }
        }

        private void LoggedIn_InitHomePage()
        {
            RiderOrderController.Do_UpdateAll();
            ActivityPageSwitch(ActivityPages.HomePage);
        }

        private void ShowAccountWindow() => AccountWindow.Show();
        private void Logout()=> RiderLoginPage.Show();

        //OrderViewPage -> OrderExceptionPage
        private void OrderException(string orderId) => RiderOrderController.OrderException(orderId, options => OrderExceptionPage.SetOptions(orderId, options));

    }
}