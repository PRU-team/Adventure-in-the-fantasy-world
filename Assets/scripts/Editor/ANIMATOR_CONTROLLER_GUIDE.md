# 🎮 Hướng dẫn tạo Animator Controller

## 📋 Mục lục
1. [Tạo Animator Controller mới](#1-tạo-animator-controller-mới)
2. [Cập nhật Animator Controller có sẵn](#2-cập-nhật-animator-controller-có-sẵn)
3. [Gán Controller vào GameObject](#3-gán-controller-vào-gameobject)
4. [Cấu trúc Animator Controller](#4-cấu-trúc-animator-controller)
5. [Sử dụng với CharacterAnimationController.cs](#5-sử-dụng-với-characteranimationcontrollercs)

---

## 1. Tạo Animator Controller mới

### Phương pháp A: Sử dụng Tool (KHUYÊN DÙNG)

#### Bước 1: Mở Tool
```
Unity Menu → Tools → Animation → Create Animator Controller
```

#### Bước 2: Cấu hình Settings
- **Controller Name**: Đặt tên (vd: "PlayerAnimator", "EnemyAnimator")
- **Save Path**: Chọn folder lưu (vd: "Assets/Animations/Characters/Player")

#### Bước 3: Chọn Options
- ✓ **Create Movement States**: Tạo states cho di chuyển (Idle, Walk Up/Down/Left/Right)
- ✓ **Create Combat States**: Tạo states cho combat (Attack, Hurt, Death)
- ✓ **Auto-Create Parameters**: Tự động tạo parameters cần thiết

#### Bước 4: Assign Animations

**Cách 1 - Auto Assign (Nhanh):**
1. Trong Project window, chọn TẤT CẢ animation clips của nhân vật
   - Ctrl/Cmd + Click để chọn nhiều files
   - Ví dụ: Soldier_Idle, Soldier_Walk_Up, Soldier_Attack, etc.
2. Trong Tool window, click **"Auto-Assign Animations from Selection"**
3. Tool sẽ tự động match animations theo tên!

**Cách 2 - Manual:**
1. Kéo từng animation clip vào đúng slot:
   - **Idle**: Animation khi đứng yên
   - **Walk Up/Down/Left/Right**: Animations di chuyển 4 hướng
   - **Attack**: Animation tấn công
   - **Hurt**: Animation bị đánh
   - **Death**: Animation chết

#### Bước 5: Create Controller
Click nút xanh **"Create New Controller"**

**Kết quả:**
- ✅ File `.controller` mới được tạo
- ✅ Tất cả states đã setup
- ✅ Transitions đã cấu hình
- ✅ Parameters đã tạo sẵn
- ✅ Sẵn sàng sử dụng!

---

## 2. Cập nhật Animator Controller có sẵn

### Khi nào dùng?
- Bạn đã có controller nhưng muốn thêm animations mới
- Muốn cập nhật animations cũ

### Cách làm:

#### Bước 1: Mở Tool
```
Tools → Animation → Create Animator Controller
```

#### Bước 2: Load Controller hiện tại
- Trong "Existing Controller", kéo file `.controller` hiện tại vào
- Ví dụ: `CharacterGFX.controller`

#### Bước 3: Assign Animations mới
- Assign các animation clips mới vào các slot

#### Bước 4: Update
- Click **"Update Existing Controller"**
- Tool sẽ thêm/cập nhật states mà không xóa cấu hình cũ

---

## 3. Gán Controller vào GameObject

### Phương pháp A: Sử dụng Helper Tool

#### Bước 1: Select GameObjects
- Trong Scene hoặc Hierarchy, chọn GameObject(s) cần gán animator
- Có thể chọn nhiều objects cùng lúc (Ctrl/Cmd + Click)

#### Bước 2: Mở Tool
```
Tools → Animation → Assign Controller to GameObjects
```

#### Bước 3: Assign Controller
1. Kéo Animator Controller vào "Animator Controller" slot
2. Tool sẽ hiển thị danh sách objects đã chọn
3. Click **"Assign Controller to Selected Objects"**

**Tool sẽ tự động:**
- ✅ Thêm Animator component (nếu chưa có)
- ✅ Gán controller
- ✅ Cấu hình settings

### Phương pháp B: Manual trong Unity

1. Chọn GameObject trong Hierarchy
2. Trong Inspector:
   - Thêm component **Animator** (nếu chưa có)
   - Trong Animator component, kéo Controller file vào "Controller" slot
3. Done!

---

## 4. Cấu trúc Animator Controller

### States được tạo tự động:

```
📁 Base Layer
├── 🟢 Idle (Default State)
│   └── Animation: Idle clip
│
├── 🔵 Walk_Up
│   └── Animation: Walk Up clip
│
├── 🔵 Walk_Down
│   └── Animation: Walk Down clip
│
├── 🔵 Walk_Left
│   └── Animation: Walk Left clip
│
├── 🔵 Walk_Right
│   └── Animation: Walk Right clip
│
├── 🔴 Attack
│   └── Animation: Attack clip
│   └── Transition: Any State → Attack (Trigger: Attack)
│   └── Transition: Attack → Idle (Exit Time)
│
├── 🟡 Hurt
│   └── Animation: Hurt clip
│   └── Transition: Any State → Hurt (Trigger: Hit)
│   └── Transition: Hurt → Idle (Exit Time)
│
└── ⚫ Death
    └── Animation: Death clip
    └── Transition: Any State → Death (Trigger: Death)
```

### Parameters được tạo tự động:

| Parameter | Type | Mô tả |
|-----------|------|-------|
| `HorizontalSpeed` | Float | Tốc độ ngang (-1: Left, 1: Right) |
| `VerticalSpeed` | Float | Tốc độ dọc (-1: Down, 1: Up) |
| `isIdle` | Bool | Đang idle hay không |
| `isSwimming` | Bool | Đang bơi hay không |
| `Attack` | Trigger | Kích hoạt attack |
| `FireAttack` | Trigger | Kích hoạt fire attack |
| `RangedAttack` | Trigger | Kích hoạt ranged attack |
| `DefensiveAbility` | Trigger | Kích hoạt defensive ability |
| `HealingAbility` | Trigger | Kích hoạt healing ability |
| `Hit` | Trigger | Nhận damage |
| `Death` | Trigger | Chết |

---

## 5. Sử dụng với CharacterAnimationController.cs

Controller được tạo **HOÀN TOÀN TƯƠNG THÍCH** với script `CharacterAnimationController.cs` hiện có!

### Setup trong Scene:

```
GameObject (Player/Enemy)
├── Animator Component
│   └── Controller: YourAnimatorController.controller
│
└── CharacterAnimationController Script
    └── Sẽ tự động tìm Animator component
```

### Code đã tương thích:

```csharp
// Script của bạn đã có sẵn các methods này:

// Di chuyển
ChangeDirection(Direction.Up);    // Dùng Walk_Up state
ChangeDirection(Direction.Down);  // Dùng Walk_Down state

// Combat
StartAttack();           // Trigger "Attack"
TakeHit();              // Trigger "Hit"
CharacterDeath();       // Trigger "Death"
```

### Kiểm tra hoạt động:

1. Chạy game
2. Mở Animator window (Window → Animation → Animator)
3. Chọn GameObject có Animator
4. Xem states chuyển đổi theo hành động của nhân vật

---

## 💡 Tips & Best Practices

### Tip 1: Đặt tên Animation rõ ràng
```
✅ Good:
- Player_Idle
- Player_Walk_Up
- Player_Attack_Sword

❌ Bad:
- anim1
- idle
- attack
```

### Tip 2: Organize Folders
```
Assets/
└── Animations/
    ├── Characters/
    │   ├── Player/
    │   │   ├── PlayerAnimator.controller
    │   │   ├── Player_Idle.anim
    │   │   ├── Player_Walk_Up.anim
    │   │   └── ...
    │   │
    │   └── Enemy/
    │       ├── EnemyAnimator.controller
    │       └── ...
    │
    └── VFX/
        └── ...
```

### Tip 3: Test Transitions
- Mở Animator window
- Click Play trong Unity
- Xem transitions có smooth không
- Adjust transition duration (thường 0.1 - 0.2s)

### Tip 4: Animation Events
Nếu cần call functions tại thời điểm cụ thể trong animation:

1. Chọn Animation clip
2. Mở Animation window
3. Add Event tại frame cần thiết
4. Chọn function từ CharacterAnimationController.cs

---

## 🐛 Troubleshooting

### Vấn đề 1: Animation không chạy
**Nguyên nhân:**
- GameObject thiếu Animator component
- Controller chưa được gán
- Animation clip bị null

**Giải pháp:**
```
Tools → Animation → Assign Controller to GameObjects
```

### Vấn đề 2: Transitions không hoạt động
**Kiểm tra:**
- Parameters có đúng tên không?
- Conditions trong transitions đúng không?
- CharacterAnimationController.cs có gọi đúng parameters không?

### Vấn đề 3: Animation bị lag/jerky
**Giải pháp:**
- Tăng Frame Rate của animation clips (16-24 FPS)
- Giảm transition duration (0.05 - 0.1s)
- Bật "Interpolate" trong Animator component

### Vấn đề 4: Character không quay mặt đúng hướng
**Lưu ý:**
- Animations 4 hướng chỉ thay đổi sprites, không rotate GameObject
- Nếu cần rotate, thêm code trong CharacterAnimationController.cs

---

## 📚 Resources

### Context Menu Shortcuts
Right-click trên GameObject trong Hierarchy:
```
GameObject → Animation →
    ├── Add Animator Component
    ├── Open Animator Window
    └── Open Animation Window
```

### Keyboard Shortcuts
- **Ctrl + 6**: Mở Animator window
- **Ctrl + 4**: Mở Animation window (để edit clips)

---

## 🎯 Quick Reference

### Quy trình hoàn chỉnh từ đầu:

1. **Tạo Animations**
   ```
   Tools → Animation → Create Editable Animation from Sprites
   ```

2. **Tạo Controller**
   ```
   Tools → Animation → Create Animator Controller
   → Auto-assign animations
   → Create New Controller
   ```

3. **Assign vào GameObject**
   ```
   Select GameObject trong Scene
   → Tools → Animation → Assign Controller to GameObjects
   → Assign controller
   ```

4. **Test**
   ```
   Click Play
   → Character sẽ tự động dùng animations!
   ```

---

**Tạo bởi**: Animator Controller Tools
**Compatible with**: CharacterAnimationController.cs
**Unity Version**: 2020.3+
