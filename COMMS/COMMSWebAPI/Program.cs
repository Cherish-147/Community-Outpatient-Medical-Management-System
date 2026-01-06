
using COMMSWebAPI.Models;
using COMMSWebAPI.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
namespace COMMSWebAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            //Add Dbcontext Start
            builder.Services.AddDbContext<MyDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
            //Add Dbcontext End
            //添加服务层
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IHomeService, HomeService>();
            builder.Services.AddScoped<IAppointmentService, AppointmentService>();//预约挂号服务
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            //builder.Services.AddSwaggerGen();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "COMMSWebAPI v1", Version = "v1" });
                //添加JWT 安全定义
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    //Type =SecuritySchemeType.Apikey,//Bearer {token}
                    Type = SecuritySchemeType.Http,//{token}
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    //Description ="输入 JWT Token,格式 ：Bearer{token}"////Bearer {token}
                    Description = "只输入Token,无需输入Bearer前缀"//{token}

                });
                //添加安全要求
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
                    new string[] { }
                    }
                    });
            });
            //添加认证
            builder.Services.AddAuthentication("Bearer").AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = "qianfaren",//与GenerateJwtToken 中issuer 一致
                    ValidAudience = "jieshouren",//与GenerateJwtToken Audience 一致
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes("ThisIsASuperLongSecretKeyForJWTAuthentication123456789"))
                };
            });

            builder.Services.AddControllersWithViews(options =>
            {
                options.Filters.Add(new Microsoft.AspNetCore.Mvc.Authorization.AuthorizeFilter());
            });//全局加上验证token 除了控制器加上[AllowAnonymous]

           
            
            //跨域
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            }); //解决跨域冲突
            var app = builder.Build();
            app.UseCors("AllowAll");
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthentication();//add Token
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
