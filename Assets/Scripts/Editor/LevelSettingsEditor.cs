using UnityEngine;
using UnityEditor;
using SlimeRpgEvolution2D.Data;

namespace SlimeRpgEvolution2D.EditorScripts
{
    [CustomEditor(typeof(LevelSettings))]
    public class LevelSettingsEditor : Editor
    {
        private int _currentPageIndex = 0;
        private bool _showUnlockRequirements = true; // Сворачиваемая шторка для красоты

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            LevelSettings levelSettings = (LevelSettings)target;

            // 1. РИСУЕМ ГЛОБАЛЬНЫЕ НАСТРОЙКИ ЛОКАЦИИ (Всегда сверху)
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("ГЛОБАЛЬНЫЕ НАСТРОЙКИ МИРА", EditorStyles.boldLabel);

            // ИСПРАВЛЕНО: Ищем реальное имя приватного поля "worldID" вместо свойства "ID"
            EditorGUILayout.PropertyField(serializedObject.FindProperty("worldID"), new GUIContent("ID Локации"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("worldName"), new GUIContent("Название Локации"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("worldIcon"), new GUIContent("Мини-иконка локации"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("stageBackground"), new GUIContent("Задний фон локации"));

            EditorGUILayout.Space(10);
            SerializedProperty unlockReqProp = serializedObject.FindProperty("_unlockRequirement");
            if (unlockReqProp != null)
            {
                _showUnlockRequirements = EditorGUILayout.Foldout(_showUnlockRequirements, "Условия Разблокировки Мира", true, EditorStyles.foldoutHeader);

                if (_showUnlockRequirements)
                {
                    EditorGUI.indentLevel++;

                    // 0. Золото рисуем всегда (базовое требование, если 0 — бесплатно)
                    EditorGUILayout.PropertyField(unlockReqProp.FindPropertyRelative("unlockCost"), new GUIContent("Цена золота"));
                    EditorGUILayout.Space(5);

                    // --- ВКЛАДКА 1: ЭТАПЫ ЛОКАЦИЙ ---
                    SerializedProperty useStageProp = unlockReqProp.FindPropertyRelative("useStageRequirement");
                    useStageProp.boolValue = EditorGUILayout.ToggleLeft(" Включить требования по Этапам Миров", useStageProp.boolValue, EditorStyles.boldLabel);
                    if (useStageProp.boolValue)
                    {
                        EditorGUI.indentLevel++;
                        SerializedProperty stageListProp = unlockReqProp.FindPropertyRelative("stageRequirements");
                        if (stageListProp != null)
                        {
                            // Unity сам красиво отрисует список структур, включая новое поле "requiredWaveNumber"
                            EditorGUILayout.PropertyField(stageListProp, new GUIContent("Список требуемых этапов"), true);
                        }
                        EditorGUI.indentLevel--;
                    }


                    EditorGUILayout.Space(5);

                    // --- ВКЛАДКА 2: ОРУЖИЕ (БЕСКОНЕЧНЫЙ СПИСОК) ---
                    SerializedProperty useWeaponProp = unlockReqProp.FindPropertyRelative("useWeaponRequirement");
                    useWeaponProp.boolValue = EditorGUILayout.ToggleLeft(" Включить требования по Оружию", useWeaponProp.boolValue, EditorStyles.boldLabel);
                    if (useWeaponProp.boolValue)
                    {
                        EditorGUI.indentLevel++;
                        // ИСПРАВЛЕНО: Отрисовываем массив weaponRequirements
                        SerializedProperty weaponListProp = unlockReqProp.FindPropertyRelative("weaponRequirements");
                        if (weaponListProp != null)
                        {
                            EditorGUILayout.PropertyField(weaponListProp, new GUIContent("Список требуемого оружия"), true);
                        }
                        EditorGUI.indentLevel--;
                    }
                    EditorGUILayout.Space(5);

                    // --- ВКЛАДКА 3: ПРЕДМЕТЫ (БЕСКОНЕЧНЫЙ СПИСОК) ---
                    SerializedProperty useItemProp = unlockReqProp.FindPropertyRelative("useItemRequirement");
                    useItemProp.boolValue = EditorGUILayout.ToggleLeft(" Включить требования по Предметам", useItemProp.boolValue, EditorStyles.boldLabel);
                    if (useItemProp.boolValue)
                    {
                        EditorGUI.indentLevel++;
                        // ИСПРАВЛЕНО: Отрисовываем массив itemRequirements
                        SerializedProperty itemListProp = unlockReqProp.FindPropertyRelative("itemRequirements");
                        if (itemListProp != null)
                        {
                            EditorGUILayout.PropertyField(itemListProp, new GUIContent("Список требуемых предметов"), true);
                        }
                        EditorGUI.indentLevel--;
                    }

                    EditorGUI.indentLevel--;
                }
            }

            // НЕ ЗАБЫВАЙТЕ ЭТУ СТРОЧКУ В САМОМ КОНЦЕ МЕТОДА OnInspectorGUI!
            serializedObject.ApplyModifiedProperties();




            EditorGUILayout.Space(5);



            SerializedProperty stagesProp = serializedObject.FindProperty("stages");

            if (stagesProp == null || stagesProp.arraySize == 0)
            {
                EditorGUILayout.HelpBox("На этой локации еще нет этапов. Нажмите кнопку ниже, чтобы добавить первый этап!", MessageType.Info);
                if (GUILayout.Button("Добавить новый этап (Страницу)", GUILayout.Height(30)))
                {
                    stagesProp.arraySize++;
                    serializedObject.ApplyModifiedProperties();
                }
                return;
            }

            if (_currentPageIndex >= stagesProp.arraySize)
            {
                _currentPageIndex = stagesProp.arraySize - 1;
            }

            EditorGUILayout.Space(15);

            // 2. ПАНЕЛЬ НАВИГАЦИИ ПО КНИГЕ
            EditorGUILayout.BeginHorizontal();

            GUI.enabled = _currentPageIndex > 0;
            if (GUILayout.Button("◀ Назад", GUILayout.Height(25)))
            {
                _currentPageIndex--;
            }
            GUI.enabled = true;

            string pageText = $"ЭТАП {_currentPageIndex + 1} из {stagesProp.arraySize}";
            GUILayout.Label(pageText, EditorStyles.centeredGreyMiniLabel, GUILayout.Height(25));

            GUI.enabled = _currentPageIndex < stagesProp.arraySize - 1;
            if (GUILayout.Button("Вперед ▶", GUILayout.Height(25)))
            {
                _currentPageIndex++;
            }
            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);

            // 3. СОДЕРЖИМОЕ СТРАНИЦЫ ТЕКУЩЕГО ЭТАПА
            SerializedProperty currentStageProp = stagesProp.GetArrayElementAtIndex(_currentPageIndex);

            // АВТОМАТИЗАЦИЯ: Сами записываем правильный номер этапа в скрытую переменную!
            // Индекс 0 станет Этапом 1, индекс 1 — Этапом 2 и так далее.
            SerializedProperty stageNumProp = currentStageProp.FindPropertyRelative("stageNumber");
            if (stageNumProp != null)
            {
                stageNumProp.intValue = _currentPageIndex + 1;
            }

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.LabelField($"НАСТРОЙКА СТРАНИЦЫ: ЭТАП {_currentPageIndex + 1}", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);


            SerializedProperty enemiesProp = currentStageProp.FindPropertyRelative("stageEnemies");
            if (enemiesProp != null)
            {
                EditorGUILayout.PropertyField(enemiesProp, new GUIContent("Врагина этом этапе"), true);
            }

            EditorGUILayout.Space(15);

            // Вывод текстового дебага шансов спавна
            SerializedProperty debugProp = currentStageProp.FindPropertyRelative("stageChanceDebug");
            if (debugProp != null)
            {
                // ИСПРАВЛЕНО: Сделали заголовок отчета крупным и жирным за счет EditorStyles.label
                GUIStyle titleStyle = new GUIStyle(EditorStyles.label);
                titleStyle.fontStyle = FontStyle.Bold;
                titleStyle.fontSize = 12; // Увеличили размер шрифта для читаемости

                EditorGUILayout.LabelField("Расчет шансов для текущего этапа:", titleStyle);
                EditorGUILayout.Space(3);

                GUI.enabled = false;

                GUIStyle richTextStyle = new GUIStyle(EditorStyles.textArea);
                richTextStyle.richText = true;
                richTextStyle.fontSize = 11; // Чуть приподняли размер самого текста отчета

                EditorGUILayout.TextArea(debugProp.stringValue, richTextStyle);

                GUI.enabled = true;
            }

            EditorGUILayout.EndVertical();


            EditorGUILayout.Space(15);

            // 4. КНОПКИ УПРАВЛЕНИЯ СТРАНИЦАМИ
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("+ Добавить этап в конец", GUILayout.Height(25)))
            {
                stagesProp.arraySize++;
                _currentPageIndex = stagesProp.arraySize - 1;
            }

            if (GUILayout.Button("- Удалить текущий этап", GUILayout.Height(25)))
            {
                if (EditorUtility.DisplayDialog("Удаление этапа", $"Вы уверены, что хотите полностью удалить Этап {_currentPageIndex + 1}?", "Да", "Нет"))
                {
                    stagesProp.DeleteArrayElementAtIndex(_currentPageIndex);
                    if (_currentPageIndex > 0) _currentPageIndex--;
                }
            }

            EditorGUILayout.EndHorizontal();

            // ИСПРАВЛЕНО: Весь лишний код с поиском chancePreview ПОЛНОСТЬЮ УДАЛЕН.
            // Оставляем только финальное сохранение изменений:
            serializedObject.ApplyModifiedProperties();
        }
    }
}
