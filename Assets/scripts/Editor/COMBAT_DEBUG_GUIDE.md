# COMBAT SYSTEM DEBUG GUIDE
## Hướng dẫn Debug Hệ Thống Chiến Đấu

### 🎯 MỤC ĐÍCH
Tool này giúp debug 2 vấn đề:
1. **Enemy không tấn công Player** - Skeleton đuổi theo nhưng không gây sát thương
2. **Player không làm damage Enemy** - Tấn công enemy nhưng không mất máu

---

## 📋 CÁCH SỬ DỤNG COMBAT SYSTEM DEBUGGER

### Bước 1: Mở Tool
```
Menu: Tools → Debug → Combat System Debugger
```

### Bước 2: Chọn đối tượng
- **Cách 1 (Tự động):**
  - Click nút `Auto-Find Enemy` để tìm Skeleton
  - Click nút `Auto-Find Player` để tìm Player
  
- **Cách 2 (Thủ công):**
  - Kéo thả GameObject Skeleton vào ô "Enemy"
  - Kéo thả GameObject Player vào ô "Player"

### Bước 3: Kiểm tra
- Click `🔍 CHECK ALL` để kiểm tra toàn bộ hệ thống
- Hoặc click từng nút:
  - `Check Enemy Attack System` - Kiểm tra enemy tấn công
  - `Check Player Attack System` - Kiểm tra player tấn công

### Bước 4: Đọc kết quả
Tool sẽ hiển thị:
- ✅ **Màu xanh**: OK, hoạt động bình thường
- ❌ **Màu đỏ**: LỖI, cần sửa

---

## 🔧 CÁC LỖI THƯỜNG GẶP VÀ CÁCH SỬA

### ❌ LỖI 1: Enemy không tấn công Player

#### Nguyên nhân có thể:

**A. CircleCollider2D.isTrigger = FALSE**
```
Triệu chứng: Tool hiển thị "CircleCollider2D.isTrigger = FALSE"
```

**Cách sửa:**
1. Chọn GameObject: `Skeleton → EnemyAttackController → BasicAttack`
2. Trong Inspector, tìm component `CircleCollider2D`
3. Tick ✅ vào ô `Is Trigger`
4. Save scene

---

**B. Thiếu Animation Event "ApplyDamageToPlayer"**
```
Triệu chứng: Tool hiển thị "Animation không có Animation Event"
```

**Cách sửa:**
1. Chọn file animation: `Skeleton_Attack.anim`
2. Mở cửa sổ Animation (Window → Animation → Animation)
3. Tìm frame giữa animation (khoảng 50-70%)
4. Click vào thanh timeline ở frame đó
5. Click nút `Add Event` (hoặc right-click → Add Animation Event)
6. Trong Inspector:
   - Function: `ApplyDamageToPlayer`
   - (Không cần parameter)
7. Save animation

**⚠️ LƯU Ý:** Animation Event sẽ gọi hàm `EnemyAttackController.ApplyDamageToPlayer()`, hàm này cần tồn tại!

---

**C. Player tag không đúng**
```
Triệu chứng: Tool hiển thị "Player tag không phải 'Player'"
```

**Cách sửa:**
1. Chọn GameObject Player
2. Trong Inspector, ở phía trên cùng
3. Tag dropdown → chọn `Player`
4. Nếu không có tag "Player", tạo mới:
   - Tags & Layers → Tags → Add (+) → Nhập "Player"

---

**D. Target reference NULL**
```
Triệu chứng: Console hiển thị "NullReferenceException: target"
```

**Cách sửa:**
- Code đã được fix để tự động tìm "Player" hoặc "PlayerCharacter"
- Đảm bảo GameObject tên là `Player` (không phải PlayerCharacter)

---

### ❌ LỖI 2: Player không làm damage Enemy

#### Nguyên nhân có thể:

**A. BasicAttack CircleCollider2D.isTrigger = FALSE**
```
Triệu chứng: Tool hiển thị "CircleCollider2D.isTrigger = FALSE"
```

**Cách sửa:**
1. Chọn GameObject: `Player → BasicAttack`
2. Trong Inspector, tìm component `CircleCollider2D`
3. Tick ✅ vào ô `Is Trigger`
4. Save scene

---

**B. Thiếu Animation Event "ApplyDamage"**
```
Triệu chứng: Tool hiển thị "Animation không có Animation Event"
```

**Cách sửa:**
1. Chọn file animation: `Player_Attack.anim` (hoặc tên tương tự)
2. Mở cửa sổ Animation (Window → Animation → Animation)
3. Tìm frame tấn công (thường là frame vung kiếm)
4. Click vào thanh timeline ở frame đó
5. Click nút `Add Event`
6. Trong Inspector:
   - Function: `ApplyDamage`
   - String parameter: `BasicAttack`
7. Save animation

**⚠️ QUAN TRỌNG:** String parameter phải là "BasicAttack" (đúng tên child GameObject)

---

**C. Enemy tag không đúng**
```
Triệu chứng: Tool hiển thị "Enemy tag không phải 'Enemy'"
```

**Cách sửa:**
1. Chọn GameObject Skeleton
2. Trong Inspector, ở phía trên cùng
3. Tag dropdown → chọn `Enemy`
4. Nếu không có tag "Enemy", tạo mới:
   - Tags & Layers → Tags → Add (+) → Nhập "Enemy"

---

**D. Thiếu AttackHitbox component**
```
Triệu chứng: Tool hiển thị "BasicAttack thiếu AttackHitbox component"
```

**Cách sửa:**
1. Chọn GameObject: `Player → BasicAttack`
2. Add Component → Search "AttackHitbox"
3. Add component `Combat.Player.AttackHitbox`
4. Save scene

