# BUILD INPUT FIX GUIDE
## Hướng dẫn Fix Input không hoạt động khi Build Game

---

## 🎯 VẤN ĐỀ
Sau khi build game (File → Build And Run), không thể di chuyển hoặc input không hoạt động.

**Triệu chứng:**
- ✅ Game chạy được, có hình ảnh, âm thanh
- ❌ Nhấn WASD/Arrow keys không di chuyển
- ❌ Click chuột không attack
- ✅ Trong Unity Editor thì hoạt động bình thường

---

## 🔍 NGUYÊN NHÂN

### Unity có 2 Input Systems:

1. **Legacy Input Manager (Old)**
   - Dùng `Input.GetAxisRaw()`, `Input.GetMouseButton()`
   - **Game của bạn đang dùng cái này!**
   - Cần enable trong Project Settings

2. **New Input System** 
   - Dùng Input Actions, PlayerInput component
   - Hiện đại hơn nhưng code khác hoàn toàn

### Vấn đề:
- Unity mặc định có thể disable Legacy Input Manager
- Khi build, Input.GetAxisRaw() không hoạt động → không nhận input
- Editor có thể vẫn chạy được vì setting khác

---

## 🛠️ CÁCH FIX - 3 BƯỚC ĐƠN GIẢN

### **BƯỚC 1: Chạy Debug Tool**

```
Menu: Tools → Debug → Build Input Debugger
```

1. Click nút **"🔍 CHECK ALL ISSUES"**
2. Đọc kết quả:
   - ✅ = OK
   - ❌ = Lỗi cần fix
3. Tool sẽ cho biết chính xác vấn đề là gì

---

### **BƯỚC 2: Fix Input Axes**

**Trong Build Input Debugger window:**

Click nút **"Fix Input Axes"**

Tool sẽ tự động:
- ✅ Thêm Horizontal axis (WASD, Arrow keys)
- ✅ Thêm Vertical axis (WASD, Arrow keys)
- ✅ Config Fire1 (Mouse click)

---

### **BƯỚC 3: Enable Legacy Input Manager** ⚠️ QUAN TRỌNG NHẤT

1. **Mở Project Settings:**
   ```
   Menu: Edit → Project Settings
   ```

2. **Chọn tab "Player"** (icon Android/iOS/Desktop)

3. **Scroll xuống tìm "Other Settings"**

4. **Tìm "Active Input Handling"**
   
   Thay đổi từ:
   - ❌ ~~Input System Package (New)~~
   
   Thành:
   - ✅ **Both** (Recommended)
   - Hoặc: **Input Manager (Old)**

5. **Unity sẽ hỏi restart:**
   ```
   "You need to restart the Editor for this change to take effect"
   ```
   → Click **"Apply"** và **đợi Unity restart**

---

### **BƯỚC 4: Rebuild Game**

1. **Mở Build Settings:**
   ```
   Menu: File → Build Settings
   ```

2. **Đảm bảo scene hiện tại có trong list:**
   - Nếu chưa: Click **"Add Open Scenes"**
   - Tick ✅ vào scenes cần build

3. **Click "Build And Run"**
   - Chọn folder lưu build
   - Đợi build xong
   - Game sẽ tự động chạy

4. **Test input:**
   - WASD / Arrow keys → Di chuyển
   - Mouse click → Attack
   - Q, E, R, T → Abilities

---

## ✅ CHECKLIST FIX

Trước khi rebuild, đảm bảo:

### Unity Settings:
- [ ] Edit → Project Settings → Player → Active Input Handling = **"Both"**
- [ ] Unity đã restart sau khi thay đổi setting
- [ ] Input Manager có Horizontal và Vertical axes
- [ ] Run In Background = **TRUE**

### Build Settings:
- [ ] File → Build Settings có ít nhất 1 scene
- [ ] Scene đang test được tick ✅
- [ ] Platform đúng (PC, Mac & Linux Standalone)

### Code:
- [ ] PlayerController.cs có FixedUpdate()
- [ ] Move() function có Input.GetAxisRaw("Horizontal")
- [ ] UseAttackAbilities() có Input.GetMouseButton(0)

---

## 🐛 TROUBLESHOOTING

### "Vẫn không hoạt động sau khi fix"

**1. Check Console trong build:**
```
Windows: Build folder → output_log.txt
Mac: ~/Library/Logs/Unity/Player.log
Linux: ~/.config/unity3d/CompanyName/ProductName/Player.log
```

Tìm error messages liên quan đến Input hoặc PlayerController.

**2. Verify Input Manager:**
```
Edit → Project Settings → Input Manager
→ Expand "Axes"
→ Check "Horizontal" và "Vertical" tồn tại
```

**3. Test trong Editor trước:**
```
1. Set Active Input Handling = "Both"
2. Restart Unity
3. Play trong Editor
4. Nếu Editor cũng lỗi → vấn đề khác (không phải build issue)
```

**4. Check cursor state:**

Thêm code debug vào PlayerController.cs:

```csharp
private void Start()
{
    Cursor.lockState = CursorLockMode.None;
    Cursor.visible = true;
    Debug.Log($"Cursor visible: {Cursor.visible}, lockState: {Cursor.lockState}");
}
```

---

### "Active Input Handling không có option 'Both'"

**Nguyên nhân:** New Input System package chưa được cài

**Fix:**
```
1. Window → Package Manager
2. Search "Input System"
3. Click "Install"
4. Đợi cài xong
5. Unity sẽ hỏi restart → Apply
6. Sau đó Active Input Handling sẽ có option "Both"
```

