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

## Kết nối MCP Unity (để Claude/AI điều khiển Unity)

Project dùng [MCP Unity](https://github.com/CoderGamester/mcp-unity) (package `com.gamelovers.mcp-unity`, đã khai báo sẵn trong `Packages/manifest.json`), cho phép Claude Code / Claude Desktop thao tác trực tiếp Unity Editor (tạo object, sửa component, chạy Play, đọc console...).

**Yêu cầu:** Unity 6 trở lên, [Node.js](https://nodejs.org) 18+ (`node --version` để kiểm tra; macOS: `brew install node@18`).

1. Mở project trong Unity, đợi package được tải về (Window → Package Manager sẽ thấy *MCP Unity*). Nếu chưa có: Package Manager → **+** → *Add package from git URL* → `https://github.com/CoderGamester/mcp-unity.git`.
2. Mở **Tools → MCP Unity → Server Window**.
3. Nếu thư mục `Server~` chưa build, bấm **Force Install Server** (hoặc chạy `npm install && npm run build` trong `Library/PackageCache/com.gamelovers.mcp-unity@.../Server~`).
4. Bấm **Start Server** (WebSocket mặc định cổng `8090`).
5. Cấu hình client AI, cách dễ nhất là bấm nút **Configure** của client tương ứng trong Server Window. Hoặc làm thủ công:

   **Claude Code** – tạo file `.mcp.json` ở thư mục gốc project:
   ```json
   {
     "mcpServers": {
       "mcp-unity": {
         "command": "node",
         "args": ["Library/PackageCache/com.gamelovers.mcp-unity@<hash>/Server~/build/index.js"]
       }
     }
   }
   ```

   **Claude Desktop** – thêm vào `claude_desktop_config.json` (đường dẫn tuyệt đối, đổi cho đúng máy bạn):
   ```json
   {
     "mcpServers": {
       "mcp-unity": {
         "command": "node",
         "args": ["/ĐƯỜNG/DẪN/TỚI/My project/Library/PackageCache/com.gamelovers.mcp-unity@<hash>/Server~/build/index.js"]
       }
     }
   }
   ```
   (`<hash>` thay đổi theo phiên bản package; Server Window hiển thị đường dẫn chính xác để copy.) Sau khi sửa config, khởi động lại Claude Desktop.
6. Mở client AI. Khi kết nối thành công, Server Window hiện client trong khung màu xanh.

**Lưu ý:**
- Luôn bấm **Start Server** trong Unity trước khi dùng AI.
- Unity chỉ xử lý lệnh khi cửa sổ Unity đang được focus; nếu Play mode bị đứng, bấm vào cửa sổ Unity (hoặc gọi lệnh *play* lần nữa để bỏ pause).
- Token xác thực nằm ở `Library/McpUnity/bridge-token` (không commit). Nếu đổi token, khởi động lại client AI.
- Không kết nối được: kiểm tra server đang chạy, Node.js có trong PATH, thử đổi cổng trong Server Window (lưu ở `ProjectSettings/McpUnitySettings.json`).
- Lỗi *Connection failed* khi chạy test ở Play mode: tắt **Reload Domain** tại Edit → Project Settings → Editor → Enter Play Mode Settings.

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
