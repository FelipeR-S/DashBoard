using DashBoard.Data;
using DashBoard.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace DashBoard.Repositories
{
    public abstract class BaseRepository<T> where T : BaseModel
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _dbSet;

        protected BaseRepository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        protected List<T> CarregaObjetoFromJson(string path, string fileName)
        {
            List<T> listaObjetos = new List<T>();
            try
            {
                string json = LoadJson(path, fileName);
                var jsonObject = JsonConvert.DeserializeObject<List<T>>(json);
                if (jsonObject != null && jsonObject.Any())
                    listaObjetos = jsonObject;
            }
            catch
            {
            }
            return listaObjetos;
        }

        protected List<string> GetListaStringFromJson(string path, string fileName)
        {
            List<string> listaObjetos = new List<string>();
            try
            {
                string json = LoadJson(path, fileName);
                var jsonObject = JsonConvert.DeserializeObject<List<string>>(json);
                if (jsonObject != null && jsonObject.Any())
                    listaObjetos = jsonObject;
            }
            catch { }
            return listaObjetos;
        }

        protected string LoadJson(string path, string fileName)
        {
            string retorno = string.Empty;
            if (File.Exists(Path.Combine(path, fileName)))
            {
                using (StreamReader r = new StreamReader(Path.Combine(path, fileName)))
                {
                    retorno = r.ReadToEnd();
                }
            }
            return retorno;
        }
    }
}
