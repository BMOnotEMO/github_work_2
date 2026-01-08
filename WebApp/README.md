# WebApp (Minimal API)

簡易 ASP.NET Core Minimal API 範例，讀取 `ConsoleApp/App_Data` 中的 JSON 檔案並提供 API，包含一個簡單前端在 `wwwroot/index.html` 顯示資料。

執行（PowerShell）:

```powershell
cd E:\work\program\softwareEngineering\workplace
dotnet run --project .\WebApp\WebApp.csproj
# 開啟瀏覽器: http://localhost:5000 或輸出命令會顯示實際埠號
```

備註：此專案會在 build 時複製 `..\ConsoleApp\App_Data\**` 到輸出資料夾，因此會使用你現有的 sample JSON 檔案。
