# 蜂巢任务调度平台

一个任务调度平台，可以注册定期/定时的HTTP请求任务


## 技术方案

* 基于.NET 6

* 采用PostgreSQL数据库

* 支持分布式追踪

* 支持Prometheus Metrics

## 详细功能

* 定时任务：注册一个定时任务后，平台会在设定时间执行配置的HTTP请求

* 定期任务：注册一个定期任务后，平台会根据设定的cron表达式，周期性地执行配置的HTTP请求

## 如何构建

1. 需要.NET 6 SDK
2. `dotnet build`
3. 若需构建前端项目，进入`src\web`目录下，运行`npm run start`

## 如何启动

1. 进入`src\HoneyComb.Server`目录下，运行`dotnet run`启动蜂巢服务端
2. 进入`src\HoneyComb.API`目录下，运行`dotnet run`启动蜂巢API
3. 进入`src\web`目录下，运行`npm run start`启动蜂巢前端

