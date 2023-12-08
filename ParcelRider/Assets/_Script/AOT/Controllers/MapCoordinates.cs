using System;
using AOT.BaseUis;
using AOT.Views;
using Mapbox.Unity.Map;
using Mapbox.Utils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public interface IMapCoordinates
{
    Vector2d GetMapPointCoordinates();
    void StartMap(double lat, double lng, UnityAction<(double lat, double lng)> onGeoCallbackAction);
    void Display(bool display);
}

public class MapCoordinates : MonoBehaviour, IMapCoordinates
{
    private const int Map_Zoom_Min = 5;
    private const int Map_Zoom_Max = 18;
    private const int Map_Zoom_Init = 12;
    private static readonly Vector2d MalaysiaCo = new Vector2d(4.2105, 101.9758);
    [SerializeField] private AbstractMap _map;
    [SerializeField] private Transform _mapPoint;
    [SerializeField] private Camera _camera;
    [SerializeField] private View _view_map;

    [SerializeField] private float zoomSensitivity = 0.1f; // 缩放敏感度
    [SerializeField] private float panSensitivity = 0.1f;  // 平移敏感度

    private View_Map View_map { get; set; }
    private GestureHandler _gestureHandler;
    private event UnityAction<(double lat, double lng)> OnLocationSet;

    public void Init(GestureHandler gestureHandler)
    {
        _gestureHandler = gestureHandler;
        _gestureHandler.OnDrag.AddListener(PanCamera);
        _gestureHandler.OnZoom.AddListener(Zoom);
        _gestureHandler.OnDragEnd.AddListener(SetCurrentMapPointToCoordinates);
        _gestureHandler.OnZoomEnd.AddListener(SetCurrentMapPointToCoordinates);
        View_map = new View_Map(_view_map, ConfirmCurrentMapPoint, CloseMap, false);
    }

    private void CloseMap() => Display(false);

    private void ConfirmCurrentMapPoint()
    {
        var co = _map.CenterLatitudeLongitude;
        OnLocationSet?.Invoke((co.x, co.y));
        CloseMap();
    }

    public Vector2d GetMapPointCoordinates()
    {
        var pos = _camera.ScreenToWorldPoint(_mapPoint.position);
        // 获取地图中心点的经纬度
        var centerCoordinates = _map.WorldToGeoPosition(pos);
        return centerCoordinates;
    }

    private void SetLocation(double lat, double lng)
    {
        ResetCameraPos();
        _map.UpdateMap(new Vector2d(lat, lng));
    }

    private void PanCamera(Vector2 delta)
    {
        var pos = delta * panSensitivity;
        var cameraMovement = new Vector3(-pos.x, 0, -pos.y);
        _camera.transform.Translate(cameraMovement, Space.World);
    }

    private void SetCurrentMapPointToCoordinates()
    {
        var c = _camera.ScreenToWorldPoint(_mapPoint.position);
        var co = _map.WorldToGeoPosition(new Vector3(c.x, 0, c.z));
        SetLocation(co.x, co.y);
    }

    private void ResetCameraPos()
    {
        var cPos = _camera.transform.position;
        _camera.transform.position = new Vector3(0, cPos.y, 0);
    }

    private void Zoom(float distance)
    {
        var zoom = (distance * zoomSensitivity) + _map.Zoom;
        zoom = zoom switch
        {
            < Map_Zoom_Min => Map_Zoom_Min,
            > Map_Zoom_Max => Map_Zoom_Max,
            _ => zoom
        };
        if (Math.Abs(zoom - _map.Zoom) > 0.00001f)
        {
            _map.UpdateMap(zoom);
        }
    }

    public void Display(bool display) => gameObject.SetActive(display);

    public void StartMap(double lat,double lng,UnityAction<(double lat, double lng)> onGeoCallbackAction)
    {
        Display(true);
        if (lat == 0 && lng == 0)
        {
            lat = MalaysiaCo.x;
            lng = MalaysiaCo.y;
        }
        _map.Initialize(new Vector2d(lat, lng), Map_Zoom_Init);
        View_map.Show();
        OnLocationSet = onGeoCallbackAction;
    }

    private class View_Map : UiBase
    {
        private Button btn_set;
        private Button btn_x;
        public View_Map(IView v,UnityAction onSetAction, UnityAction onCloseAction ,bool display = true) : base(v, display)
        {
            btn_set = v.Get<Button>("btn_set");
            btn_x = v.Get<Button>("btn_x");
            btn_set.onClick.AddListener(onSetAction);
            btn_x.onClick.AddListener(onCloseAction);
        }
    }
}
