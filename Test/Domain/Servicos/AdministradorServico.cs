using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using minimalAPI.Dominio.Servicos;
using MinimalAPI.Dominio.Entidades;
using MinimalAPI.Infraestrutura.DB;

namespace Test.Domain.Entidades;

[TestClass]
public class AdministradorServicoTest
{

  private DBContexto CriarContextoDeTeste()
  {
    var builder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

    var configuration = builder.Build();

    return new DBContexto(configuration);
  }
  
  [TestMethod]
  public void TestandoSalvarAdministrador()
  {
    // Arrange
    var context = CriarContextoDeTeste();
    context.Database.ExecuteSqlRaw("TRUNCATE TABLE Administradores");

    var adm = new Administrador();
    adm.Id = 1;
    adm.Email = "teste@teste.com";
    adm.Senha = "teste";
    adm.Perfil = "Adm";
    context = CriarContextoDeTeste();
    var administradorServico = new AdministradorServico(context);

    // Act
    administradorServico.Incluir(adm);

    // Assert
    Assert.AreEqual(1, administradorServico.Todos(1).Count());
    Assert.AreEqual("teste@teste.com", adm.Email);
    Assert.AreEqual("teste", adm.Senha);
    Assert.AreEqual("Adm", adm.Perfil);
  }
}
