using AOT.Core;

namespace AOT.Model
{
    public class ModelBase
    {
        protected void SendEvent(string eventName, params object[] args) => App.MessagingManager.SendParams(eventName, args);
    }
}