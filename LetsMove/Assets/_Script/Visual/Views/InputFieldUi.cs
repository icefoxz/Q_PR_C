using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// InputField扩展版, 主要实现<see cref="OnSelectEvent"/>和<see cref="OnDeselectEvent"/>
/// </summary>
public class InputFieldUi : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    private InputField _inputField;
    public InputField InputField => _inputField ??= GetComponent<InputField>();
    public UnityEvent<BaseEventData> OnSelectEvent { get; } = new UnityEvent<BaseEventData>();
    public UnityEvent<BaseEventData> OnDeselectEvent { get; } = new UnityEvent<BaseEventData>();
    // 当 InputField 获得焦点时调用
    public void OnSelect(BaseEventData eventData) => OnSelectEvent.Invoke(eventData);
    // 当 InputField 失去焦点时调用
    public void OnDeselect(BaseEventData eventData) => OnDeselectEvent.Invoke(eventData);
}