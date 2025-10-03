using Microsoft.EntityFrameworkCore;
using MinimalAPI.Dominio.Entidades;

namespace MinimalAPI.Infraestrutura.DB;

public class DBContexto : DbContext
{
  private readonly IConfiguration _configuracaoAppSettings;
  public DBContexto(IConfiguration configuracaoAppSettings)
  {
    _configuracaoAppSettings = configuracaoAppSettings;
  }

  public DbSet<Administrador> Administradores { get; set; } = default!;  
  protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
  {
    if (!optionsBuilder.IsConfigured)
    {
      var stringConexao = _configuracaoAppSettings.GetConnectionString("mysql")?.ToString();
      if (!string.IsNullOrEmpty(stringConexao))
      {
          optionsBuilder.UseMySql(
          stringConexao,
          ServerVersion.AutoDetect(stringConexao));
      }
    }
  }
}