# Work 2

## 專案說明

本工作區包含兩個 .NET 專案：

- ConsoleApp：WinForms 桌面應用程式，讀取觀光與相關開放資料，將高雄市主要觀光景點人數視覺化。
- WebApp：ASP.NET Core Web 應用程式，提供單頁式儀表板，整合「觀光人數」、「交通事故與科技執法」、「人口統計」三大主題，使用 Chart.js 與 Leaflet 呈現。

## 資料來源

- 高雄市主要觀光遊憩區遊客人次（政府資料開放平台）
- 高雄市 104 年前 50 大易肇事路口
- 高雄市交通科技執法據點（固定式、移動式）
- 各村里人口統計月報表

JSON 原始檔都放在 `ConsoleApp/App_Data/`
WebApp 透過 `WebApp.csproj` 的設定`（Include="..\ConsoleApp\App_Data\**\*.*"）`在建置時自動把這些檔案連結並複製到執行時的 App_Data 目錄，所以兩個專案共用同一份資料，不需要在 WebApp 再放一份 JSON。

## 工作區分析與示範說明

- ConsoleApp
  - 來源碼位於 `ConsoleApp/ConsoleApp/`。
  - 使用 ScottPlot 繪製年度景點人次長條圖，使用者可選擇年份並調整圖表顯示。
- WebApp
  - 來源碼位於 `WebApp/`。
  - `wwwroot/index.html` 為單頁儀表板：
    - 觀光人數：年度與景點篩選、Top 10 圖表與表格。
    - 交通事故：A1/A2/A3 行政區統計、事故熱點地圖。
    - 科技執法：固定／移動測速地圖、各行政區與各執法種類統計圖表與完整表格。
    - 人口統計：依行政區彙總出生數／死亡數／結婚／離婚指標，提供表格與上下兩個長條圖。

## 專案結構（摘要）

```text
github_work_2/
├── ConsoleApp/
│   ├── ConsoleApp.sln
│   └── ConsoleApp/
│       ├── ConsoleApp.csproj
│       ├── MainForm.cs
│       ├── Services/
│       └── App_Data/
│           └── Visitor.json     # 觀光人數資料
├── WebApp/
│   ├── WebApp.csproj
│   ├── Program.cs
│   ├── App_Data/
│   │   ├── Visitor.json
│   │   ├── AccidentPlaceBefore104.json
│   │   ├── Camara113.json
│   │   ├── MoveCamara113.json
│   │   └── Population.json
│   └── wwwroot/
│       └── index.html           # 儀表板前端程式
└── README.md
```

## 建置與執行

### 1. 開發環境需求

- .NET 9 SDK（可用 `dotnet --version` 確認）
- Windows（ConsoleApp 是 WinForms 專案）

---

## 2. 執行 WebApp

```
cd WebApp
dotnet run --urls http://localhost:5000
```

### 3. 或 執行 ConsoleApp

```
powershell
cd ConsoleApp
dotnet run
```