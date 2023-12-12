# 题库管理系统

## 开始

### 创建数据库

> 默认使用 PostgreSql 数据库

1.生成数据脚本

```sh
dotnet ef migrations script 0 InitialCreate --output script/From_0_To_InitialCreate.sql
```

2.创建数据库名为`quistiondb`

3.执行(1)的脚本

### 发布项目

```sh
# linux-arm64平台
dotnet publish QuestionApi.csproj -c Release -a arm64 --os linux -o publish/linux-arm64
# dotnet publish QuestionApi.csproj -c Release -p:PublishSingleFile=true -p:PublishTrimmed=false --self-contained true -a arm64 --os linux -o publish/linux-arm64

# win-x64平台
dotnet publish QuestionApi.csproj -c Release -a x64 --os win -o publish/win-x64
# dotnet publish QuestionApi.csproj -c Release -p:PublishSingleFile=true -p:PublishTrimmed=false --self-contained true -a x64 --os win -o publish/win-x64
```

### 部署项目
