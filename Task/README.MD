# Challenge 00 – Dockerfile & Docker Compose cho MiniERP

## 1. Tech stack của project

Đầu tiên ta thấy stack tech của project như sau:

| Thành phần | Công nghệ |
|---|---|
| **Core API** | .NET 9.0 (ASP.NET Core Web API) |
| **Database** | PostgreSQL 15 |
| **ORM** | Entity Framework Core 9 (EF Core) hỗ trợ LINQ, Migrations và Optimistic Concurrency |
| **Background Jobs** | Hangfire (sử dụng PostgreSQL Storage để quản lý tác vụ chạy ngầm) |
| **Security & Auth** | JWT (JSON Web Token) kết hợp Policy-Based Authorization |
| **Containerization** | Docker & Docker Compose |

**Thư viện hỗ trợ:**
- **FluentValidation**: Kiểm tra tính hợp lệ dữ liệu đầu vào.
- **BCrypt.Net**: Mã hóa mật khẩu bảo mật cao.
- **CsvHelper**: Hỗ trợ xuất dữ liệu báo cáo dạng stream để tối ưu bộ nhớ.

---

## 2. Dockerfile

### 2.1. Tham khảo Dockerfile mẫu

Ta thử research xem một Dockerfile mẫu cho 1 project NET 9.0
🔗 https://nodogmablog.bryanhogan.net/2025/09/dockerfile-for-a-net-9-web-api-application/

Ta có mẫu như sau:

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /source

# Copy everything. This is less efficient
COPY --link . .
# Publish app - will get a warning if there is a .sln file in the directory.
# -c Release is now the default for dotnet publish, but if you are using .NET < 8.0, you should include it.
# A full restore is done as part of the publish. 
RUN dotnet publish -c Release -o /app 

# Change to the runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0
EXPOSE 8080
WORKDIR /app
COPY --link --from=build /app .

ENTRYPOINT ["./WebApiDocker"]
```

### 2.2. Quy trình hoạt động

Ta tiến hành tìm hiểu quy trình hoạt động của Dockerfile cho project NET 9.0 như sau:

```
Từ source code
   → ta tải các NuGet package về
   → biên dịch .cs thành dll
   → gom tất cả mọi thứ cần chạy vào một thư mục
   → chạy app bằng câu lệnh dotnet xxx.dll
```

Từ những thứ trên ta có thể suy ra được khi chạy app nó chỉ cần runtime thôi thứ cái file `xxx.dll` cuối đó nên ta có thể dùng **multi-stage** để build tối ưu về kích thước của image được tạo ra.

### 2.3. Các bước viết Dockerfile

#### Bước 1: Base image

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /source
```

#### Bước 2: Restore các package

**2.1.** Copy các file `.csproj` vào, ta vào từng folder đó copy những file đó vào:

```dockerfile
COPY ["MiniERP/MiniERP.csproj", "MiniERP/"]
COPY ["MiniERP.Application/MiniERP.Application.csproj", "MiniERP.Application/"]
COPY ["MiniERP.Domain/MiniERP.Domain.csproj", "MiniERP.Domain/"]
COPY ["MiniERP.Infrastructure/MiniERP.Infrastructure.csproj", "MiniERP.Infrastructure/"]
```

**2.2.** Sau đó chạy restore để down các package về:

```dockerfile
RUN dotnet restore "MiniERP/MiniERP.csproj"
RUN dotnet restore "MiniERP.Application/MiniERP.Application.csproj"
RUN dotnet restore "MiniERP.Domain/MiniERP.Domain.csproj"
RUN dotnet restore "MiniERP.Infrastructure/MiniERP.Infrastructure.csproj"
```

#### Bước 3: Publish

Ta sẽ copy phần còn lại của dự án vào bên trong, vì source code thay đổi rất thường xuyên nhưng các file requirement định nghĩa các dependency ít khi thay đổi nên ta mới copy vào trước và tiến hành restore.

```dockerfile
COPY . .
```

Ta lại tra trên link hồi nãy và thấy câu publish mẫu như sau:

```dockerfile
RUN dotnet publish -a $TARGETARCH --no-restore -c Release -o /app
```

Ta tiến hành adjust cho phù hợp:

```dockerfile
RUN dotnent publish "MiniERP/MiniERP.csproj" -c Release -o /app/publish
```