**Hoặc:** Chọn **"Input Manager (Old)"** nếu không muốn cài package mới

---

### "Build crash hoặc không mở được"

**Kiểm tra:**

1. **Missing DLLs:**
   ```
   Đảm bảo build folder có đầy đủ files:
   - .exe file (Windows)
   - _Data folder
   - UnityPlayer.dll
   - Mono folder
   ```

2. **Antivirus blocking:**
   - Windows Defender có thể block Unity builds
   - Add exception cho build folder

3. **Admin permissions:**
   - Run build as Administrator

---

### "Input delay hoặc sluggish"

**Check:**

1. **VSync:**
   ```
   Edit → Project Settings → Quality
   → VSync Count → Set to "Don't Sync"
   ```

2. **Target framerate:**
   ```csharp
   // Thêm vào PlayerController.Awake()
   Application.targetFrameRate = 60;
   ```

3. **Fixed Timestep:**
   ```
   Edit → Project Settings → Time
   → Fixed Timestep → Set to 0.02 (50 FPS physics)
   ```

---

## 📊 UNDERSTANDING INPUT SYSTEMS

### Legacy Input Manager (hiện tại đang dùng):

**Pros:**
- ✅ Đơn giản, dễ dùng
- ✅ Code ít
- ✅ Không cần package thêm

**Cons:**
- ❌ Khó customize
- ❌ Không hỗ trợ rebinding dễ
- ❌ Deprecated (Unity khuyến cáo chuyển sang New)

**Code example:**
```csharp
float h = Input.GetAxisRaw("Horizontal"); // -1, 0, or 1
if (Input.GetMouseButton(0)) { Attack(); }
```

---

### New Input System (alternative):

**Pros:**
- ✅ Hiện đại, powerful
- ✅ Easy rebinding
- ✅ Gamepad support tốt hơn
- ✅ Action-based

**Cons:**
- ❌ Cần học cách dùng mới
- ❌ Code nhiều hơn
- ❌ Cần refactor toàn bộ input code

**Migration:** Không cần thiết trừ khi muốn features mới

---

## 💡 TIPS

### Development Workflow:

1. **Always test build sớm:**
   - Đừng đợi đến khi game gần xong
   - Build và test mỗi khi thêm feature mới
   - Tránh bất ngờ cuối cùng

2. **Keep build settings consistent:**
   - Document lại settings đã dùng
   - Share với team members
   - Commit ProjectSettings folder vào Git

3. **Use Development Build:**
   ```
   File → Build Settings
   → ✅ Tick "Development Build"
   → ✅ Tick "Script Debugging"
   → Easier to debug issues
   ```

### Performance:

1. **Strip unused code:**
   ```
   Edit → Project Settings → Player → Other Settings
   → Managed Stripping Level → "Low" or "Medium"
   ```

2. **Compression:**
   ```
   File → Build Settings → Compression Method
   → "LZ4" for faster load
   → "LZ4HC" for smaller size
   ```

---

## 🎮 TESTING CHECKLIST

Sau khi build, test các scenarios:

### Basic Movement:
- [ ] WASD keys di chuyển
- [ ] Arrow keys di chuyển
- [ ] Diagonal movement (W+D, etc.)
- [ ] Movement speed đúng

### Combat:
- [ ] Mouse click attack
- [ ] Attack animation chạy
- [ ] Damage được apply
- [ ] Attack cooldown hoạt động

### Abilities:
- [ ] Q key - Fire Attack
- [ ] E key - Ranged Attack
- [ ] R key - Defensive Ability
- [ ] T key - Healing Ability
- [ ] Cooldowns hiển thị đúng

### UI:
- [ ] Health bar updates
- [ ] Ability cooldown UI
- [ ] Pause menu (ESC)
- [ ] Can click UI buttons

---

## 🚀 OPTIMIZATION

### Build Size:

Nếu build quá lớn:

```
Edit → Project Settings → Player → Other Settings
→ API Compatibility Level: ".NET Standard 2.1"
→ Managed Stripping Level: "Medium"
→ Strip Engine Code: ✅

File → Build Settings
→ Compression: "LZ4HC"
```

### Startup Time:

```
Edit → Project Settings → Player
→ Splash Screen:
   → Show Splash Screen: ❌ (nếu có license)
   
Assets → Right-click unused assets → Delete
```

---

## 📚 ADDITIONAL RESOURCES

### Unity Documentation:
- [Input System](https://docs.unity3d.com/Manual/Input.html)
- [Build Settings](https://docs.unity3d.com/Manual/BuildSettings.html)
- [Player Settings](https://docs.unity3d.com/Manual/class-PlayerSettings.html)

### Common Issues:
- Input not working: Usually Active Input Handling
- Cursor locked: Check Cursor.lockState
- No scene: Add scenes to Build Settings
- Missing DLLs: Rebuild with all files

---

## ✅ FINAL CHECKLIST

Trước khi distribute game:

- [ ] Tested build on clean machine (không có Unity)
- [ ] All inputs working (movement, attack, abilities)
- [ ] No console errors in build logs
- [ ] Performance acceptable (60 FPS target)
- [ ] Resolution settings working
- [ ] Can exit game properly
- [ ] Save/Load working (if applicable)

---

## 🎉 SUCCESS!

Nếu đã fix xong:
1. ✅ Build game chạy được
2. ✅ Input hoạt động bình thường
3. ✅ Gameplay như trong Editor
4. ✅ Có thể distribute cho người khác test

**Next steps:**
- Test trên machines khác
- Share build với friends
- Gather feedback
- Polish based on testing

Chúc mừng! Game của bạn đã sẵn sàng! 🎮✨
