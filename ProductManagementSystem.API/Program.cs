using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ProductManagementSystem.Db.Data;
using ProductManagementSystem.API.Services;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// ตรวจสอบและสร้างโฟลเดอร์สำหรับเก็บไฟล์รูปภาพใน WebRoot ของโปรเจกต์อย่างถูกต้องและปลอดภัย
var webRoot = builder.Environment.WebRootPath;
if (string.IsNullOrEmpty(webRoot))
{
    webRoot = Path.Combine(builder.Environment.ContentRootPath, "wwwroot");
}
var uploadsPath = Path.Combine(webRoot, "uploads");
if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProductService, ProductService>();

// ตั้งค่า Swagger พร้อมรองรับ JWT
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Product Management System API", Version = "v1" });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "กรอก JWT Token ในช่องด้านล่างในรูปแบบ: Bearer {your_jwt_token}"
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});

// ตั้งค่า JWT Bearer Authentication
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrEmpty(jwtKey))
{
    // FALLBACK KEY FOR LOCAL DEVELOPMENT AND TESTING ONLY. DO NOT USE IN PRODUCTION.
    // คีย์จำลองใช้สำหรับรันทดสอบระบบในเครื่องโฮสต์ (Local Dev/Test) เพื่อความสะดวกในการรันตรวจงาน
    jwtKey = "local_development_fallback_secret_key_for_testing_only_32_bytes";
}
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// ตั้งค่า CORS เพื่ออนุญาตให้ Angular ยิงหา API ได้
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// ทำการรัน Migration สร้างฐานข้อมูลพร้อมตารางและ Seed ข้อมูลอัตโนมัติ (สำคัญมากสำหรับ Docker)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// เปิดใช้งาน Swagger เสมอสำหรับขั้นตอนการพัฒนาและเทสระบบ
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseStaticFiles(); // เปิดสิทธิ์ให้บริการโฮสต์รูปภาพ

app.UseCors("AllowAngular"); // ต้องเปิด CORS ก่อนทำการตรวจสอบสิทธิ์ (Authentication)

app.UseAuthentication(); // ตรวจว่าใครเข้ามา (ต้องอยู่ก่อน UseAuthorization เสมอ)
app.UseAuthorization();  // ตรวจว่ามีสิทธิ์ทำอะไรได้บ้าง

app.MapControllers();

app.Run();
