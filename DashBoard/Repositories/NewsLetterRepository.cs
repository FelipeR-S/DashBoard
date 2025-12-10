using DashBoard.Data;
using DashBoard.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DashBoard.Repositories
{
    public interface INewsLetterRepository
    {

        /// <summary>
        /// Cadastra email no banco de dados
        /// </summary>
        /// <param name="email"></param>
        /// <returns>Retorna resposta <see cref="string"/> se foi cadastrado ou não.</returns>
        Task<string> CadastraEmail(NewsLetter email);

        /// <summary>
        /// Retorna todos os cadastros
        /// </summary>
        /// <returns><see cref="List{T}"/> de <see cref="NewsLetter"/></returns>
        Task<List<NewsLetter>> GetAllData();

        /// <summary>
        /// Gera loop para envio de emails para lista de newsletters
        /// </summary>
        /// <param name="listaEmails"><see cref="List{T}"/> de <see cref="NewsLetter"/> e-mails</param>
        /// <param name="html"><see cref="string"/> Html da mensagem</param>
        /// <param name="assunto"><see cref="string"/> assunto</param>
        /// <param name="ct"><see cref="CancellationToken"/> token de cancelamento</param>
        /// <returns><see cref="Tuple{T1, T2}"/><see cref="int"/> quantidade de envios, <see cref="string"/> mensagem de confirmação</returns>
        Task<Tuple<int, string>> EnviaEmailMarketing(List<NewsLetter> listaNewsLetter, RequestEmail request, CancellationToken ct);
    }
    public class NewsLetterRepository : BaseRepository<NewsLetter>, INewsLetterRepository
    {
        private readonly ISendEmail _sendEmail;
        public NewsLetterRepository(ApplicationDbContext context, ISendEmail sendEmail) : base(context)
        {
            _sendEmail = sendEmail;
        }

        public async Task<string> CadastraEmail(NewsLetter email)
        {
            var emailDB = await _dbSet.FirstOrDefaultAsync(c => c.Email == email.Email);

            if (emailDB == null)
            {
                await _dbSet.AddAsync(email);
                await _context.SaveChangesAsync();
                return "Cadastro concluído";
            }
            else return "E-mail já consta nas bases de dados.";
        }

        public async Task<List<NewsLetter>> GetAllData()
            => await _dbSet.AsNoTracking().ToListAsync();

        public async Task<Tuple<int, string>> EnviaEmailMarketing(List<NewsLetter> listaNewsLetter, RequestEmail request, CancellationToken ct)
        {
            var enviados = 0;
            var naoEnviados = listaNewsLetter.Count();
            var mensagem = "";
            try
            {
                var options = new ParallelOptions
                {
                    CancellationToken = ct,
                    MaxDegreeOfParallelism = 10
                };
                await Parallel.ForEachAsync(listaNewsLetter, options, async (newsLetter, token) =>
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(newsLetter.Email))
                        {
                            var sendHtml = request.TemplateHtml.Replace("@email", newsLetter.Email);
                            var result = await _sendEmail.EnviarEmail(newsLetter.Email, sendHtml, request.Assunto);
                            if (result)
                            {
                                Interlocked.Decrement(ref naoEnviados);
                                Interlocked.Increment(ref enviados);
                            }
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Erro: " + ex.Message);
                    }
                });
            }
            catch (OperationCanceledException)
            {
                mensagem = "Operação cancelada\r\n";
            }
            if (naoEnviados > 0) return new Tuple<int, string>(enviados, mensagem += $"{enviados} e-mails foram enviados!\r\n{naoEnviados} não puderam ser enviados.");
            else return new Tuple<int, string>(enviados, mensagem += $"{enviados} e-mails foram enviados!");
        }
    }
}
