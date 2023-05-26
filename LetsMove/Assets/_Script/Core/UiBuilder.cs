using System;
using System.Threading.Tasks;
using UnityEngine;
using Views;

namespace Core
{
    /// <summary>
    /// <see cref="IView"/>标准的生成器
    /// </summary>
    public class UiBuilder
    {
        private Canvas SceneCanvas { get; }
        private Res Res { get; }
        public UiBuilder(Canvas sceneCanvas, Res res)
        {
            SceneCanvas = sceneCanvas;
            Res = res;
        }
        /// <summary>
        /// 生成Ui
        /// </summary>
        /// <param name="resName">物体名字</param>
        /// <param name="buildAction">生成函数<see cref="IView"/>为传入物件</param>
        /// <param name="rootCanvas"></param>
        public async Task<T> Build<T>(string resName, Func<IView, T> buildAction, bool rootCanvas = true) =>
            await Build(resName, (_, v) => buildAction(v), rootCanvas);
        /// <summary>
        /// 生成Ui
        /// </summary>
        /// <param name="resName">物体名字</param>
        /// <param name="buildAction">生成函数<see cref="IView"/>为传入物件</param>
        /// /// <param name="callbackAfterInit">当初始化完成后执行</param>
        /// <param name="rootCanvas"></param>
        public void Build(string resName, Action<IView> buildAction, Action callbackAfterInit, bool rootCanvas = true) =>
            Build(resName, (_, v) => buildAction(v), callbackAfterInit, rootCanvas);
        /// <summary>
        /// 生成Ui
        /// </summary>
        /// <param name="resName">物体名字</param>
        /// <param name="buildAction">生成函数<see cref="IView"/>为传入物件</param>
        /// /// <param name="callbackAfterInit">当初始化完成后执行</param>
        /// <param name="parent"></param>
        public void Build(string resName, Action<IView> buildAction, Action callbackAfterInit, Transform parent) =>
            Build(resName, (_, v) => buildAction(v), callbackAfterInit, parent);
        /// <summary>
        /// 生成Ui
        /// </summary>
        /// <param name="resName">物体名字</param>
        /// <param name="buildAction">生成函数<see cref="GameObject"/>和<see cref="IView"/>为传入物件</param>
        /// /// <param name="callbackAfterInit">当初始化完成后执行</param>
        /// <param name="rootCanvas"></param>
        private void Build(string resName, Action<GameObject, IView> buildAction, Action callbackAfterInit, bool rootCanvas = true) =>
            Build(resName, rootCanvas ? SceneCanvas.transform : null, go =>
            {
                if (go.GetComponent<View>() is not IView v)
                    throw new NullReferenceException($"{go.name} is not View!");
                buildAction(go, v);
            }, callbackAfterInit);
        /// <summary>
        /// 生成Ui
        /// </summary>
        /// <param name="resName">物体名字</param>
        /// <param name="buildAction">生成函数<see cref="GameObject"/>和<see cref="IView"/>为传入物件</param>
        /// <param name="rootCanvas"></param>
        private async Task<T> Build<T>(string resName, Func<GameObject, IView, T> buildAction, bool rootCanvas = true) =>
            await Build(resName, rootCanvas ? SceneCanvas.transform : null, go =>
            {
                if (go.GetComponent<View>() is not IView v)
                    throw new NullReferenceException($"{go.name} is not View!");
                return buildAction(go, v);
            });

        /// <summary>
        /// 生成Ui
        /// </summary>
        /// <param name="resName">物体名字</param>
        /// <param name="buildAction">生成函数<see cref="GameObject"/>和<see cref="IView"/>为传入物件</param>
        /// <param name="callbackAfterInit">当初始化完成后执行</param>
        /// <param name="parent"></param>
        private void Build(string resName, Action<GameObject, IView> buildAction, Action callbackAfterInit, Transform parent) =>
            Build(resName, parent, go =>
            {
                if (go.GetComponent<View>() is not IView v)
                    throw new NullReferenceException($"{go.name} is not View!");
                buildAction(go, v);
            }, callbackAfterInit);

        /// <summary>
        /// 生成Ui
        /// </summary>
        /// <param name="resName">物体名字</param>
        /// <param name="buildAction">生成函数<see cref="GameObject"/>为传入物件</param>
        /// <param name="callbackAfterInit">当初始化完成后执行</param>
        /// <param name="rootCanvas"></param>
        private void Build(string resName, Action<GameObject> buildAction, Action callbackAfterInit,
            bool rootCanvas = true) =>
            Build(resName, rootCanvas ? SceneCanvas.transform : null, buildAction, callbackAfterInit);

        private async void Build(string resName, Transform parent, Action<GameObject> buildAction, Action callbackAfterInit)
        {
            var obj = await Res.InstantiateAsync(resName, parent);
            buildAction.Invoke(obj);
            callbackAfterInit?.Invoke();
        }
        private async Task<T> Build<T>(string resName, Transform parent, Func<GameObject, T> buildAction)
        {
            var obj = await Res.InstantiateAsync(resName, parent);
            return buildAction.Invoke(obj);
        }
    }
}