---

## 🎮 KIỂM TRA SAU KHI SỬA

### Test Enemy Attack:
1. Enter Play mode
2. Di chuyển Player lại gần Skeleton
3. Chờ Skeleton attack
4. **Kỳ vọng:**
   - Skeleton chạy đến
   - Skeleton dừng lại
   - Animation attack chạy
   - Player health giảm ❤️

### Test Player Attack:
1. Enter Play mode
2. Di chuyển Player lại gần Skeleton
3. Nhấn phím tấn công (thường là Space hoặc Mouse click)
4. **Kỳ vọng:**
   - Player animation attack chạy
   - Skeleton health giảm
   - Skeleton health bar giảm

---

## 📊 HIỂU KẾT QUẢ DEBUG

### Enemy Attack System Check:

```
✅ EnemyController: OK
✅ EnemyAttackController: OK
✅ EnemyBasicAttack: OK
✅ CircleCollider2D: radius=1.5
✅ CircleCollider2D.isTrigger: TRUE
✅ CharacterAnimationController: OK
✅ Animator: Skeleton_AnimatorController
✅ Animation Event 'ApplyDamageToPlayer' found at time 0.5
✅ Player.PlayerController: OK
   → Player Tag: Player
✅ AggroRange: OK

=== SUMMARY ===
✅ TẤT CẢ OK - Enemy attack system hoạt động bình thường!
```

**Ý nghĩa:** Hệ thống enemy attack đã setup đúng, sẽ hoạt động trong game!

---

### Player Attack System Check:

```
✅ PlayerController: OK
✅ PlayerAttackController: OK
✅ AttackHitbox: OK
✅ CircleCollider2D: radius=2.0
✅ CircleCollider2D.isTrigger: TRUE
✅ CharacterAnimationController: OK
✅ Animator: Player_AnimatorController
✅ Animation Event 'ApplyDamage' found at time 0.4
✅ Enemy to test: Skeleton
   → Enemy Tag: Enemy
✅ Enemy có thể nhận damage (TakeDamage method)

=== SUMMARY ===
✅ TẤT CẢ OK - Player attack system hoạt động bình thường!
```

**Ý nghĩa:** Hệ thống player attack đã setup đúng, sẽ gây damage cho enemy!

---

## 🔍 DEBUG NÂNG CAO

### Nếu vẫn còn lỗi sau khi sửa:

1. **Check Console Log:**
   - Mở Console (Window → General → Console)
   - Tìm error message màu đỏ
   - Chú ý các dòng có `[EnemyBasicAttack]` hoặc `[AttackHitbox]`

2. **Check Attack Range:**
   - Trong Scene view, khi chọn BasicAttack
   - Sẽ thấy vòng tròn xanh (Gizmos)
   - Đảm bảo vòng tròn đủ lớn để chạm enemy/player

3. **Check Animation Timing:**
   - Animation Event có thể trigger quá sớm hoặc quá muộn
   - Điều chỉnh time của event trong Animation window
   - Thường đặt ở 50-70% duration của animation

4. **Check Attack Cooldown:**
   - Enemy có thể đang trong cooldown
   - Check trong Database: `characterAttackCooldown`
   - Giảm cooldown để test nhanh hơn

---

## 💡 TIPS

### Tối ưu Attack Range:
- **Enemy attack range:** 1.5 - 2.0 (melee)
- **Player attack range:** 2.0 - 3.0 (sword)
- Range càng lớn = dễ hit hơn nhưng kém realistic

### Debug trong Play Mode:
1. Pause game (Ctrl + Shift + P)
2. Check values trong Inspector
3. Xem collider overlap trong Scene view
4. Continue play để test

### Log Custom Debug:
Thêm vào code để debug chi tiết:
```csharp
// Trong EnemyBasicAttack.Attack()
Debug.Log($"[EnemyBasicAttack] Attack called! inRange={inRange}, damage={attackDamage}");

// Trong AttackHitbox.Attack()
Debug.Log($"[AttackHitbox] Attacking {enemies.Count} enemies, damage={attackDamage}");
```

---

## 📚 RELATED FILES

- **Enemy Attack:**
  - `EnemyController.cs` - AI logic, attack triggering
  - `EnemyAttackController.cs` - Animation control
  - `EnemyBasicAttack.cs` - Damage application
  
- **Player Attack:**
  - `PlayerController.cs` - Input handling
  - `PlayerAttackController.cs` - Attack coordination
  - `AttackHitbox.cs` - Collision detection & damage

- **Shared:**
  - `CharacterAnimationController.cs` - Animation state
  - `CharacterStats.cs` - Health & damage values
  - `Database.db` - Stats configuration

---

## ❓ FAQ

**Q: Enemy đuổi theo nhưng không attack?**
A: Check animation event và attack cooldown trong database.

**Q: Animation attack chạy nhưng không có damage?**
A: Check CircleCollider2D.isTrigger = TRUE và animation event có đúng tên function.

**Q: Player attack xuyên qua enemy không hit?**
A: Check Enemy tag = "Enemy" và CircleCollider2D trên BasicAttack.

**Q: Health bar không giảm nhưng Console không báo lỗi?**
A: Check TakeDamage() có được gọi bằng Debug.Log trong hàm đó.

**Q: Làm sao biết Animation Event đã được gọi?**
A: Thêm Debug.Log trong hàm ApplyDamage/ApplyDamageToPlayer, sẽ thấy trong Console khi attack.

---

**🎉 Chúc debug thành công!**

Nếu vẫn còn vấn đề, hãy:
1. Chụp màn hình kết quả từ Combat System Debugger
2. Copy error messages từ Console
3. Check lại các bước trong guide này
