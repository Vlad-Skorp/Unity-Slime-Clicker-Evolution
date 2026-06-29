using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

[System.Serializable]
public struct BigNumber
{
    // Наш бессмертный массив из 4-х ячеек по 1 миллиарду
    [SerializeField] private int[] _array;

    public int GetSegment(int index)
    {
        if (_array == null || index < 0 || index >= _array.Length) return 0;
        return _array[index];
    }

    public int Length => _array?.Length ?? 0;

    // Конструктор 1: Для формул прокачки мечей (double)
    public BigNumber(double totalValue)
    {
        const double ONE_BILLION = 1000000000d;
        _array = new int[4];

        if (double.IsNaN(totalValue) || double.IsInfinity(totalValue) || totalValue <= 0d) return;

        int index = 0;
        while (totalValue > 0d && index < _array.Length)
        {
            double remainder = Math.Floor(totalValue % ONE_BILLION);
            _array[index] = (int)remainder;
            totalValue = Math.Floor(totalValue / ONE_BILLION);
            index++;
        }
    }

    // Конструктор 2: Для простых чисел и инспектора (int)
    public BigNumber(int baseCoins)
    {
        const long ONE_BILLION = 1000000000;
        long total = baseCoins;
        _array = new int[4];
        int index = 0;

        while (total > 0 && index < _array.Length)
        {
            _array[index] = (int)(total % ONE_BILLION);
            total /= ONE_BILLION;
            index++;
        }
    }

    // Конструктор 3: Для парсинга строк вида "9Qi", "1.5B"
    public BigNumber(string textValue)
    {
        _array = new int[4];

        if (string.IsNullOrWhiteSpace(textValue)) return;

        textValue = textValue.Trim().Replace(',', '.');

        // Список поддерживаемых суффиксов
        var suffixes = new List<string> { "", "K", "M", "B", "T", "Qa", "Qi", "Sx", "Sp" };

        string numberPart = "";
        string suffixPart = "";

        for (int i = 0; i < textValue.Length; i++)
        {
            if (char.IsDigit(textValue[i]) || textValue[i] == '.' || textValue[i] == '-')
                numberPart += textValue[i];
            else
                suffixPart += textValue[i];
        }

        if (!double.TryParse(numberPart, NumberStyles.Any, CultureInfo.InvariantCulture, out double rawValue)) return;

        int suffixIndex = suffixes.FindIndex(s => s.Equals(suffixPart, StringComparison.OrdinalIgnoreCase));
        if (suffixIndex < 0) suffixIndex = 0;

        double multiplier = Math.Pow(1000, suffixIndex);
        double totalValue = rawValue * multiplier;

        if (double.IsNaN(totalValue) || double.IsInfinity(totalValue) || totalValue <= 0d) return;

        const double ONE_BILLION = 1000000000d;
        int index = 0;
        while (totalValue > 0d && index < _array.Length)
        {
            double remainder = Math.Floor(totalValue % ONE_BILLION);
            _array[index] = (int)remainder;
            totalValue = Math.Floor(totalValue / ONE_BILLION);
            index++;
        }

        Normalize();
    }

    // Метод принудительного вычитания сегментов
    public void DeductSegment(int index, int amount)
    {
        if (_array == null || index <= 0 || index >= _array.Length || amount <= 0) return;
        _array[index] -= amount;
        Normalize();
    }

    // Метод изменения базовой ячейки (индекс 0)
    public void UpdateBaseValue(int newTotal)
    {
        if (_array == null) _array = new int[4];
        _array[0] = newTotal;
        Normalize();
    }

    // Универсальный цикл нормализации
    public void Normalize()
    {
        if (_array == null || _array.Length < 4)
        {
            int[] newArray = new int[4];
            if (_array != null)
            {
                Array.Copy(_array, newArray, _array.Length);
            }
            _array = newArray;
        }

        const int ONE_BILLION = 1000000000;

        for (int i = 0; i < _array.Length - 1; i++)
        {
            if (_array[i] >= ONE_BILLION)
            {
                _array[i + 1] += _array[i] / ONE_BILLION;
                _array[i] %= ONE_BILLION;
            }
            else if (_array[i] < 0 && _array[i + 1] > 0)
            {
                int needed = (Mathf.Abs(_array[i]) / ONE_BILLION) + 1;
                _array[i + 1] -= needed;
                _array[i] += needed * ONE_BILLION;
            }
        }
    }

