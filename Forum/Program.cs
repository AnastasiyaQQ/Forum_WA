using Forum.Data;
using Forum.Services.Interfaces;
using Forum.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer; 
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens; 
using Microsoft.OpenApi.Models; 
using System.Text;

var builder = WebApplication.CreateBuilder(args);


// �������� ��
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ForumDbContext>(options =>
// ��� Pomelo MySQL
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))); 

// Password Hasher
builder.Services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>(); 



builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<IAdminService, AdminService>();

// �����������
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

// ��� �����������
builder.Services.AddAuthorization(); 


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", 
        policy =>
        {
            // ��������� �������� ������� �� appsettings
            policy.WithOrigins(builder.Configuration["Jwt:Audience"]) 
                  .AllowAnyHeader()
                  .AllowAnyMethod();

        });
});



builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // ������������ ��� JWT � Swagger UI
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Forum API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        // �������� � ApiKey �� Http
        Type = SecuritySchemeType.Http,
        // ��������� ����� bearer
        Scheme = "bearer",
        // ������ ������
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
    // ���������� ��������� ������
}
else
{
    app.UseExceptionHandler("/Error"); 
    app.UseHsts(); 
}

// �������������� HTTP �� HTTPS
app.UseHttpsRedirection(); 

app.UseRouting(); 

app.UseCors("AllowSpecificOrigin");

// �������� ��������������
app.UseAuthentication();
// �������� �����������
app.UseAuthorization();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();

app.Run();
