using MinimalAPI.Infraestrutura.DB;
using MinimalAPI.DTOs;
using Microsoft.EntityFrameworkCore;
using MinimalAPI.Dominio.Interfaces;
using minimalAPI.Dominio.Servicos;
using Microsoft.AspNetCore.Mvc;
using MinimalAPI.Dominio.ModelViews;
using MinimalAPI.Dominio.Entidades;
using MinimalAPI.Dominio.Enuns;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;

#region Builder
var builder = WebApplication.CreateBuilder(args);

var key = builder.Configuration.GetSection("Jwt").ToString();
if (string.IsNullOrEmpty(key)) key = "123456";

builder.Services.AddAuthentication(option =>
{
  option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
  option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(option =>
{
  option.TokenValidationParameters = new TokenValidationParameters
  {
    ValidateLifetime = true,
    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
  };
});

builder.Services.AddAuthorization();

builder.Services.AddScoped<IAdministradorServico, AdministradorServico>();
builder.Services.AddScoped<IVeiculoServico, VeiculoServico>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<DBContexto>(options =>
{
  options.UseMySql(
    builder.Configuration.GetConnectionString("MySql"),
    ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("mysql")));

});

var app = builder.Build();
#endregion

#region Home
app.MapGet("/", () => Results.Json(new Home())).WithTags("Home");
#endregion

#region Administradores
string GerarTokenJwt(Administrador administrador)

{
  if (string.IsNullOrEmpty(key)) return string.Empty;

  var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
  var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

  var claims = new List<Claim>()
  {
    new Claim("Email", administrador.Email),
    new Claim("Perfil", administrador.Perfil.ToString())
  };

  var token = new JwtSecurityToken(
    claims: claims,
    expires: DateTime.Now.AddDays(1),
    signingCredentials: credentials
  );

  return new JwtSecurityTokenHandler().WriteToken(token);
}

app.MapPost("/administradores/login", ([FromBody] LoginDTO loginDTO, IAdministradorServico administradorServico) =>
{
  var adm = administradorServico.Login(loginDTO);
  if (adm != null)
  {
    string token = GerarTokenJwt(adm);
    return Results.Ok(new AdministradorLogado
    {
      Email = adm.Email,
      Perfil = adm.Perfil,
      Token = token
    });
  }
  else
    return Results.Unauthorized();
}).WithTags("Administradores");

app.MapGet("/administradores", ([FromQuery] int? pagina, IAdministradorServico administradorServico) =>
{
  var adms = new List<AdministradorModelView>();
  var administradores = administradorServico.Todos(pagina);
  foreach (var adm in administradores)
  {
    adms.Add(new AdministradorModelView
    {
      Id = adm.Id,
      Email = adm.Email,
      Perfil = adm.Perfil
    });
  }
  return Results.Ok(adms);
}).RequireAuthorization().WithTags("Administradores");

app.MapPost("/administradores", ([FromBody] AdministradorDTO administradorDTO, IAdministradorServico administradorServico) =>
{
  var validacao = new ErrosDeValidacao
  {
    Mensagens = new List<string>()
  };

  if (string.IsNullOrEmpty(administradorDTO.Email))
    validacao.Mensagens.Add("O email não pode ficar em branco");
  if (string.IsNullOrEmpty(administradorDTO.Senha))
    validacao.Mensagens.Add("A senha não pode ficar em branco");
  if (administradorDTO.Perfil == null)
    validacao.Mensagens.Add("O perfil não pode ficar em branco");

  if (validacao.Mensagens.Count > 0)
    return Results.BadRequest(validacao);

  var administrador = new Administrador
  {
    Email = administradorDTO.Email,
    Senha = administradorDTO.Senha,
    Perfil = administradorDTO.Perfil.ToString() ?? Perfil.Editor.ToString()
  };

  administradorServico.Incluir(administrador);

  return Results.Created($"/administrador/{administrador.Id}",new AdministradorModelView
  {
    Id = administrador.Id,
    Email = administrador.Email,
    Perfil = administrador.Perfil
  });

}).RequireAuthorization().WithTags("Administradores");

app.MapGet("/administradores/{id}", ([FromRoute] int id, IAdministradorServico administradorServico) =>
{
  var administrador = administradorServico.BuscarPorId(id);

  if (administrador == null) return Results.NotFound();

  return Results.Ok(administrador);
}).RequireAuthorization().WithTags("Administradores");
#endregion

#region Veiculos
ErrosDeValidacao validaDTO (VeiculoDTO veiculoDTO)
{
  var validacao = new ErrosDeValidacao
  {
    Mensagens = new List<string>()
  };

  if (string.IsNullOrEmpty(veiculoDTO.Nome))
    validacao.Mensagens.Add("O nome não pode ficar em branco");

  if (string.IsNullOrEmpty(veiculoDTO.Marca))
    validacao.Mensagens.Add("A marca não pode ficar em branco");

  if (veiculoDTO.Ano < 1950)
    validacao.Mensagens.Add("Veículo muito antigo, aceito somente anos superiores a 1950");

  return validacao;
}

app.MapPost("/veiculos", ([FromBody] VeiculoDTO veiculoDTO, IVeiculoServico veiculoServico) =>
{
  var validacao = validaDTO(veiculoDTO);
  if (validacao.Mensagens.Count > 0)
  {
    return Results.BadRequest(validacao);
  }


  var veiculo = new Veiculo
  {
    Nome = veiculoDTO.Nome,
    Marca = veiculoDTO.Marca,
    Ano = veiculoDTO.Ano
  };
  veiculoServico.Incluir(veiculo);

  return Results.Created($"/veiculo/{veiculo.Id}", veiculo);
}).RequireAuthorization().WithTags("Veículos");

app.MapGet("/veiculos", ([FromQuery] int? pagina, IVeiculoServico veiculoServico) =>
{
  var veiculos = veiculoServico.Todos(pagina);

  return Results.Ok(veiculos);
}).RequireAuthorization().WithTags("Veículos");

app.MapGet("/veiculos/{id}", ([FromRoute] int id, IVeiculoServico veiculoServico) =>
{
  var veiculo = veiculoServico.BuscarPorId(id);

  if (veiculo == null) return Results.NotFound();

  return Results.Ok(veiculo);
}).RequireAuthorization().WithTags("Veículos");

app.MapPut("/veiculos/{id}", ([FromRoute] int id, VeiculoDTO veiculoDTO, IVeiculoServico veiculoServico) =>
{
  var veiculo = veiculoServico.BuscarPorId(id);
  if (veiculo == null) return Results.NotFound();

  var validacao = validaDTO(veiculoDTO);
  if (validacao.Mensagens.Count > 0)
  {
    return Results.BadRequest(validacao);
  }

  veiculo.Nome = veiculoDTO.Nome;
  veiculo.Marca = veiculoDTO.Marca;
  veiculo.Ano = veiculoDTO.Ano;

  veiculoServico.Atualizar(veiculo);

  return Results.Ok(veiculo);
}).RequireAuthorization().WithTags("Veículos");

app.MapDelete("/veiculos/{id}", ([FromRoute] int id, IVeiculoServico veiculoServico) =>
{
  var veiculo = veiculoServico.BuscarPorId(id);
  if (veiculo == null) return Results.NotFound();

  veiculoServico.Apagar(veiculo);

  return Results.NoContent();
}).RequireAuthorization().WithTags("Veículos");
#endregion

#region App
app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.Run();
#endregion



