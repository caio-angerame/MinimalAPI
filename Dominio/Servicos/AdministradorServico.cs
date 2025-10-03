using MinimalAPI.Dominio.Entidades;
using MinimalAPI.Dominio.Interfaces;
using MinimalAPI.DTOs;
using MinimalAPI.Infraestrutura.DB;

namespace minimalAPI.Dominio.Servicos;

public class AdministradorServico : IAdministradorServico
{
  private readonly DBContexto _contexto;
  public AdministradorServico(DBContexto contexto)
  {
    _contexto = contexto;
  }
  public Administrador? Login(LoginDTO loginDTO)
  {
    var adm = _contexto.Administradores.Where(a => a.Email == loginDTO.Email && a.Senha == loginDTO.Senha).FirstOrDefault();
    return adm;
  }
}