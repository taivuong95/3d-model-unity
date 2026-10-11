# 3d-model-unity

Game Unity 3D góc nhìn isometric: nhân vật chính **Iron Resolve** (cel-shading) chiến đấu tay không với bot AI.

## Yêu cầu

- **Unity 6000.0.30f1** (Unity 6) – cài qua Unity Hub
- Render pipeline: Universal Render Pipeline (URP)
- Input: Input System package (đã cấu hình sẵn trong project)

## Cách chạy

1. Clone repo:
   ```bash
   git clone https://github.com/taivuong95/3d-model-unity.git
   ```
2. Mở **Unity Hub** → **Add** → chọn thư mục vừa clone → mở bằng đúng phiên bản `6000.0.30f1`.
3. Lần đầu mở Unity sẽ tự import asset và tạo thư mục `Library` (mất vài phút).
4. Mở scene `Assets/Scenes/SampleScene.unity`.
5. Nhấn nút **Play**.

## Điều khiển

| Phím | Hành động |
|------|-----------|
| W / A / S / D | Di chuyển (theo hướng màn hình isometric) |
| Chuột trái | Đấm (jab) |
| F | Vào / ra thế đấm (fight stance) |
| Lăn chuột | Zoom camera |
| Space | Nhảy |

## Tính năng

- **Camera isometric** (orthographic, xoay 45°, nghiêng ~35°), bám theo nhân vật – `Assets/Scripts/IsometricCamera.cs`
- **Nhân vật chính Iron Resolve** (model Meshy, rig Humanoid) điều khiển bằng SUPER Character Controller
- **Cel-shading** có viền đen – `Assets/Shaders/ToonCel.shader`
- **Bot AI** (màu xanh): đuổi theo và đấm người chơi – `Assets/Scripts/BotAI.cs`
- Animation: idle, đi, chạy, nhảy, thế đấm Boxing, Lead Jab (Mixamo + Kevin Iglesias)

## Menu công cụ trong Unity (Tools → Character)

| Menu | Chức năng |
|------|-----------|
| Enable / Disable Isometric View | Bật / tắt camera isometric (quay lại camera góc nhìn thứ 3) |
| Apply / Remove Cel Shading | Bật / tắt cel-shading cho nhân vật chính |
| Build Character Animator | Tạo lại Animator Controller của nhân vật |

Chỉnh nét vẽ cel-shading: mở material `Assets/Enemy/IronResolve/IronResolveToon.mat` (Outline Width, Shadow Color, Edge Softness...).
Chỉnh camera: component **Isometric Camera** trên `Main Camera` (Yaw, Pitch, Ortho Size).

## Cấu trúc thư mục

```
Assets/
  Animations/   clip Mixamo (Boxing, Lead Jab)
  Editor/       script tool trong menu Tools
  Enemy/        model + texture Iron Resolve
  Player/       model + material nhân vật / bot
  Scripts/      BotAI, FighterInput, IsometricCamera
  Shaders/      ToonCel (cel-shading)
  Scenes/       SampleScene
```

## Ghi chú / hạn chế hiện tại

- Chưa có máu và sát thương, các nhân vật chưa va chạm với nhau
- Chỉ có một đòn đấm (Lead Jab)
- Nhân vật xuất phát ở góc terrain (0,0), terrain chỉ trải về phía +x/+z
