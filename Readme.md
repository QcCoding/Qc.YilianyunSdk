# 易联云打印机 SDK

`Qc.YilianyunSdk` 基于 `netstandard2` 构建  
`Qc.YilianyunSdk.Sample` 包含基础授权及打印调用示例，授权及打印接口测试，其他接口不常用只是对文档接口做了对应封装,使用前请测试好接口再使用

## 使用

### 一.安装程序包

[![Nuget](https://img.shields.io/nuget/v/Qc.YilianyunSdk)](https://www.nuget.org/packages/Qc.YilianyunSdk/)

- dotnet cli  
  `dotnet add package Qc.YilianyunSdk`
- 包管理器  
  `Install-Package Install-Package Qc.YilianyunSdk`

### 二.添加配置

> 如需实现自定义存储 AccessToken，动态获取应用配置，可自行实现接口 `IYilianyunSdkHook`  
> 默认提供 `DefaultYilianyunSdkHook`，存储 AccessToken 等信息到指定目录

```cs
using Qc.YilianyunSdk
public void ConfigureServices(IServiceCollection services)
{
  //...
  services.AddYilianyunSdk<YilianyunSdk.DefaultYilianyunSdkHook>(opt =>
  {
      // 应用ID请自行前往 dev.10ss.net 获取
      opt.ClientId = "click_id";
      opt.ClientSecret = "client_secret";
      opt.YilianyunClientType = YilianyunClientType.自有应用;
  });
  //...
}
```

### 三.代码中使用

```cs
private readonly YilianyunService _yilianyunService;
public IndexModel(YilianyunService yilianyunService)
{
    _yilianyunService = yilianyunService;
}

/// <summary>
/// 终端授权
/// </summary>
/// <returns></returns>
public IActionResult OnPostAuthTerminal()
{
    var result = _yilianyunService.AuthTerminal(MachineCode, Msign, Phone, PrinterName);
    var message = result.IsSuccess() ? "终端授权成功" : ("错误信息：" + result.Error_Description);
    return Ok(message);
}
```

### YilianyunConfig 配置

```cs
public class YilianyunConfig
{
    /// <summary>
    /// 应用ID
    /// </summary>
    public string ClientId { get; set; }
    /// <summary>
    /// 应用密钥
    /// </summary>
    public string ClientSecret { get; set; }
    /// <summary>
    /// 应用类型
    /// </summary>
    public YilianyunClientType YilianyunClientType { get; set; }
    /// <summary>
    /// token保存目录 默认 ./AppData
    /// </summary>
    public string SaveTokenDirPath { get; set; } = "./AppData";
    /// <summary>
    /// 接口地址 默认 https://open-api.10ss.net
    /// </summary>
    public string ApiUrl { get; set; } = "https://open-api.10ss.net";
    /// <summary>
    /// 接口超时时间 默认 30s
    /// </summary>
    public int? Timeout { get; set; } = 30;
}
```

## 示例说明

`Qc.YilianyunSdk.Sample` 为示例项目，运行后，获得社保号，新建易联云应用后再`Startup.cs`配置后即可测试

易联云文档地址: http://doc2.10ss.net/
