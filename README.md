# ARAM Dashboard

自建的海克斯大乱斗桌面应用，无账号登录、无 HexBox 授权、无遥测。

## 页面

- 推荐
- 数据
- 战绩
- 设置

## 构建

```powershell
powershell -ExecutionPolicy Bypass -File .\build.ps1
```

生成 `AramDashboard.exe`。

## 数据

- `data/latest.json`：英雄胜率
- `data/augment-fit.json`：英雄×海克斯适配分数

## 说明

当前是新的界面壳，后续接入 LCU 战绩、数据表和推荐逻辑。
