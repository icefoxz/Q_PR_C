# DTO (Data Transfer Object)
- DTO 主要适用于网络传输的载体结构
- 主要分为3类

## 1. Entity DTO Model
- 是服务重要的数据模型, 一般都是订单, 用户之类的数据模型. 本质上还是DTO(用来传输), 但是它是客户端主要的数据规范. 基本上都是依据服务端的数据库实体结构转化的.
   1. **Model**
   - 命名方式基本上会根据数据库实体的名字后缀Model, 例如 : DeliverOrderModel  主要是用Mapster这个框架容易Convert Data.
   2. **Dto**
   - 命名方式后缀Dto, 例如 : LocationDto, 它一般上是附属在某个Model模型下的小结构

## 2. Result Model
- ResultModel 主要是一些请求后特别处理的Model, 一般上不会持久. 例如: Login_Result 这类.
- 命名方式 : 一般上都会有Result作为后缀, 如: Login_Result

## 3. Request DTO Model
- 是客户端请求服务端的数据模型, 一般都是客户端请求服务端的数据模型. 基本上都是依据服务端的接口结构转化的.
- 命名方式 : "前缀_请求Dto" 例如: User_ReqLoginDto


---
# DataBag
- DataBag 主要是用来传输一些数据的, 例如: 传输一些数据到服务端, 服务端处理后, 再传输回来.

## 1. 写入数据:

```csharp
var id = 1;
var name = "test";
var user = new User();
var bag = DataBag.Serialize(id,name,user)//分别写入{0=id,1=name,2=user}
```
## 2. 获取数据:
- 主要获取数据方式就是声明第组数据:
```csharp
var id = bag.Get<int>(0);//第0组数据是int
var name = bag.Get<string>(1);//第1组数据是string
var user = bag.Get<User>(2);//第2组数据是User
```