Ok đến đây là ta đã có runtime để chạy rồi, copy sang stage khác và chạy thôi.

#### Bước 4: Bước cuối cùng

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish . 
USER app
EXPOSE 8080
ENTRYPOINT ["dotnent", "MiniERP.dll"]
```

### 2.4. Dockerfile hoàn chỉnh

Ta có Dockerfile cuối cùng như sau:

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /source

COPY ["MiniERP/MiniERP.csproj", "MiniERP/"]
COPY ["MiniERP.Application/MiniERP.Application.csproj", "MiniERP.Application/"]
COPY ["MiniERP.Domain/MiniERP.Domain.csproj", "MiniERP.Domain/"]
COPY ["MiniERP.Infrastructure/MiniERP.Infrastructure.csproj", "MiniERP.Infrastructure/"]


RUN dotnet restore "MiniERP/MiniERP.csproj"
RUN dotnet restore "MiniERP.Application/MiniERP.Application.csproj"
RUN dotnet restore "MiniERP.Domain/MiniERP.Domain.csproj"
RUN dotnet restore "MiniERP.Infrastructure/MiniERP.Infrastructure.csproj"

COPY . .

RUN dotnet publish "MiniERP/MiniERP.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish . 
USER app
EXPOSE 8080
ENTRYPOINT ["dotnet", "MiniERP.dll"]
```

---

## 3. docker-compose.yml

Tiếp theo ta viết đến `docker-compose.yml` cho dự án, ta nhận thấy project có database là PostgreSQL 15, ta search đơn giản về 1 pattern cho database.

### 3.1. Service `db`

Ta research được trang chính của Docker Hub
🔗 https://hub.docker.com/_/postgres

Có 1 example như sau:

```yaml
services:

  db:
    image: postgres
    restart: always
    # set shared memory limit when using docker compose
    shm_size: 128mb
    # or set shared memory limit when deploy via swarm stack
    #volumes:
    #  - type: tmpfs
    #    target: /dev/shm
    #    tmpfs:
    #      size: 134217728 # 128*2^20 bytes = 128Mb
    environment:
      POSTGRES_PASSWORD: example
```

Và database cần volume để persistent nên cần tạo thêm 1 volume cho database.

Ta sẽ có quy trình viết cho service `db` như sau:

```
Chọn image → đặt tên container → đọc các biến môi trường → volume → healthcheck
```

#### 1. Chọn image

```yaml
image: postgres:15-alpine
```

#### 2. Tên

```yaml
container_name: minierp_db
```

#### 3. Biến môi trường

```yaml
POSTGRES_USER: ${POSTGRES_USER}
POSTGRES_PASSWORD: ${POSTGRES_PASSWORD}
POSTGRES_DB: ${POSTGRES_DB}
```

Ta cần xem file env để biết tên biến môi trường:

```env
DB_USER=postgres
DB_PASSWORD=your_secure_password_here
DB_NAME=minierp_docker
```

⇒

```yaml
POSTGRES_USER: ${DB_USER}
POSTGRES_PASSWORD: ${DB_PASSWORD}
POSTGRES_DB: ${DB_NAME}
```

#### 4. Volume

```yaml
volumes:
  - pgdata:/var/lib/postgresql/data
```

#### 5. Healthcheck

Ta thử research và tìm được trang web sau:
🔗 https://www.postgresql.org/message-id/CAFOeHx1wpqRLcs8jSDar-Em3F3ogSetV8sJPZnDMEehc_3XWuQ%40mail.gmail.com

Ta thấy đoạn ví dụ healthcheck:

```yaml
healthcheck:
  test: ["CMD-SHELL", "sh -c 'pg_isready -U ${POSTGRES_USER} -d
  ${POSTGRES_DB}'"]
  interval: 30s
  timeout: 10s
  retries: 5
```

Ta sửa lại 1 tí rồi sài:

```yaml
healthcheck:
  test: ["CMD-SHELL", "sh -c 'pg_isready -U ${DB_USER} -d
  ${DB_NAME}'"]
  interval: 30s
  timeout: 10s
  retries: 5
```

#### Final service `db`

Ta có final như sau:

