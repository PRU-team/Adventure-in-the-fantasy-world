# 🔄 Hướng dẫn Sprite Flipping

## ❓ Vấn đề
Bạn chỉ có 1 animation Walk (thường vẽ hướng phải), nhưng cần character quay mặt sang trái khi nhấn phím A/Left Arrow.

## ✅ Giải pháp: Auto Sprite Flip

Script `CharacterAnimationController.cs` đã được cập nhật với tính năng **tự động flip sprite** khi di chuyển!

---

## 🎯 Cách sử dụng

### Bước 1: Kiểm tra GameObject

Đảm bảo GameObject có đầy đủ components:
```
GameObject (Player/Enemy)
├── Transform
├── SpriteRenderer ✓ (Bắt buộc để flip)
├── Animator ✓
└── CharacterAnimationController ✓
```

### Bước 2: Cấu hình trong Inspector

1. **Chọn GameObject** có `CharacterAnimationController` trong Hierarchy

2. **Trong Inspector**, bạn sẽ thấy:

```
┌─────────────────────────────────────────┐
│ Character Animation Controller          │
├─────────────────────────────────────────┤
│ Sprite Flip Settings                    │
│                                          │
│ ✓ Enable Auto Flip                      │
│ ✓ Default Facing Right                  │
│                                          │
│ [→ Facing Right] [← Facing Left]        │
└─────────────────────────────────────────┘
```

3. **Cấu hình:**

   **a. Enable Auto Flip:**
   - ✓ **Tick** để bật tính năng tự động flip
   - ✗ **Untick** nếu bạn có 4 animations riêng cho 4 hướng

   **b. Default Facing Right:**
   - ✓ **Tick** nếu sprite của bạn nhìn sang **PHẢI** mặc định
   - ✗ **Untick** nếu sprite của bạn nhìn sang **TRÁI** mặc định

### Bước 3: Test

1. **Click Play** trong Unity
2. **Trong Inspector** (khi Play mode), bạn sẽ thấy:

```
┌─────────────────────────────────────────┐
│ Test Controls (Play Mode)               │
│                                          │
│ [← Test Left]  [→ Test Right]           │
│ [↑ Test Up]    [↓ Test Down]            │
│ [Test Attack]                            │
└─────────────────────────────────────────┘
```

3. **Click các nút test** để xem sprite có flip đúng không

---

## 🔧 Cách hoạt động

### Logic tự động:

```
Di chuyển sang PHẢI → Sprite hiển thị hướng phải
Di chuyển sang TRÁI  → Sprite tự động FLIP ngang
Di chuyển lên/xuống  → Giữ nguyên hướng hiện tại
```

### Ví dụ với Default Facing Right = ✓

| Hành động | Sprite FlipX | Hiển thị |
|-----------|--------------|----------|
| Nhấn D (Right) | false | → Nhìn phải |
| Nhấn A (Left) | true | ← Nhìn trái (flipped) |
| Nhấn W/S | Không đổi | Giữ hướng hiện tại |

### Ví dụ với Default Facing Right = ✗

| Hành động | Sprite FlipX | Hiển thị |
|-----------|--------------|----------|
| Nhấn D (Right) | true | → Nhìn phải (flipped) |
| Nhấn A (Left) | false | ← Nhìn trái |
| Nhấn W/S | Không đổi | Giữ hướng hiện tại |

---

## 💡 Tips

### Tip 1: Xác định hướng mặc định của sprite

**Cách kiểm tra:**
1. Mở sprite trong Project window
2. Nhìn vào nhân vật
3. Nếu mặt nhìn sang **phải** → Default Facing Right = ✓
4. Nếu mặt nhìn sang **trái** → Default Facing Right = ✗

### Tip 2: Tắt Auto Flip nếu có đủ 4 animations

Nếu bạn có sẵn:
- Walk_Left animation
- Walk_Right animation
- Walk_Up animation
- Walk_Down animation

Thì:
- ✗ **Untick** "Enable Auto Flip"
- Dùng Animator Controller với 4 states riêng biệt

### Tip 3: Flip cả Collider

Nếu character có Collider không đối xứng, có thể cần flip cả collider:

