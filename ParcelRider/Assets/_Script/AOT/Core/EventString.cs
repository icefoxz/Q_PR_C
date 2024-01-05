namespace AOT.Core
{
    public static class EventString
    {
        public const string Rider_Login = "Rider_Login";
        public const string Rider_Logout = "Rider_Logout";
        public const string Rider_Update = "Rider_Update"; // Rider 更新

        public const string User_Login = "User_Login";
        public const string User_Logout = "User_Logout";
        public const string User_Update = "User_Update"; // User 更新

        public const string User_DoPay_Rider = "User_DoPay_Rider";
        public const string User_DoPay_Credit = "User_DoPay_Credit";
        
        public const string Order_Current_Set = "Order_Current_Update";
        public const string Order_Current_OptionsUpdate = "Order_Current_OptionsUpdate";

        public const string Orders_Assigned_Update = "Orders_Assigned_Update"; // order 列表更新
        public const string Orders_History_Update = "Orders_History_Update";
        public const string Orders_Unassigned_Update = "Orders_Unassigned_Update";
        public const string Rider_Do_StateUpdate = "Rider_Do_StateUpdate";
        public const string Rider_Do_GetUpdateImagesToken = "Rider_Do_GetUpdateImagesToken";
        public const string Rider_Image_Do = "Rider_Image_Do";
    }
}