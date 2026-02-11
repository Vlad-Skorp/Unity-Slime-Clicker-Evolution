using SlimeRpgEvolution2D.Data;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class BaseDatabase<TConfig, TKey> : ScriptableObject
    where TConfig : ScriptableObject, IIdentifiable<TKey>
{
    [Header("Database Content")]
    [SerializeField] private List<TConfig> _allEntries;

    public IReadOnlyList<TConfig> AllEntries => _allEntries;

    private Dictionary<TKey, TConfig> _cache;

    public void Initialize()
    {
        _cache = new Dictionary<TKey, TConfig>();

        if (_allEntries == null || _allEntries.Count == 0)
        {
            Debug.LogWarning($"<color=yellow>[{name}]</color> База данных пуста!");
            return;
        }

        foreach (var entry in _allEntries)
        {
            if (entry == null) continue;

            TKey id = entry.ID;

            // 1. Проверка на пустой ID (если это строка)
            if (id == null || string.IsNullOrEmpty(id.ToString()))
            {
                Debug.LogError($"<color=red>[{name}]</color> Найден объект без ID в списке! Объект: {entry.name}");
                continue;
            }

            // 2. Проверка на дубликаты (чтобы не упал словарь)
            if (_cache.ContainsKey(id))
            {
                Debug.LogError($"<color=red>[{name}]</color> Дубликат ID: '{id}'! Второй объект: {entry.name}. Пропускаю.");
                continue;
            }

            _cache.Add(id, entry);
        }
    }

    public TConfig GetByID(TKey id)
    {
        if (_cache == null || _cache.Count != _allEntries.Count) Initialize(); 
        return _cache.TryGetValue(id, out var config) ? config : default;
    }
}
