namespace AOT.Core
{
    public static class EventString
    {
        public const string Rider_Update = "Rider_Update"; // Rider 更新
        public const string User_Update = "User_Update"; // User 更新

        public const string Order_Assigned_Current_Update = "Order_Assigned_Current_Update"; // (单个选中)Order 更新
        public const string Order_History_Current_Update = "Order_History_Current_Update";
        public const string Order_Unassigned_Current_Update = "Order_Unassigned_Current_Update";

        public const string Orders_Assigned_Update = "Orders_Assigned_Update"; // order 列表更新
        public const string Orders_History_Update = "Orders_History_Update";
        public const string Orders_Unassigned_Update = "Orders_Unassigned_Update";
    }
}