```csharp
// Thêm vào method FlipSprite() nếu cần
private void FlipSprite(float horizontalDirection)
{
    if (horizontalDirection > 0)
    {
        spriteRenderer.flipX = !defaultFacingRight;
        
        // Nếu cần flip collider
        var collider = GetComponent<BoxCollider2D>();
        if (collider != null)
        {
            var offset = collider.offset;
            offset.x = Mathf.Abs(offset.x);
            collider.offset = offset;
        }
    }
    else if (horizontalDirection < 0)
    {
        spriteRenderer.flipX = defaultFacingRight;
        
        // Flip collider
        var collider = GetComponent<BoxCollider2D>();
        if (collider != null)
        {
            var offset = collider.offset;
            offset.x = -Mathf.Abs(offset.x);
            collider.offset = offset;
        }
    }
}
```

---

## 🐛 Troubleshooting

### Vấn đề 1: Sprite không flip

**Nguyên nhân:**
- Thiếu SpriteRenderer component
- Enable Auto Flip chưa được tick
- Script chưa được gán vào GameObject

**Giải pháp:**
1. Chọn GameObject trong Inspector
2. Kiểm tra có **SpriteRenderer** không
3. Kiểm tra **Enable Auto Flip** = ✓
4. Nếu thiếu component, Inspector sẽ hiện nút "Add SpriteRenderer"

### Vấn đề 2: Sprite flip sai hướng

**Nguyên nhân:**
- Setting "Default Facing Right" không đúng

**Giải pháp:**
1. Mở Inspector
2. Toggle setting "Default Facing Right"
3. Test lại bằng nút test trong Play mode

### Vấn đề 3: Sprite flip nhưng vẫn dùng animation cũ

**Đây không phải lỗi!**
- Sprite flip chỉ **đảo ngang** sprite
- Animation vẫn là animation gốc
- Điều này tiết kiệm animations (chỉ cần 1 thay vì 2)

### Vấn đề 4: Character flip khi đứng yên

**Nguyên nhân:**
- Code khác đang gọi `ChangeDirection()` liên tục

**Giải pháp:**
- Chỉ gọi `ChangeDirection()` khi direction thay đổi
- Không gọi trong Update() mỗi frame

---

## 📝 Code Reference

### Sử dụng trong code khác:

```csharp
// Get component
var animController = GetComponent<CharacterAnimationController>();

// Di chuyển sang trái (sẽ tự động flip)
animController.ChangeDirection(Direction.Left);

// Di chuyển sang phải (sẽ tự động unflip)
animController.ChangeDirection(Direction.Right);

// Bật/tắt auto flip lúc runtime
animController.enableAutoFlip = true;  // Bật
animController.enableAutoFlip = false; // Tắt

// Đổi hướng mặc định lúc runtime
animController.defaultFacingRight = true;  // Sprite vẽ hướng phải
animController.defaultFacingRight = false; // Sprite vẽ hướng trái
```

---

## 🎮 Kịch bản sử dụng

### Kịch bản 1: Game 2D Side-scrolling

```
Animations: Idle, Walk, Jump, Attack
Default Facing Right: ✓
Enable Auto Flip: ✓

→ Character tự động flip khi di chuyển trái/phải
→ Chỉ cần 1 set animations
```

### Kịch bản 2: Game 2D Top-down (4 directions)

```
Animations: Idle, Walk_Up, Walk_Down, Walk (horizontal)
Default Facing Right: ✓
Enable Auto Flip: ✓

→ Walk_Up cho di chuyển lên
→ Walk_Down cho di chuyển xuống
→ Walk cho di chuyển ngang (tự động flip trái/phải)
```

### Kịch bản 3: Game 2D Top-down (full 4-direction animations)

```
Animations: Idle, Walk_Up, Walk_Down, Walk_Left, Walk_Right
Enable Auto Flip: ✗

→ Mỗi hướng có animation riêng
→ Không dùng flip, dùng Animator Controller
```

---

## ✨ Tổng kết

**Ưu điểm của Auto Flip:**
- ✅ Tiết kiệm animations (chỉ cần 1 thay vì 2)
- ✅ Tiết kiệm dung lượng
- ✅ Dễ maintain và update
- ✅ Performance tốt hơn

**Khi nào KHÔNG nên dùng Auto Flip:**
- ❌ Character không đối xứng (mặt trái khác mặt phải)
- ❌ Cần hiệu ứng đặc biệt cho mỗi hướng
- ❌ Đã có đủ animations cho 4 hướng

---

**Tạo bởi**: Character Animation System
**Updated**: Auto Sprite Flip Support
**Compatible with**: Unity 2020.3+