```yaml
services:
  db:
    image: postgres:15-alpine
    container_name: minierp_db
    restart: always
    environemnt: 
      POSTGRES_USER: ${DB_USER}
      POSTGRES_PASSWORD: ${DB_PASSWORD}
      POSTGRES_DB: ${DB_NAME}
    ports: 
      - "5432:5432"
    volumes:
      - pgdata:/var/lib/postgresql/data
    healthcheck:
      test: ["CMD-SHELL", "sh -c 'pg_isready -U ${DB_USER} -d
      ${DB_NAME}'"]
      interval: 30s
      timeout: 10s
      retries: 5
```

### 3.2. Service `app`

Rồi ta chuyển qua viết cho cái application của chúng ta, tương tự `db` cũng có quy trình như trên.

#### 1. Base image

Chúng ta đã có Dockerfile cho app rồi nên:

```yaml
app:
  build:
    context: .
    dockerfile: Dockerfile
  container_name: minierp_app
```

#### 2. Biến môi trường

Ta sẽ check tại file `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=mini_erp_db;Username=postgres;Password=YOUR_SECRET_PASSWORD"
  },
  "Jwt": {
    "Key": "SuperSecretKeyForMiniErpChallenge5AuthAndJwtTokenVerificationMustBeAtLeast32BytesLong!iguess",
    "Issuer": "MiniERP",
    "Audience": "MiniERPUsers",
    "ExpireMinutes": 10080
  }
}
```

Ta thấy có 4 key của jwt như sau: `["Key","Issuer","Audience","ExporeMinutes"]` và ConnectionStrings: `["DefaultConnection"]`, ta có:

```yaml
environment: 
  - ASPNETCORE_ENVIRONMENT=Production
  - ConnectionStrings__DefaultConnection: "Host=db;Port=5432;Database=${POSTGRES_DB};Username=${POSTGRES_USER};nPassword=${POSTGRES_PASSWORD}"
  - Jwt__Key: ${JWT_KEY}
  - Jwt__Issuer: ${JWT_ISSUER}
  - Jwt__Audience: ${JWT_AUDIENCE}
  - Jwt__ExporeMinutes : ${JWT_EXPIRATION}
```

#### 3. Healthcheck

Ta đọc README của project có 1 API healthcheck (API Healthcheck: `http://localhost:8080/api/health`) nên ta triển khai luôn:

```yaml
healthcheck:
  test: ["CMD","curl","-f", "http://localhost:8080/api/health"]
  interval: 30s
  timeout: 10s
  retries: 5
  start_period: 30s
```

#### 4. Phụ thuộc vào `db`

App phụ thuộc vào `db`, nên `db` phải running trước và `db` phải healthy luôn:

```yaml
depends_on: 
  db:
    condition: service_healthy
```

#### Final service `app`

Ta có bản final cho app:

```yaml
  app:
    build:
      context: .
      dockerfile: Dockerfile
    container_name: minierp_app
    restart: always
    environment: 
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection: "Host=db;Port=5432;Database=${POSTGRES_DB};Username=${POSTGRES_USER};nPassword=${POSTGRES_PASSWORD}"
      - Jwt__Key: ${JWT_KEY}
      - Jwt__Issuer: ${JWT_ISSUER}
      - Jwt__Audience: ${JWT_AUDIENCE}
      - Jwt__ExpireMinutes : ${JWT_EXPIRATION}
    ports:
      - "8080:8080"
    depends_on: 
    db:
      condition: service_healthy
    healthcheck:
      test: ["CMD","curl","-f", "http://localhost:8080/api/health"]
      interval: 30s
      timeout: 10s
      retries: 5
      start_period: 30s
```

✅ Ta đã xong docker-compose.

---

## 4. Bổ sung

### 4.1. Cài `curl` cho image app

Có mỗi lỗi mới phát hiện là base image của app không có `curl` sẵn nên phải cài `curl`:

```dockerfile
RUN apt-get update \
  && apt-get install -y --no-install-recommends curl \
  && rm -rf /var/lib/apt/lists/*
```

### 4.2. `.dockerignore`

Ta viết thêm `.dockerignore` để bỏ những thứ không cần thiết khi build, ta thử research ta thấy trang sau:
🔗 https://github.com/mjrousos/AspNetCore-Sample/blob/master/.dockerignore

```
.dockerignore
.env
.git
.gitignore
.vs
.vscode
docker-compose.yml
docker-compose.*.yml
*/bin
*/obj
```

