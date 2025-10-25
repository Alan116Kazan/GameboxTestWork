# Gamebox

**Автор:** Фарниев Алан Урузмагович

**Описание:**  
Gamebox — это Unity-проект, реализующий систему оружия с поддержкой инвентаря, UI и взаимодействия с целями. Проект включает в себя механики стрельбы, попаданий, отображения патронов и взаимодействия с объектами.
Версия Unity 2022.3.62f2

## 📁 Структура проекта

- Централизованный ввод: `PlayerInputBridge` (обёртка для Input System).  
- Оружейная подсистема:
  - `WeaponItemSO` (ScriptableObject) — параметры оружия.
  - `EquippedWeapon` — поведение инстанцированного оружия (стрельба, VFX, декали).
  - `EquipmentManager` — экипировка, кобура, буфер патрон.
  - `WeaponHitRouter` — маршрутизация попаданий к `Target`.
- Инвентарь:
  - `Inventory`, `InventorySlot`, `InventoryItemSO` — управление предметами и стеками.
  - `InventoryUIController`, `InventorySlotUI` — UI + пул слотов.
- UI / HUD:
  - `WeaponHUD`, `InteractionHintUI`, `ControlsDisplay`.
- Цели / респавн: `Target`, `TargetSpawner`.

---

## Архитектура (вкратце)

ASCII-диаграмма основных связей:

PlayerInputBridge
↓
EquipmentManager ───→ WeaponHUD
↓
EquippedWeapon ───→ WeaponHitRouter ───→ Target
↑
Inventory (ammo) ←── EquipmentManager (load/save ammo)

InventoryUIController ↔ Inventory ↔ InventorySlotUI
UI helpers: ControlsDisplay, InteractionHintUI