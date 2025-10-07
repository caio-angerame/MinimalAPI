using MinimalAPI.Dominio.Entidades;
using MinimalAPI.DTOs;

namespace MinimalAPI.Dominio.Interfaces;

public interface IAdministradorServico
{
  Administrador? Login(LoginDTO loginDTO);
  Administrador Incluir(Administrador administrador);
  Administrador? BuscarPorId(int id);
  List<Administrador> Todos(int? pagina);

}