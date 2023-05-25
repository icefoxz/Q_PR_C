using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// 抖动控制器
/// </summary>
public class Debouncer
{
    //抖动时间
    private float debounceTime;
    //是否在抖动
    private bool isWaitingForDebounce = false;
    private MonoBehaviour mono;

    public Debouncer(float debounceTime, MonoBehaviour monoBehaviour)
    {
        this.debounceTime = debounceTime;
        mono = monoBehaviour;
    }

    public void Debounce(string input, Action<string> action)
    {
        if (string.IsNullOrWhiteSpace(input)) return;
        if (isWaitingForDebounce)
        {
            mono.StopCoroutine(DebouncedAction());
        }

        isWaitingForDebounce = true;
        mono.StartCoroutine(DebouncedAction());

        IEnumerator DebouncedAction()
        {
            yield return new WaitForSeconds(debounceTime);
            isWaitingForDebounce = false;
            action(input);
        }
    }
    public void Debounce(string input, Func<string,IEnumerator> action)
    {
        if (string.IsNullOrWhiteSpace(input)) return;
        if (isWaitingForDebounce)
        {
            mono.StopCoroutine(DebouncedAction());
        }

        isWaitingForDebounce = true;
        mono.StartCoroutine(DebouncedAction());

        IEnumerator DebouncedAction()
        {
            yield return new WaitForSeconds(debounceTime);
            isWaitingForDebounce = false;
            yield return action(input);
        }
    }
}