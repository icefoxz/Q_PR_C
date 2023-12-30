using System.Collections;
using AOT.BaseUis;
using AOT.Controllers;
using AOT.Core;
using AOT.Views;
using Cysharp.Threading.Tasks;
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
            UnassignedPage,
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
        [SerializeField] private Page _win_reqRetry;

        private ConfirmWindow ConfirmWindow { get; set; }
        private MessageWindow MessageWindow { get; set; }
        private ImageWindow ImageWindow { get; set; }
        private RiderAccountWindow AccountWindow { get; set; }
        private ReqRetryWindow ReqRetryWindow { get; set; }

        private View_AccountSect AccountSect { get; set; }
        private View_pageButtons View_pageButtons { get; set; }

        private RiderLoginPage RiderLoginPage { get; set; }
        private RiderJobListPage RiderJobListPage { get; set; }
        private RiderHistoryPage RiderHistoryPage { get; set; }
        private RiderHomePage RiderHomePage { get; set; }
    
        private RiderOrderViewPage RiderOrderViewPage { get; set; }
        private OrderExceptionPage OrderExceptionPage { get; set; }

        private RiderOrderController RiderOrderController => App.GetController<RiderOrderController>();
        private RiderLoginController RiderLoginController => App.GetController<RiderLoginController>();

        public override void Init(bool startUi)
        {
            base.Init(startUi);
            ConfirmWindow = new ConfirmWindow(_win_confirm);
            MessageWindow = new MessageWindow(_win_message);
            ImageWindow = new ImageWindow(_win_image);
            AccountWindow = new RiderAccountWindow(_win_account);
            ReqRetryWindow = new ReqRetryWindow(_win_reqRetry);

            AccountSect = new View_AccountSect(_accountSect, SetProfile, RiderLoginController.Logout);
            View_pageButtons = new View_pageButtons(v: _pageButtons,
                onJobsPageAction: () => ActivityPageSwitch(ActivityPages.UnassignedPage),
                onHomePageAction: () => ActivityPageSwitch(ActivityPages.HomePage),
                onHistoryPageAction: () => ActivityPageSwitch(ActivityPages.HistoryPage));
            RiderHomePage = new RiderHomePage(_riderHomePage, 
                OrderCurrentSelected,
                () => ActivityPageSwitch(ActivityPages.UnassignedPage),
                this);

            RiderHistoryPage = new RiderHistoryPage(_orderHistoryPage,
                onOrderSelectedAction: OrderCurrentSelected, this);
            RiderJobListPage = new RiderJobListPage(_orderListPage, OrderCurrentSelected, this);
            RiderLoginPage = new RiderLoginPage(_riderLoginPage, onLoggedInAction: LoggedIn_InitHomePage, this);
            //OrderViewPage = new OrderViewPage(_orderViewPage, this);
            OrderExceptionPage = new OrderExceptionPage(_orderExceptionPage, this);
            RiderOrderViewPage = new RiderOrderViewPage(_orderPage, OrderExceptionPage.DisplayPossibleExceptions, this);
            if(startUi) RiderLoginPage.Show();
        }

        private void OrderCurrentSelected(long orderId) => RiderOrderController.Do_Current_Set(orderId);

        // bottom page buttons
        private void ActivityPageSwitch(ActivityPages page)
        {
            Display(RiderHomePage, page == ActivityPages.HomePage);
            Display(RiderJobListPage, page == ActivityPages.UnassignedPage);
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
            StartCoroutine(RiderOrderController.OnLoginLoadTask().ToCoroutine());
            ActivityPageSwitch(ActivityPages.HomePage);
        }


        private void SetProfile() => AccountWindow.Show();

    }
}