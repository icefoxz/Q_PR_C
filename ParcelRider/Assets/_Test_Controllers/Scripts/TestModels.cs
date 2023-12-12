using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using OrderHelperLib.Dtos.DeliveryOrders;

[Serializable]
public class Model_Do
{
    public long Id;
    public string UserId;
    public Model_User User;
    public Info_Item ItemInfo;
    public string MyState;
    public Info_Sender SenderInfo;
    public Info_Receiver ReceiverInfo;
    public Info_Delivery DeliveryInfo;
    public string RiderId;
    public Model_Rider Rider;
    public Info_Payment PaymentInfo;
    public int Status;
    public int SubState;
    public Info_StageSegment[] StateHistory;

    public Model_Do FromDto(DeliverOrderModel m)
    {
        var jObj = JObject.FromObject(m);
        return jObj.ToObject<Model_Do>();
    }
}
[Serializable]
public class Model_User
{
    public string Id;
    public string Name;
    public string Username;
    public string Email;
    public string Phone;
    public string AvatarUrl;
    public Model_lingau Lingau;
}
[Serializable]
public class Model_lingau
{
    public string Id;
    public float Credit;
}
[Serializable]
public class Info_Item
{
    public float Weight;
    public int Quantity;
    public double Volume;
    public double Value;
    public float Length;
    public float Width;
    public float Height;
    public string Remark;
}
[Serializable]
public class Info_Sender
{
    public string UserId;
    public Model_User User;
    public string Name;
    public string PhoneNumber;
    public string NormalizedPhoneNumber;
}
[Serializable]
public class Info_Receiver
{
    public string Name;
    public string PhoneNumber;
}
[Serializable]
public class Info_Delivery
{
    public Info_Location StartLocation;
    public Info_Location EndLocation;
    public float Distance;
}
[Serializable]
public class Info_Location
{
    public string PlaceId;
    public string Address;
    public double Latitude;
    public double Longitude;
}
[Serializable]
public class Model_Rider
{
    public string Id;
    public string Name;
    public string Phone;
}
[Serializable]
public class Info_Payment
{
    public float Fee;
    public float Charge;
    public string Method;
    public string Reference;
    public string TransactionId;
    public bool IsReceived;
}
[Serializable]
public class Info_StageSegment
{
    public int SubState;
    public DateTime Timestamp;
    public string ImageUrl;
    public string Remark;
}