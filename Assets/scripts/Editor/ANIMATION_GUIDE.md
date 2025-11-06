# 🎬 Hướng dẫn tạo Animation KHÔNG bị Read-Only

## ⚠️ Vấn đề
Các animation clips được tự động generate từ sprite sheets (Aseprite, Texture Packer, v.v.) thường bị **READ-ONLY** và không thể chỉnh sửa Loop setting trong Inspector.

## ✅ Giải pháp

Có **3 cách** để tạo hoặc sửa animation KHÔNG bị read-only:

---

## 🎯 Cách 1: Tạo Animation Mới từ Sprites (KHUYÊN DÙNG)

### Bước 1: Mở Tool
- Unity Menu → **Tools → Animation → Create Editable Animation from Sprites**

### Bước 2: Chọn Sprites
Có 2 cách:

**A. Auto-Load (Nhanh):**
1. Trong "Sprite Folder/Asset", kéo sprite sheet của bạn vào
   - Ví dụ: File `Soldier.png` (sprite sheet có nhiều frames)
2. Click nút **"Auto-Load Sprites from Folder"**
3. Tool sẽ tự động load tất cả frames theo thứ tự

**B. Manual (Linh hoạt):**
1. Click **"+ Add Sprite Slot"** để thêm từng frame
2. Kéo từng sprite vào từng slot
3. Dùng nút ▲▼ để sắp xếp thứ tự frames

### Bước 3: Cài đặt Animation
- **Animation Name**: Đặt tên (vd: "Soldier_Walk_New")
- **Frame Rate (FPS)**: 12 (hoặc tùy thích)
- **Loop Animation**: ✓ Tick để bật loop
- **Save Path**: Chọn folder lưu (vd: "Assets/Animations/Characters/Player")

### Bước 4: Tạo Animation
- Click nút xanh **"Create Animation Clip"**
- Animation mới sẽ được tạo và BẠN CÓ THỂ CHỈNH SỬA TỰ DO!

---

## 🔄 Cách 2: Duplicate Animation Cũ thành Editable Copy

### Cách làm:
1. Trong Project window, chọn animation READ-ONLY (vd: Soldier-Attack01)
2. Click chuột phải → **Animation → Duplicate as Editable Copy**
3. Một file mới sẽ được tạo với tên "..._Editable.anim"
4. File mới này HOÀN TOÀN EDITABLE!

---

## 💪 Cách 3: Force Loop Setting cho Animation Cũ

### Cách làm:
1. Unity Menu → **Tools → Animation → Force Loop Settings**
2. Click **"Select Non-Looping"**
3. Click nút xanh **"Force Enable Loop (Selected)"**
4. Xác nhận và đợi hoàn thành

**Lưu ý**: Cách này sửa trực tiếp animation gốc, nhưng đôi khi vẫn bị reset khi reimport.

---

## 📝 So sánh các phương pháp

| Phương pháp | Ưu điểm | Nhược điểm | Nên dùng khi |
|-------------|---------|------------|--------------|
| **Tạo mới từ Sprites** | - Hoàn toàn editable<br>- Tùy chỉnh tự do<br>- Không bị reset | - Mất thời gian setup | Cần animation hoàn toàn mới hoặc customize nhiều |
| **Duplicate as Editable** | - Nhanh<br>- Giữ nguyên settings cũ<br>- Hoàn toàn editable | - Tạo file mới (duplicate) | Muốn giữ nguyên animation nhưng cần chỉnh sửa |
| **Force Loop** | - Rất nhanh<br>- Không tạo file mới | - Có thể bị reset khi reimport<br>- Chỉ sửa được Loop setting | Chỉ cần bật/tắt Loop |

---

## 🎮 Ví dụ thực tế

### Tình huống: Bạn có file "Soldier-Attack01" bị read-only

**Giải pháp nhanh:**
```
1. Click chuột phải vào "Soldier-Attack01"
2. Chọn: Animation → Duplicate as Editable Copy
3. Dùng file "Soldier-Attack01_Editable.anim"
4. Chỉnh Loop, Frame Rate, etc. thoải mái!
```

**Hoặc tạo mới hoàn toàn:**
```
1. Tools → Animation → Create Editable Animation from Sprites
2. Kéo file sprite sheet "Soldier.png" vào "Sprite Folder/Asset"
3. Click "Auto-Load Sprites from Folder"
4. Đặt tên: "Soldier_Attack_Custom"
5. Tick "Loop Animation"
6. Click "Create Animation Clip"
```

---

## ✨ Tips

### Tip 1: Tìm Sprite Sheet
Sprite sheet thường nằm ở:
- `Assets/Resources/...`
- `Assets/Sprite/...`
- `Assets/Character/...`

Trong ảnh của bạn: 
`Assets/Resources/Tiny RPG Character Asset Pack v1.03 -Free Soldier&Orc 3/Characters(100x100)/Soldier/`

### Tip 2: Frame Rate phổ biến
- **8-10 FPS**: Idle animations
- **12 FPS**: Walking animations
- **16-24 FPS**: Attack animations
- **30+ FPS**: Smooth effects

### Tip 3: Thay đổi Animation trong Animator Controller
Sau khi tạo animation editable mới:
1. Mở Animator Controller (vd: CharacterGFX.controller)
2. Click vào state (vd: "Attack" state)
3. Trong Inspector, đổi "Motion" từ animation cũ sang animation mới

---

## 🐛 Troubleshooting

### Lỗi: "Could not find any sprites"
→ Đảm bảo bạn chọn đúng sprite sheet file (.png, .jpg) có multiple sprites

### Lỗi: Animation không chạy
→ Kiểm tra xem sprite binding path có đúng không (thường là "" hoặc "Sprite")

### Animation vẫn bị read-only sau khi tạo
→ Bạn có thể đã chọn sai file. File mới phải có đuôi "_Editable.anim"

---

## 📞 Hỗ trợ thêm

Nếu vẫn gặp vấn đề, hãy kiểm tra:
1. Console log có lỗi gì không
2. File .anim mới có được tạo không
3. Inspector của file .anim mới có hiển thị Loop setting được edit không

---

**Tạo bởi**: Animation Helper Tools
**Version**: 1.0
**Unity Version**: 2020.3+
