using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(BigNumber))]
public class BigNumberDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Начало отрисовки поля
        EditorGUI.BeginProperty(position, label, property);

        // Ищем приватный массив _array внутри структуры BigNumber
        SerializedProperty arrayProp = property.FindPropertyRelative("_array");

        // Защита на случай, если массив в ассете еще не создан движком
        if (arrayProp == null)
        {
            EditorGUI.LabelField(position, label, "Инициализация BigNumber...");
            EditorGUI.EndProperty();
            return;
        }

        // Принудительно задаем размер массива в 4 элемента
        if (arrayProp.arraySize < 4)
        {
            arrayProp.arraySize = 4;
        }

        // Извлекаем текущие значения сегментов из Unity
        int[] segments = new int[4];
        for (int i = 0; i < 4; i++)
        {
            segments[i] = arrayProp.GetArrayElementAtIndex(i).intValue;
        }

        // Собираем из сегментов double, чтобы наш метод ToString() перевел его в красивый текст
        double total = 0;
        const double ONE_BILLION = 1000000000d;
        for (int i = 0; i < 4; i++)
        {
            total += segments[i] * System.Math.Pow(ONE_BILLION, i);
        }

        BigNumber tempNumber = new BigNumber(total);
        string currentStringValue = tempNumber.ToString();

        // Отрисовываем ОДНУ текстовую строку вместо выпадающего списка элементов массива
        string newStringValue = EditorGUI.TextField(position, label, currentStringValue);

        // Если вы изменили текст руками (например, стерли "1" и написали "9Qi")
        if (newStringValue != currentStringValue)
        {
            // Парсим строку обратно через строковый конструктор структуры
            BigNumber parsedNumber = new BigNumber(newStringValue);

            // Записываем новые значения сегментов обратно в Unity
            for (int i = 0; i < 4; i++)
            {
                arrayProp.GetArrayElementAtIndex(i).intValue = parsedNumber.GetSegment(i);
            }

            // Принудительно сохраняем изменения в ассете
            property.serializedObject.ApplyModifiedProperties();
        }

        EditorGUI.EndProperty();
    }
}

