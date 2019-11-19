# 易联云打印机 SDK

`Qc.YilianyunSdk` 基于 `.NET Standard 2.0 ` 构建  
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
> 需要注意**自有应用**和**开放应用**的区别，它们的保存不一样，自有应用应根据 client_id(应用ID) 保存 accessToken 信息，开放应用应根据 machine_code(打印机终端号) 保存 accessToken 信息

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

public string Message { get; set; }

/// <summary>
/// 极速授权-开放应用
/// </summary>
/// <returns></returns>
public IActionResult OnPostAuthFast()
{
    var result = _yilianyunService.AuthFast(MachineCode, QrKey);
    Message = result.IsSuccess() ? "极速授权成功" : ("错误信息：" + result.Error_Description);
    return Page();
}
/// <summary>
/// 终端授权-自有应用 注意：获取的AccessToken无失效时间，若为自有应用，获取一次就可以了，其实就是把应用的AccessToken和打印机绑定了
/// </summary>
/// <returns></returns>
public IActionResult OnPostAuthTerminal()
{
    var result = _yilianyunService.AuthTerminal(MachineCode, Msign, Phone, PrinterName);
    Message = result.IsSuccess() ? "终端授权成功" : ("错误信息：" + result.Error_Description);
    return Page();
}

/// <summary>
/// 打印机状态
/// </summary>
/// <returns></returns>
public IActionResult OnPostPrinterStatus()
{
    var result = _yilianyunService.PrinterGetStatus(AccessToken, MachineCode);
    if (result.IsSuccess())
    {
        PrinterStatus = result.Body.State.ToString();
    }
    Message = result.IsSuccess() ? "获取终端状态成功" : ("错误信息：" + result.Error_Description);
    return Page();
}
/// <summary>
/// 打印文本
/// </summary>
/// <returns></returns>
public IActionResult OnPostPrintText()
{
    var result = _yilianyunService.PrintText(AccessToken, MachineCode, PrintContent);
    Message = result.IsSuccess() ? "打印文本成功" : ("错误信息：" + result.Error_Description);
    return Page();
}
```

### YilianyunConfig 配置项
| 字段名        | 类型           | 描述  |
| ------------- |:-------------:| -----:|
| ClientId      | string |  应用ID |
| ClientSecret     | string      |   应用密钥 |
| YilianyunClientType | int(开放应用=0,自有应用=1)    |    应用类型 |
| SaveTokenDirPath     | string      |    token保存目录 默认 ./AppData |
| ApiUrl     | string      |    接口地址 默认 https://open-api.10ss.net |
| Timeout     | int      |    接口超时时间 30s |

## 示例说明

`Qc.YilianyunSdk.Sample` 为示例项目，运行后，获得设备号，新建易联云应用后再`Startup.cs`配置后即可测试

易联云文档地址: http://doc2.10ss.net/
