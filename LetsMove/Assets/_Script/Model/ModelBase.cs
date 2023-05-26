using Core;

namespace Model
{
    public class ModelBase
    {
        protected void SendEvent(string eventName, params object[] args) => App.MessagingManager.SendParams(eventName, args);
    }
}