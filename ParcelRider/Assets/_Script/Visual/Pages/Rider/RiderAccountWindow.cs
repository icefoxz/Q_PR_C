using System;
using System.Collections;
using Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Views;

namespace Visual.Pages.Rider
{
    internal class RiderAccountWindow : WinUiBase
    {
        private Button btn_close { get; }
        private Text text_lastLogin { get; }
        private Element_input element_inputName { get; }
        private Element_input element_inputPhone { get; }
        private View_avatar view_avatar { get; }

        public RiderAccountWindow(IView v, UiManagerBase uiManager) : base(v, uiManager)
        {
            btn_close = v.GetObject<Button>("btn_close");
            text_lastLogin = v.GetObject<Text>("text_lastLogin");
            element_inputName = new Element_input(v.GetObject<View>("element_inputName"));
            element_inputPhone = new Element_input(v.GetObject<View>("element_inputPhone"));
            view_avatar = new View_avatar(v.GetObject<View>("view_avatar"), () => { });
            btn_close.OnClickAdd(Hide);
        }

        private class Element_input : UiBase
        {
            private View_switch view_switch { get; }
            private Text text_label { get; }
            public string Value => text_label.text;
            public Element_input(IView v, bool display = true) : base(v, display)
            {
                text_label = v.GetObject<Text>("text_label");
                view_switch = new View_switch(v.GetObject<View>("view_switch"), SetText);
            }

            public void SetText(string text)
            {
                text_label.text = text;
                view_switch.SetPlaceholder(text);
            }

            private class View_switch : UiBase
            {
                private GameObject obj_input { get; }
                private InputField input_value { get; }
                private Button btn_confirm { get; }
                private Button btn_edit { get; }

                public View_switch(IView v, Action<string> onConfirmAction, bool display = true) : base(v, display)
                {
                    obj_input = v.GetObject("obj_input");
                    input_value = v.GetObject<InputField>("input_value");
                    btn_edit = v.GetObject<Button>("btn_edit");
                    btn_confirm = v.GetObject<Button>("btn_confirm");

                    input_value.onEndEdit.AddListener(arg => DelayEndEditMode());
                    btn_confirm.OnClickAdd(() =>
                    {
                        Switch(false);
                        onConfirmAction(input_value.text);
                    });
                    btn_edit.OnClickAdd(() => Switch(true));
                }

                private void DelayEndEditMode()
                {
                    App.MonoService.StartCoroutine(DelayEndEdit());

                    IEnumerator DelayEndEdit()
                    {
                        yield return new WaitForSeconds(0.2f);
                        yield return new WaitUntil(() =>
                            EventSystem.current.currentSelectedGameObject != btn_confirm.gameObject);
                        Switch(false);
                    }
                }

                private void Switch(bool editMode)
                {
                    obj_input.SetActive(editMode);
                    btn_edit.gameObject.SetActive(!editMode);
                    input_value.caretPosition = input_value.text.Length;
                    input_value.ActivateInputField();
                    App.MonoService.StartCoroutine(DelayEventSelectObject());
                }

                private IEnumerator DelayEventSelectObject()
                {
                    yield return null;
                    EventSystem.current.SetSelectedGameObject(input_value.gameObject);
                }

                public void SetPlaceholder(string placeholder) => input_value.placeholder.GetComponent<Text>().text = placeholder;
            }
        }

        private class View_avatar : UiBase
        {
            private Element element_user { get; }
            //private Element element_rider { get; }

            public View_avatar(IView v, Action onUserSelected, bool display = true)
                : base(v, display)
            {
                element_user = new Element(v.GetObject<View>("element_user"), onUserSelected);
                //element_rider = new Element(v.GetObject<View>("element_rider"), onRiderSelected);
            }

            public void UpdateAvatars()
            {
                element_user.Show();
                element_user.SetSelected(true);
            }

            private class Element : UiBase
            {
                private Button btn_avatar { get; }
                private Image img_selected { get; }
                public Element(IView v, Action onClickAction, bool display = true) : base(v, display)
                {
                    btn_avatar = v.GetObject<Button>("btn_avatar");
                    btn_avatar.OnClickAdd(onClickAction);
                    img_selected = v.GetObject<Image>("img_selected");
                }

                public void SetSelected(bool selected) => img_selected.gameObject.SetActive(selected);
            }

        }
    }
}