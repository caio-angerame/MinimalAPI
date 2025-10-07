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

  public Administrador? BuscarPorId(int id)
  {
    return _contexto.Administradores.Where(v => v.Id == id).FirstOrDefault();
  }

  public Administrador Incluir(Administrador administrador)
  {

    _contexto.Administradores.Add(administrador);
    _contexto.SaveChanges();

    return administrador;
  }

  public Administrador? Login(LoginDTO loginDTO)
  {
    var adm = _contexto.Administradores.Where(a => a.Email == loginDTO.Email && a.Senha == loginDTO.Senha).FirstOrDefault();
    return adm;
  }
  
  public List<Administrador> Todos(int? pagina)
  {
    var query = _contexto.Administradores.AsQueryable();

    int itensPorPagina = 10;

    if (pagina != null)
    {
      query = query.Skip(((int)pagina - 1) * itensPorPagina).Take(itensPorPagina);
    }
    return query.ToList();
  }
}