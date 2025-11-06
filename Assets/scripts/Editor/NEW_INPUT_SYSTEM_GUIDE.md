# NEW INPUT SYSTEM SETUP GUIDE
## Hướng dẫn Setup Input System Mới cho Build

---

## ✅ ĐÃ SỬA CODE

PlayerController đã được chuyển từ **Legacy Input** sang **New Input System**!

### Thay đổi:
- ❌ ~~Input.GetAxisRaw("Horizontal")~~  → ✅ `moveInput.x`
- ❌ ~~Input.GetMouseButton(0)~~ → ✅ `OnAttack()` callback
- ❌ ~~Input.GetKeyDown(KeyCode.Q)~~ → ✅ `Keyboard.current.qKey`

---

## 📋 SETUP TRONG UNITY - 5 BƯỚC

### **BƯỚC 1: Add PlayerInput Component**

1. **Mở scene có Player**
2. **Select Player GameObject** trong Hierarchy
3. **Add Component:**
   ```
   Inspector → Add Component → Search "Player Input" → Add
   ```

### **BƯỚC 2: Configure PlayerInput**

Trong Inspector, component **Player Input**:

1. **Actions:**
   - Click vào dropdown
   - Select: `InputSystem_Actions` (đã có sẵn trong Assets/)
   
2. **Default Map:**
   - Set to: `Player`
   
3. **Behavior:**
   - Set to: `Invoke Unity Events` hoặc `Send Messages`
   - **Recommended:** `Send Messages` (đơn giản hơn)

4. **Tick:** ✅ **Auto-Enable Input**

### **BƯỚC 3: Verify Input Actions**

Double-click file `Assets/InputSystem_Actions.inputactions`:

**Đảm bảo có:**
- ✅ **Move** action → WASD + Arrow keys
- ✅ **Attack** action → Mouse Left Click
- ⚠️ Nếu thiếu Q/E/R/T → Không sao, code có fallback

### **BƯỚC 4: Enable New Input System**

```
1. Edit → Project Settings → Player
2. Scroll down → Other Settings
3. Active Input Handling → Set to: "Both"
4. Unity ask restart → Click "Apply"
5. Wait for Unity to restart
```

### **BƯỚC 5: Test & Build**

1. **Test trong Editor:**
   ```
   - Play mode
   - WASD → Di chuyển ✅
   - Mouse click → Attack ✅
   - Q → Fire Attack ✅
   - E → Ranged Attack ✅
   - R → Defensive ✅
   - T → Healing ✅
   ```

2. **Build game:**
   ```
   File → Build Settings → Build And Run
   ```

3. **Test trong build:**
   - Input sẽ hoạt động bình thường! 🎉

---

## 🎮 CONTROLS

### Movement:
- **WASD** or **Arrow Keys** → Di chuyển
- **Diagonal** → Tự động normalize (không nhanh hơn)

### Combat:
- **Mouse Left Click** → Basic Attack
- **Q** → Fire Attack
- **E** → Ranged Attack (Wind Slash)
- **R** → Defensive Ability (Shield)
- **T** → Healing Ability

### Swimming:
- Movement tự động chậm 70% khi trong nước

---

## 🔧 TROUBLESHOOTING

### "PlayerInput component không tìm thấy InputSystem_Actions"

**Fix:**
```
1. Select InputSystem_Actions.inputactions trong Assets/
2. Inspector → Check "Generate C# Class" (nếu có option)
3. Hoặc: PlayerInput → Actions → Click folder icon
4. Navigate to Assets/ → Select InputSystem_Actions
```

### "Input vẫn không hoạt động trong build"

**Check:**
```
1. Player GameObject có PlayerInput component? ✅
2. PlayerInput.Actions = InputSystem_Actions? ✅
3. PlayerInput.Behavior = Send Messages? ✅
4. Active Input Handling = "Both"? ✅
5. Unity đã restart? ✅
6. Game đã rebuild? ✅
```

### "Attack button không hoạt động"

Input Actions file đã có Mouse Left Click map cho Attack action. Nếu vẫn lỗi:

```csharp
// Debug trong PlayerController.OnAttack()
public void OnAttack(InputValue value)
{
    Debug.Log($"OnAttack called! isPressed: {value.isPressed}");
    if (value.isPressed)
    {
        attackInput = true;
    }
}
```

### "Q/E/R/T abilities không hoạt động"

Code đã có fallback dùng `Keyboard.current`. Nếu vẫn lỗi:

**Option 1: Add vào Input Actions (Recommended)**
```
1. Double-click InputSystem_Actions.inputactions
2. Click "+" để add new action
3. Name: "FireAttack"
4. Type: Button
5. Add binding: Keyboard → Q
6. Repeat for E, R, T
7. Save
```

**Option 2: Use current fallback**
- Code tự động check Keyboard.current.qKey.wasPressedThisFrame
- Sẽ hoạt động trong cả Editor và Build

---

## 📊 KIẾN TRÚC MỚI

### Input Flow:

