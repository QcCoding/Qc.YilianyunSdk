# 易联云 SDK

## 使用

如需实现自定义存储AccessToken，动态获取应用配置，可自行实现接口 `IYilianyunSdkHook`  
默认提供 `DefaultYilianyunSdkHook`，存储AccessToken等信息到指定目录   

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
