﻿using System.Collections.Generic;
using System.Linq;
using Domain;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [ExecuteInEditMode]
    // ReSharper disable once RequiredBaseTypesIsNotInherited
    public class MobileScreenResolutionsWindow : EditorWindow
    {
        private List<Phone> _phones = new List<Phone>();

        [MenuItem("Window/Mobile Screen Resolutions/Open window")]
        private static void Init()
        {
            GetWindow(typeof(MobileScreenResolutionsWindow), false, "Resolutions").Show();
        }

        [MenuItem("Window/Mobile Screen Resolutions/Set next #n")]
        private static void Next()
        {
            GameViewUtils.SetNext();
        }

        [MenuItem("Window/Mobile Screen Resolutions/Set previous #p")]
        private static void Previous()
        {
            GameViewUtils.SetPrevious();
        }

        private void OnEnable()
        {
            LoadJson();
        }

        private void LoadJson()
        {
            var resolutionsJson = Resources.Load<TextAsset>("resolutions");
            var phones = JsonUtility.FromJson<Phones>(resolutionsJson.text);

            _phones = phones.CommonPhones;
        }

        private void OnGUI()
        {
#if !UNITY_ANDROID && !UNITY_IOS
            EditorGUILayout.HelpBox("Please choose Android or iOS", MessageType.Warning);
            return;
#endif
            PrintAddAllResolutionsPanel();
            PrintDeleteAllResolutions();
            PrintAllPhones();
        }

        private void PrintAddAllResolutionsPanel()
        {
            var guiStyle = new GUIStyle() {wordWrap = true, padding = new RectOffset(10, 10, 10, 0)};
            GUILayout.Label("Add all the following resolutions to the dropdown in the game window.", guiStyle);

            if (GUILayout.Button("Add resolutions"))
            {
                foreach (var phone in _phones)
                {
                    Resize(phone.Resolution.Width, phone.Resolution.Height, phone.Name);
                }
            }
        }

        private void PrintDeleteAllResolutions()
        {        
            var guiStyle = new GUIStyle() {wordWrap = true, padding = new RectOffset(10, 10, 10, 0)};
            GUILayout.Label("Remove all the user-resolutions of the dropdown in the game  .", guiStyle);
            
            if (GUILayout.Button("Remove resolutions"))
            {
                GameViewUtils.RemoveResolutions();
            }
        }

        private void PrintAllPhones()
        {
            GUILayout.Label("Devices", EditorStyles.boldLabel);

            _phones = _phones.OrderBy(phone => phone.Resolution.Get()).ToList();

            var importantResolutions = new List<Phone>() {_phones[0], CalculateTheMedium(), _phones[_phones.Count - 1]};

            DisplayImportantResolution("Smaller", importantResolutions[0]);
            DisplayImportantResolution("Medium", importantResolutions[1]);
            DisplayImportantResolution("Bigger", importantResolutions[2]);

            GUILayout.Label("Others", EditorStyles.boldLabel);
            var otherResolutions = new List<Phone>();
            otherResolutions.AddRange(_phones.Except(importantResolutions));
            otherResolutions.ForEach(DisplayButton);
        }

        private Phone CalculateTheMedium()
        {
            var smallerResolution = _phones[0].Resolution;
            var biggestResolution = _phones[_phones.Count - 1].Resolution;
            var exactlyMiddleResolution = (smallerResolution.Get() + biggestResolution.Get()) / 2.0f;
            for (int i = 1; i < _phones.Count - 2; i++)
            {
                if (Mathf.Abs(exactlyMiddleResolution - _phones[i].Resolution.Get()) <
                    Mathf.Abs(exactlyMiddleResolution - _phones[i + 1].Resolution.Get()))
                {
                    return _phones[i];
                }
            }

            return _phones[_phones.Count / 2];
        }

        private void DisplayImportantResolution(string text, Phone phone)
        {
            GUILayout.Label(text);
            DisplayButton(phone);
        }

        private void DisplayButton(Phone phone)
        {
            DisplayButton(phone.Name, phone.Resolution.Width, phone.Resolution.Height, phone.Tooltip);
        }

        private void DisplayButton(string text, int width, int height, string tooltip)
        {
            var guiContent = new GUIContent(text + " " + width + "x" + height, tooltip);
            var guiStyle = new GUIStyle("button") {alignment = TextAnchor.MiddleLeft};

            if (GUILayout.Button(guiContent, guiStyle))
                Resize(width, height, text);
        }

        private void Resize(int width, int height, string text)
        {
            // TODO Refactor to not search twice.
            var index = GameViewUtils.FindSize(width, height);
            if (index == -1)
                GameViewUtils.AddCustomSize(width, height, text);

            GameViewUtils.SetSize(GameViewUtils.FindSize(width, height));
        }
    }
}