---

## 5. Chạy test & debug

Ta tiến hành chạy test.

### 5.1. Lỗi gặp phải

Lúc chạy ta thấy có lỗi sau:

```log
dleware.cs:line 30
minierp_app  | fail: MiniERP.Middleware.GlobalExceptionMiddleware[0]
minierp_app  |       An unhandled exception occurred during the request.
minierp_app  |       System.ArgumentException: IDX10703: Cannot create a 'Microsoft.IdentityModel.Tokens.SymmetricSecurityKey', key length is zero.
minierp_app  |          at Microsoft.IdentityModel.Tokens.SymmetricSecurityKey..ctor(Byte[] key)
minierp_app  |          at Program.<>c__DisplayClass0_0.<<Main>$>b__3(JwtBearerOptions options) in /source/MiniERP/Program.cs:line 71
minierp_app  |          at Microsoft.Extensions.Options.OptionsFactory`1.Create(String name)
minierp_app  |          at System.Lazy`1.ViaFactory(LazyThreadSafetyMode mode)
minierp_app  |       --- End of stack trace from previous location ---
minierp_app  |          at System.Lazy`1.CreateValue()
minierp_app  |          at Microsoft.Extensions.Options.OptionsCache`1.GetOrAdd[TArg](String name, Func`3 createOptions, TArg factoryArgument)
minierp_app  |          at Microsoft.AspNetCore.Authentication.AuthenticationHandler`1.InitializeAsync(AuthenticationScheme scheme, HttpContext context)
```

### 5.2. Phân tích

Nó bảo là cái `SymmetricSecurityKey` nó bằng 0, nó bảo ở dòng `Program.cs:line 71`. Ta check thử ta thấy nội dung tại đó như sau:

```csharp
// Cấu hình JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["Key"] ?? throw new InvalidOperationException("JWT Secret Key is not configured.");
var key = Encoding.UTF8.GetBytes(secretKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});
```

### 5.3. Cách sửa

Có nghĩa là nó phải yêu cầu env có biến `JWT_KEY`, ta thêm biến đó vào trong `.env` và chạy lại.

---

## 6. Kết quả

### 6.1. Các container đều healthy

✅ Oke done

```powershell
PS D:\Intership\MEU\FIRST_CHALLENAGE\VietProDev-Backend-Challenge-Dotnet> docker compose ps
NAME          IMAGE                                     COMMAND                  SERVICE   CREATED          STATUS                    PORTS
minierp_app   vietprodev-backend-challenge-dotnet-app   "dotnet MiniERP.dll"     app       9 seconds ago    Up 7 seconds (healthy)    0.0.0.0:8080->8080/tcp
minierp_db    postgres:15-alpine                        "docker-entrypoint.s…"   db        10 minutes ago   Up 10 minutes (healthy)   0.0.0.0:5432->5432/tcp
```

### 6.2. Login thành công

```powershell
PS D:\Intership\MEU\FIRST_CHALLENAGE\VietProDev-Backend-Challenge-Dotnet> curl.exe -i -X POST http://localhost:8080/api/auth/login `
>>   -H "Content-Type: application/json" `
>>   -d '{\"email\":\"admin@example.com\",\"password\":\"admin123\"}'
HTTP/1.1 200 OK
Content-Type: application/json; charset=utf-8
Date: Tue, 06 Oct 2026 05:42:07 GMT
Server: Kestrel
Transfer-Encoding: chunked

{"success":true,"message":"Login successful.","data":{"token":"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI5ZThmNTk5My01N2ExLTQyYTctOGIzOC0wMzhhZjY1ZjE2M2QiLCJlbWFpbCI6ImFkbWluQGV4YW1wbGUuY29tIiwidW5pcXVlX25hbWUiOiJhZG1pbiIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IkFETUlOIiwiZXhwIjoxNzkxMzUxNzI4LCJpc3MiOiJNaW5pRVJQX0RvY2tlcl9Jc3N1ZXIiLCJhdWQiOiJNaW5pRVJQX0RvY2tlcl9BdWRpZW5jZSJ9.hrsU27XLUTKqHIOFIECXrrI_6CtFx0DEDs30oKrqLuc","expiresAt":"2026-10-07T05:42:08.3088303Z"},"errors":null}
```