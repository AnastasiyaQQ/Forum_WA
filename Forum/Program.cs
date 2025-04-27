using Forum.Data;
using Forum.Services.Interfaces;
using Forum.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer; 
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens; 
using Microsoft.OpenApi.Models; 
using System.Text;

var builder = WebApplication.CreateBuilder(args);


// Контекст БД
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ForumDbContext>(options =>
// Исп Pomelo MySQL
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))); 

// Password Hasher
builder.Services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>(); 



builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<IAdminService, AdminService>();

// Контроллеры
builder.Services.AddControllers();


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
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

// Доб авторизацию
builder.Services.AddAuthorization(); 


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", 
        policy =>
        {
            // Разрешить источник клиента из appsettings
            policy.WithOrigins(builder.Configuration["Jwt:Audience"]) 
                  .AllowAnyHeader()
                  .AllowAnyMethod();

        });
});



builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // Конфигурация для JWT в Swagger UI
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Forum API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        // Изменено с ApiKey на Http
        Type = SecuritySchemeType.Http,
        // Указываем схему bearer
        Scheme = "bearer",
        // Формат токена
        BearerFormat = "JWT" 
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});




var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Forum API v1"));
    app.UseDeveloperExceptionPage(); 
    // Показывает детальные ошибки
}
else
{
    app.UseExceptionHandler("/Error"); 
    app.UseHsts(); 
}

// Перенаправлять HTTP на HTTPS
app.UseHttpsRedirection(); 

app.UseRouting(); 

app.UseCors("AllowSpecificOrigin");

// Включить аутентификацию
app.UseAuthentication();
// Включить авторизацию
app.UseAuthorization();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();

app.Run();
