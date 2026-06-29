using SlimeRpgEvolution2D.Data;


public static class NumberFormatter
{
    // Полный список суффиксов: пусто, Тысячи, Миллионы, Миллиарды, Триллионы, Квадриллионы...
    private static readonly string[] Suffices = { "", "K", "M", "B", "T", "Qa", "Qi", "Sx" };

    public static string Format(BigNumber value)
    {
        // 1. Находим самый старший заполненный сегмент
        int seniorIndex = 0;
        for (int i = 3; i >= 0; i--)
        {
            if (value.GetSegment(i) > 0)
            {
                seniorIndex = i;
                break;
            }
        }

        // Если везде нули — пишем "0"
        if (seniorIndex == 0 && value.GetSegment(0) == 0) return "0";

        // 2. Высчитываем базовый суффикс на основе индекса ячейки.
        // Каждая ячейка (кроме нулевой) — это строго шаг в 3 суффикса (миллиард = 3 шага: K, M, B)
        // Индекс 0 -> старт с 0 (пусто)
        // Индекс 1 -> старт с 3 ("B")
        // Индекс 2 -> старт с 4 ("T")
        // Индекс 3 -> старт с 5 ("Qa")
        int suffixOffset = 0;
        if (seniorIndex == 1) suffixOffset = 3;      // Ячейка 1 — это Миллиарды (B)
        else if (seniorIndex == 2) suffixOffset = 4; // Ячейка 2 — это Триллионы (T)
        else if (seniorIndex == 3) suffixOffset = 5; // Ячейка 3 — это Квадриллионы (Qa)

        // 3. Собираем значение старшей ячейки и её хвост в один double
        double mainValue = value.GetSegment(seniorIndex);
        int juniorValue = (seniorIndex > 0) ? value.GetSegment(seniorIndex - 1) : 0;

        // Переводим хвост в дробную часть
        double combinedValue = mainValue + ((double)juniorValue / 1000000000d);

        // 4. УНИВЕРСАЛЬНЫЙ СДВИГ:
        // Если число больше или равно 1000 — делим на 1000 и двигаем букву вперед (например, из B делаем T)
        while (combinedValue >= 1000d && suffixOffset < Suffices.Length - 1)
        {
            combinedValue /= 1000d;
            suffixOffset++;
        }

        // Если число оказалось меньше 1 (такое бывает у хвостов при переливании) — двигаем букву назад
        while (combinedValue < 1d && suffixOffset > 0 && seniorIndex == 0)
        {
            combinedValue *= 1000d;
            suffixOffset--;
        }

        // Выводим результат с округлением до 2 знаков после запятой
        return $"{combinedValue:0.##}{Suffices[suffixOffset]}";
    }
}
