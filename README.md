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

> ~~如果在 linux 平台 的 systemd.service 中无法运行可尝试：设置自包含`--self-contained false`参数，重新发布~~
> systemd.service 中加入环境变量：`Environment=DOTNET_ROOT=/root/.dotnet`即可。不能使用`$HOME`变量

```sh
# linux-arm64平台
dotnet publish QuestionApi.csproj -c Release -a arm64 --os linux -o publish/linux-arm64
# dotnet publish QuestionApi.csproj -c Release -p:PublishSingleFile=true -p:PublishDir=publish/linux-arm64 --self-contained false -a arm64 --os linux -o publish/linux-arm64
# dotnet publish QuestionApi.csproj -c Release -p:PublishSingleFile=true --self-contained false -a arm64 --os linux -o publish/linux-arm64
# dotnet publish QuestionApi.csproj -c Release -p:PublishSingleFile=true -p:PublishTrimmed=false --self-contained true -a arm64 --os linux -o publish/linux-arm64

# win-x64平台
dotnet publish QuestionApi.csproj -c Release -a x64 --os win -o publish/win-x64
# dotnet publish QuestionApi.csproj -c Release -p:PublishSingleFile=true --self-contained false -a x64 --os win -o publish/win-x64
# dotnet publish QuestionApi.csproj -c Release -p:PublishSingleFile=true -p:PublishTrimmed=false --self-contained true -a x64 --os win -o publish/win-x64
```

### 部署项目

## 考试设计

> 真实考试和模拟考试，逻辑是一样的，仅仅是分为两种类型而已

开始考试：

1.学生提交`考试ID`去获取`答题卡ID`(`答题历史ID`)和`考试试卷`（在一个接口返回）

结束考试：

1.提交`答题卡ID`和`学生答案`

## 练习

开始练习：

1.学生提交`试卷ID`去获取`答题卡ID`(`答题历史ID`)和`考试试卷`（在一个接口返回）

结束练习：

1.提交`答题卡ID`和`学生答案`

## 恢复答题

在`考试历史`页面点击恢复答题即可

可以分类别：考试记录（包含模拟考试）、练习记录

## MapsterMap 的 Fork 用法

```cs
var result = _mapper
    .From(history)
    .ForkConfig(f => f.ForType<AnswerHistory, AnswerBoard>()
    .Map(dest => dest.Questions, src => src.StudentAnswers), "sub-exam")
    .AdaptToType<AnswerBoard>();
```

## EFCore 异常处理

[EntityFramework.Exceptions](https://github.com/Giorgi/EntityFramework.Exceptions.git)

## REST Client 文档

[参考](https://marketplace.visualstudio.com/items?itemName=humao.rest-client)

## 添加自定义认证

```cs
builder.Services.AddAuthorization(options => {
    options.AddPolicy("api", options => {
        options.RequireAuthenticatedUser();
        options.AddAuthenticationSchemes(IdentityConstants.ApplicationScheme);
        options.AddAuthenticationSchemes(IdentityConstants.BearerScheme);
        options.AddAuthenticationSchemes("Identity.BearerAndApplication");
        if (builder.Environment.IsDevelopment()) {
            options.AddAuthenticationSchemes(AuthorizationCodeAuthenticationDefaults.AuthenticationScheme);
        }
    });
    options.DefaultPolicy = options.GetPolicy("api")!;
});

builder.Services.AddAuthentication(AuthorizationCodeAuthenticationDefaults.AuthenticationScheme).AddAuthorizationCode();
```