```
User Input (WASD, Mouse, Q/E/R/T)
    ↓
PlayerInput Component (Unity built-in)
    ↓
InputSystem_Actions (Config file)
    ↓
PlayerController.OnMove() / OnAttack() (Callbacks)
    ↓
moveInput / attackInput (Variables)
    ↓
Move() / UseAttackAbilities() (Logic)
    ↓
CharacterMovement / PlayerAttackController (Action)
```

### Legacy vs New:

| Legacy (Old) | New Input System |
|--------------|------------------|
| `Input.GetAxisRaw()` | `moveInput` from `OnMove()` |
| `Input.GetMouseButton()` | `attackInput` from `OnAttack()` |
| `Input.GetKeyDown()` | `Keyboard.current.qKey` |
| Manual polling in Update | Automatic callbacks |
| Project Settings required | Action Asset required |

---

## 💡 ADVANTAGES

### Tại sao New Input System tốt hơn:

✅ **Works in builds reliably**
- Không phụ thuộc Project Settings khó hiểu
- Input Actions được embed trong build

✅ **Easier rebinding**
- User có thể change keybinds
- Support UI rebinding

✅ **Better gamepad support**
- Automatic gamepad detection
- Multi-platform compatibility

✅ **Event-driven**
- Callbacks thay vì polling
- More efficient

✅ **Modern & maintained**
- Unity's recommended approach
- Future-proof

---

## 🎯 KEY CHANGES IN CODE

### Before (Legacy):
```csharp
void Move()
{
    float h = Input.GetAxisRaw("Horizontal");
    float v = Input.GetAxisRaw("Vertical");
    // ...
}

void UseAttackAbilities()
{
    if (Input.GetMouseButton(0))
    {
        Attack();
    }
}
```

### After (New):
```csharp
// Callbacks from PlayerInput
public void OnMove(InputValue value)
{
    moveInput = value.Get<Vector2>();
}

public void OnAttack(InputValue value)
{
    if (value.isPressed)
    {
        attackInput = true;
    }
}

// Use stored input values
void Move()
{
    float h = moveInput.x;
    float v = moveInput.y;
    // ...
}

void UseAttackAbilities()
{
    if (attackInput)
    {
        Attack();
        attackInput = false; // Reset
    }
}
```

---

## 📚 ADDITIONAL SETUP (Optional)

### Add More Actions:

Nếu muốn customize thêm input:

1. **Open Input Actions:**
   ```
   Assets/InputSystem_Actions.inputactions
   ```

2. **Add New Action:**
   ```
   Player map → Click "+" → Name it
   ```

3. **Add Binding:**
   ```
   Select action → "+" → Path → Choose key/button
   ```

4. **Add Callback in PlayerController:**
   ```csharp
   public void OnYourAction(InputValue value)
   {
       // Handle input
   }
   ```

### Rebinding UI:

Nếu muốn cho player change keys:

```csharp
using UnityEngine.InputSystem;

// Get current binding
var binding = playerInput.actions["Move"].bindings[0];

// Start rebinding
playerInput.actions["Move"].PerformInteractiveRebinding(0)
    .OnComplete(operation => {
        // Binding changed
        operation.Dispose();
    })
    .Start();
```

---

## ✅ CHECKLIST HOÀN THÀNH

Trước khi build, check:

- [ ] Player GameObject có PlayerInput component
- [ ] PlayerInput.Actions = InputSystem_Actions
- [ ] PlayerInput.Behavior = Send Messages
- [ ] PlayerInput.Auto-Enable = TRUE
- [ ] Active Input Handling = "Both"
- [ ] Unity đã restart sau khi change setting
- [ ] Test trong Editor: WASD, Mouse, Q/E/R/T
- [ ] Build game
- [ ] Test trong build: All inputs work
- [ ] Movement smooth
- [ ] Attack cooldowns work
- [ ] Abilities trigger correctly

---

## 🚀 PERFORMANCE

New Input System tốt hơn về performance:

- **Event-driven** → Chỉ process khi có input
- **No polling** → Không waste cycles mỗi frame
- **Optimized** → Unity's native implementation
- **Async** → Không block main thread

---

## 🎉 DONE!

Sau khi setup xong:

1. ✅ Input hoạt động trong Editor
2. ✅ Input hoạt động trong Build
3. ✅ Không cần config Project Settings phức tạp
4. ✅ Future-proof và maintainable
5. ✅ Ready để add gamepad support

**Game của bạn giờ đã dùng Input System hiện đại! 🎮✨**

---

## 📞 QUICK REFERENCE

### Common Issues:

| Issue | Solution |
|-------|----------|
| Input not working | Add PlayerInput component |
| Actions not found | Assign InputSystem_Actions |
| Callbacks not called | Set Behavior = Send Messages |
| Build still broken | Set Active Input Handling = Both |
| Abilities not working | Q/E/R/T keys use fallback system |
| Movement jerky | Check normalize logic in Move() |

### Quick Test:

```
1. Play in Editor
2. Press WASD → See player move
3. Click mouse → See attack animation
4. Press Q → See fire effect
5. If all work → Build will work!
```

---

**🎊 Chúc mừng! Input system đã được modernize!**