    // --- МАТЕМАТИЧЕСКИЕ ОПЕРАТОРЫ (Безопасны для памяти ScriptableObject) ---

    // Сложение двух BigNumber (результат в НОВОМ массиве)
    public static BigNumber operator +(BigNumber a, BigNumber b)
    {
        BigNumber result = new BigNumber(0);
        for (int i = 0; i < 4; i++)
        {
            result._array[i] = a.GetSegment(i) + b.GetSegment(i);
        }
        result.Normalize();
        return result;
    }

    // Вычитание двух BigNumber (результат в НОВОМ массиве)
    public static BigNumber operator -(BigNumber a, BigNumber b)
    {
        BigNumber result = new BigNumber(0);
        for (int i = 0; i < 4; i++)
        {
            result._array[i] = a.GetSegment(i) - b.GetSegment(i);
        }
        result.Normalize();
        return result;
    }

    // Операторы сравнения
    public static bool operator >(BigNumber a, BigNumber b)
    {
        for (int i = 3; i >= 0; i--)
        {
            if (a.GetSegment(i) > b.GetSegment(i)) return true;
            if (a.GetSegment(i) < b.GetSegment(i)) return false;
        }
        return false;
    }

    public static bool operator <(BigNumber a, BigNumber b)
    {
        for (int i = 3; i >= 0; i--)
        {
            if (a.GetSegment(i) < b.GetSegment(i)) return true;
            if (a.GetSegment(i) > b.GetSegment(i)) return false;
        }
        return false;
    }

    public static bool operator >=(BigNumber a, BigNumber b)
    {
        for (int i = 3; i >= 0; i--)
        {
            if (a.GetSegment(i) > b.GetSegment(i)) return true;
            if (a.GetSegment(i) < b.GetSegment(i)) return false;
        }
        return true;
    }

    public static bool operator <=(BigNumber a, BigNumber b)
    {
        for (int i = 3; i >= 0; i--)
        {
            if (a.GetSegment(i) < b.GetSegment(i)) return true;
            if (a.GetSegment(i) > b.GetSegment(i)) return false;
        }
        return true;
    }

    // Автоматическое преобразование обычного int в BigNumber
    public static implicit operator BigNumber(int value) => new BigNumber(value);


    // Метод для красивого вывода обратно в строку инспектора
    public override string ToString()
    {
        if (_array == null) return "0";

        double total = 0;
        const double ONE_BILLION = 1000000000d;
        for (int i = 0; i < _array.Length; i++)
        {
            total += _array[i] * Math.Pow(ONE_BILLION, i);
        }

        if (total <= 0) return "0";

        var suffixes = new[] { "", "K", "M", "B", "T", "Qa", "Qi", "Sx", "Sp" };
        int suffixIndex = 0;

        while (total >= 1000 && suffixIndex < suffixes.Length - 1)
        {
            total /= 1000;
            suffixIndex++;
        }

        return $"{total.ToString("0.##", CultureInfo.InvariantCulture)}{suffixes[suffixIndex]}";
    }

    // Добавьте этот метод внутрь вашей структуры BigNumber в файле BigNumber.cs
    public float ToFloat()
    {
        if (_array == null) return 0f;

        float total = 0f;
        const float ONE_BILLION = 1000000000f;

        // Собираем float по всем 4-м ячейкам
        for (int i = 0; i < _array.Length; i++)
        {
            total += _array[i] * Mathf.Pow(ONE_BILLION, i);
        }

        return total;
    }

    public double ToDouble()
    {
        if (_array == null) return 0d;

        double total = 0d;
        const double ONE_BILLION = 1000000000d;

        for (int i = 0; i < _array.Length; i++)
        {
            total += _array[i] * Math.Pow(ONE_BILLION, i);
        }

        return total;
    }


}
