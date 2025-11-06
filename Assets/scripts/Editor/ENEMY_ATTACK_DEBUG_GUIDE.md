# 🐛 Hướng dẫn Debug Enemy Attack

## ❓ Vấn đề
Enemy không tự động tấn công và player không bị mất máu khi đánh enemy.

## 🔍 Tool Debug

Tôi đã tạo tool **Enemy Attack Debugger** để tự động kiểm tra tất cả các vấn đề có thể xảy ra.

### Cách sử dụng:

```
1. Unity Menu → Tools → Debug → Enemy Attack Debugger
2. Trong Hierarchy, chọn enemy GameObject (vd: Skeleton)
3. Tool sẽ tự động kiểm tra 7 điểm:
   ✓ Enemy Controller
   ✓ Enemy Attack Controller
   ✓ Enemy Basic Attack
   ✓ Character Animation Controller
   ✓ Animator
   ✓ Player Setup
   ✓ Animation Events
```

---

## 🎯 Checklist Manual (nếu không dùng tool)

### 1. Kiểm tra Hierarchy Structure

**Cấu trúc đúng phải là:**

```
Skeleton (hoặc enemy name)
├─ EnemyController component ✓
├─ Rigidbody2D ✓
├─ Seeker (for pathfinding) ✓
├─ AggroRange (reference) ✓
├─ HealthBar (reference) ✓
│
├─ CharacterGFX (child)
│  ├─ SpriteRenderer ✓
│  ├─ Animator ✓
│  └─ CharacterAnimationController ✓
│
└─ EnemyAttack (child)
   ├─ EnemyAttackController ✓
   │
   └─ BasicAttack (child of EnemyAttack)
      ├─ EnemyBasicAttack script ✓
      └─ CircleCollider2D (IsTrigger = TRUE) ✓
```

**Kiểm tra trong Inspector:**
1. Chọn Skeleton trong Hierarchy
2. Xem tất cả child objects
3. Đảm bảo có đúng structure như trên

---

### 2. Kiểm tra Components

#### A. EnemyController (trên root GameObject)
- ✓ Script có gán không?
- ✓ AggroRange có assign không?
- ✓ HealthBar có assign không?

#### B. EnemyAttackController (trên child "EnemyAttack")
- ✓ Script có gán không?
- ✓ Có child "BasicAttack" không?

#### C. EnemyBasicAttack (trên child "BasicAttack")
- ✓ Script có gán không?
- ✓ **CircleCollider2D** có chưa?
- ✓ **CircleCollider2D.isTrigger = TRUE** chưa? (QUAN TRỌNG!)
- ✓ Radius của collider (thường 1-2 units)

---

### 3. Kiểm tra Player Setup

#### GameObject Name
```csharp
// Code tìm player:
GameObject.Find("PlayerCharacter")
```

**Phải đảm bảo:**
- ✓ Player GameObject name = **"PlayerCharacter"** (chính xác!)
- ✓ Player có tag = **"Player"**
- ✓ Player có component **PlayerController**

**Kiểm tra:**
1. Chọn player trong Hierarchy
2. Xem Inspector:
   - Name phải là "PlayerCharacter"
   - Tag phải là "Player"
3. Có PlayerController component

---

### 4. Kiểm tra Animator Controller

#### A. Animator Component
1. Chọn CharacterGFX (child của enemy)
2. Kiểm tra Animator component:
   - Controller có assign chưa?
   - Controller có "Attack" trigger không?

#### B. Animation Clip
1. Mở Animator window (Ctrl + 6)
2. Kiểm tra có "Attack" state không?
3. Attack state có animation clip không?

---

### 5. Kiểm tra Animation Events (QUAN TRỌNG!)

**Đây thường là nguyên nhân chính!**

#### Kiểm tra:
1. Trong Project, tìm attack animation clip (vd: Skeleton_Attack)
2. Select animation clip
3. Mở Animation window (Ctrl + 4)
4. Xem timeline có **Event marker** (hình cờ trắng) không?

#### Animation Event phải có:
```
Function Name: ApplyDamageToPlayer
Time: ~ 50-70% of animation (khi animation đánh trúng)
```

#### Nếu không có Event:
**Cách thêm:**
1. Select attack animation clip
2. Mở Animation window
3. Click vào timeline tại frame muốn gây damage (thường giữa animation)
4. Click button "Add Event"
5. Chọn function: **ApplyDamageToPlayer**
6. Save

---

## 🔧 Quick Fixes

### Fix 1: Missing CircleCollider2D

```
1. Select "BasicAttack" GameObject
2. Add Component → Physics 2D → Circle Collider 2D
3. Set:
   - Is Trigger = ✓ TRUE
   - Radius = 1.5 (adjust as needed)
```

### Fix 2: Wrong Player Tag

```
1. Select PlayerCharacter
2. Inspector → Tag dropdown → Player
```

### Fix 3: Missing Animation Event

```
1. Select attack animation clip in Project
2. Window → Animation → Animation
3. At frame ~15-20 (or mid-swing):
   - Click "Add Animation Event"
   - Function: ApplyDamageToPlayer
4. Save (Ctrl + S)
```

### Fix 4: Wrong Hierarchy

```
If BasicAttack is not child of EnemyAttack:
1. Drag BasicAttack onto EnemyAttack in Hierarchy
2. Make sure EnemyBasicAttack script is on BasicAttack
```

---

## 🧪 Testing

### Test trong Editor:

**Bước 1: Mở Debug Tool**
```
Tools → Debug → Enemy Attack Debugger
```

**Bước 2: Select Enemy**
```
Hierarchy → Click Skeleton
```

**Bước 3: Xem Report**
```
Tool sẽ hiển thị:
✓ = OK
⚠️ = Warning
❌ = Error
```

**Bước 4: Fix Issues**
```
- Click nút "Fix" bên cạnh mỗi issue
- Hoặc follow hướng dẫn trong tool
```

**Bước 5: Test in Play Mode**
```
1. Click Play
2. Click button "Test Attack Now" trong tool
3. Xem Console có log gì không
```

---

### Test trong Game:

**Bước 1: Play Game**
```
Click Play trong Unity
```

**Bước 2: Approach Enemy**
```
Di chuyển player gần enemy
```

**Bước 3: Observe**

**Enemy phải:**
- ✓ Di chuyển đến player (nếu in aggro range)
- ✓ Dừng lại khi gần player
- ✓ Play attack animation
- ✓ Player mất máu (healthbar giảm)

**Kiểm tra Console:**
```
Window → General → Console
```

Nếu thấy errors → đọc error message và fix

---

## 🐛 Common Issues & Solutions

### Issue 1: "Player not found"

**Nguyên nhân:**
- Player GameObject name không phải "PlayerCharacter"

**Giải pháp:**
```
Rename player GameObject to: PlayerCharacter
```

---

### Issue 2: "Enemy không attack"

**Nguyên nhân:**
- Enemy không trong attack range
- nextAttack cooldown chưa hết
- playerInRange = false

**Giải pháp:**
1. Kiểm tra AggroRange component có assign không
2. Kiểm tra CircleCollider2D.radius của BasicAttack
3. Move player rất gần enemy để test

---

### Issue 3: "Player không mất máu"

**Nguyên nhân:**
- Animation event **ApplyDamageToPlayer** chưa có
- CircleCollider2D.isTrigger = false
- Player tag không đúng

**Giải pháp:**
```
Fix theo thứ tự:
1. Add animation event (xem Fix 3 phía trên)
2. Set isTrigger = true
3. Set player tag = "Player"
```

---

### Issue 4: "NullReferenceException"

**Nguyên nhân:**
- Thiếu component hoặc reference

**Giải pháp:**
```
1. Đọc error message trong Console
2. Xem dòng nào bị lỗi
3. Check component/reference đó
```

**Ví dụ:**
```
Error: NullReferenceException at EnemyBasicAttack.Attack()
→ player GameObject not found
→ Fix: Check player name = "PlayerCharacter"
```

---

## 📊 Debug Logs

### Thêm Debug Logs để test:

#### Trong EnemyController.cs:

```csharp
private void Attack()
{
    if (playerInRange && Time.time >= nextAttack && !isDead)
    {
        Debug.Log($"[{gameObject.name}] Starting attack!");
        
        if (gameObject.transform.position.x > target.position.x)
        {
            gameObject.transform.localScale = new Vector3(-1, 1, 1);
        }
        
        enemyAttackController.Attack();
        nextAttack = Time.time + characterStats.GETAttackCooldown();
        
        Debug.Log($"[{gameObject.name}] Next attack at: {nextAttack}");
    }
    else
    {
        if (!playerInRange)
            Debug.Log($"[{gameObject.name}] Player not in range");
    }
}
```

#### Trong EnemyBasicAttack.cs:

```csharp
public void Attack(float attackDamage)
{
    Debug.Log($"EnemyBasicAttack.Attack() called. InRange: {inRange}, Damage: {attackDamage}");
    
    if (inRange)
    {
        Debug.Log($"Applying {attackDamage} damage to player");
        player.GetComponent<PlayerController>().TakeDamage(attackDamage);
    }
    else
    {
        Debug.LogWarning("Player not in attack range!");
    }
}
```

---

## ✅ Expected Behavior

**Khi mọi thứ hoạt động đúng:**

```
1. Player vào aggro range
   → Enemy bắt đầu di chuyển đến player

2. Player trong attack range
   → Enemy dừng lại
   → Animation "Attack" play
   
3. Animation đến frame có event
   → ApplyDamageToPlayer() called
   → Player.TakeDamage() called
   → Player health giảm
   → HealthBar update

4. Sau cooldown
   → Enemy attack lại
```

**Console logs (nếu thêm debug):**
```
[Skeleton] Starting attack!
[Skeleton] Next attack at: 2.5
EnemyBasicAttack.Attack() called. InRange: True, Damage: 10
Applying 10 damage to player
[Player] Taking 10 damage. Health: 90/100
```

---

## 🎯 Summary

**Các bước debug:**
1. ✓ Use **Enemy Attack Debugger** tool
2. ✓ Check hierarchy structure
3. ✓ Verify all components present
4. ✓ Ensure CircleCollider2D.isTrigger = TRUE
5. ✓ Check player name = "PlayerCharacter" and tag = "Player"
6. ✓ **Add animation event "ApplyDamageToPlayer"** ⭐ QUAN TRỌNG
7. ✓ Test in Play mode

**Nếu vẫn không được:**
- Gửi screenshot của:
  1. Enemy Hierarchy
  2. BasicAttack Inspector (showing CircleCollider2D)
  3. Animation window (showing events)
  4. Console errors

---

**Created by**: Enemy Attack Debug System
**Version**: 1.0
**Unity**: 2020.3+
