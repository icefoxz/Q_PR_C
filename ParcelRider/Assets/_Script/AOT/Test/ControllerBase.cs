using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using AOT.Core;
using AOT.Model;
using AOT.Network;

namespace AOT.Test
{
    public class ControllerBase : IController
    {
        /// <summary>
        /// key = methodName, value = responseAction
        /// </summary>
        private Dictionary<string, Func<object[], object[]>> _testers = new Dictionary<string, Func<object[], object[]>>();
        public void RegTester(Func<object[], object[]> responseAction, string methodName)
        {
            if (methodName == null) throw new Exception("methodName is null!");
            _testers.Add(methodName, responseAction);
        }

        /// <summary>
        /// 支持测试的调用器, 会根据TestMode来判断是否调用真实的Api
        /// </summary>
        /// <param name="testConvertFunc"></param>
        /// <param name="testModeCallback"></param>
        /// <param name="reqAction"></param>
        /// <param name="methodName"></param>
        /// <exception cref="Exception"></exception>
        protected void Call<T>(Func<object[], T> testConvertFunc, Action<T> testModeCallback,
            Action reqAction, [CallerMemberName] string methodName = null) => Call(null, testConvertFunc, testModeCallback, reqAction, methodName);
        protected void Call<T>(object[] args, Func<object[], T> testConvertFunc, Action<T> testModeCallback,
        Action reqAction, [CallerMemberName] string methodName = null)
        {
            if (TestMode)
            {
                if (!_testers.TryGetValue(methodName, out var responseAction))
                    throw new Exception($"No tester found for {methodName}!");
                testModeCallback?.Invoke(testConvertFunc(responseAction(args)));
                return;
            }
            reqAction?.Invoke();
        }
        /// <summary>
        /// 如果index = -1, 则使用Model.PageIndex, 否则使用当前的pageIndex
        /// </summary>
        /// <param name="model"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        protected int ResolvePageIndex(DoPageModel model, int pageIndex) => pageIndex < 0 ? model.PageIndex : pageIndex;
        public void SetTestMode(bool isTestMode)
        {
            TestMode = isTestMode;
        }

        private bool TestMode { get; set; }
    }
}