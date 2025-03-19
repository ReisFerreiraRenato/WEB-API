using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using WEB_API.Models;
using WEB_API.Services;
using WEB_API.Utils;

namespace WEB_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProdutosController(ProdutosUsuariosContext context, ILogger<ProdutosController> logger, ILogErroService logErroService) : ControllerBase
    {
        private readonly ProdutosUsuariosContext _context = context;
        private readonly ILogger<ProdutosController> _logger = logger;
        private readonly ILogErroService _logErroService = logErroService;

        // GET: api/Produtos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Produto>>> GetProdutos()
        {
            try
            {
                return await _context.Produtos.ToListAsync();
            }
            catch (Exception ex)
            {
                return await HandleGenericError(ex, Constantes.ErroListaProdutos);
            }
        }

        // GET: api/Produtos/5
        [HttpGet("{id}", Name = Constantes.GetProduto)]
        public async Task<ActionResult<Produto>> GetProduto(int id)
        {
            try
            {
                if (id == 0)
                {
                    return await HandleProdutoNaoEncontrado(id);
                }

                var produto = await _context.Produtos.FindAsync(id);

                if (produto == null)
                {
                    return await HandleProdutoNaoEncontrado(id);
                }

                return produto;
            }
            catch (Exception ex)
            {
                return await HandleGenericError(ex, Funcoes.ObterMensagemErroObterProduto(id), id);
            }
        }

        // POST: api/Produtos
        [HttpPost]
        public async Task<ActionResult<Produto>> PostProduto(ProdutoRequest request)
        {
            if (!ModelState.IsValid)
            {
                return new ObjectResult(ModelState) { StatusCode = 406 };
            }

            try
            {
                var produto = Produto.CriarNovoProduto(request.Nome, request.Preco, Convert.FromBase64String(request.Imagem));
                _context.Produtos.Add(produto);
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetProduto", new { id = produto.Id }, produto);
            }
            catch (DbUpdateException ex) when (ex.InnerException is Microsoft.Data.Sqlite.SqliteException sqliteException && sqliteException.SqliteErrorCode == 19)
            {
                _logger.LogError(sqliteException, "SqliteException com SqliteErrorCode 19 capturada"); // Log de depuração
                return Conflict("Produto com o mesmo ID já existe.");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "DbUpdateException capturada");
                if (ex.InnerException != null)
                {
                    _logger.LogError(ex.InnerException, "InnerException não é nulo");
                    _logger.LogError(ex.InnerException, "Tipo da InnerException: {InnerExceptionType}", ex.InnerException.GetType().FullName);
                    if (ex.InnerException is Microsoft.Data.Sqlite.SqliteException sqliteException)
                    {
                        _logger.LogError(sqliteException, "SqliteException capturada");
                        _logger.LogError(sqliteException, "SqliteErrorCode: {SqliteErrorCode}", sqliteException.SqliteErrorCode);
                    }
                }

                _logger.LogError(ex, "Erro ao criar produto");
                await _logErroService.LogErroAsync(ex, Request.Path, Request.Method, Request.Body.ToString(), nameof(ProdutosController));
                return new ObjectResult("Erro interno do servidor") { StatusCode = 500 };
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar produto");
                await _logErroService.LogErroAsync(ex, Request.Path, Request.Method, Request.Body.ToString(), nameof(ProdutosController));
                return new ObjectResult("Erro interno do servidor") { StatusCode = 500 };
            }
        }

        // PUT: api/Produtos/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduto(int id, ProdutoRequest request)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(406, ModelState);
            }

            if (id != request.Id)
            {
                return BadRequest(Constantes.ErroIdRotaDiferenteCorpo);
            }

            try
            {
                var produtoExistente = await _context.Produtos.FindAsync(id);
                if (produtoExistente == null)
                {
                    return NotFound();
                }

                var imagemBytes = string.IsNullOrEmpty(request.Imagem) ? Array.Empty<byte>() : Convert.FromBase64String(request.Imagem);
                var produtoAtualizado = Produto.CriarProdutoExistente(id, request.Nome ?? Constantes.ErroProdutoSemNome, request.Preco, imagemBytes);

                _context.Entry(produtoExistente).CurrentValues.SetValues(produtoAtualizado);
                _context.Entry(produtoExistente).State = EntityState.Modified;

                await _context.SaveChangesAsync();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }
            catch (Exception ex)
            {
                return await HandleGenericError(ex, Funcoes.ObterMensagemErroAtualizarProduto(id), id);
            }

            return NoContent();
        }

        // DELETE: api/Produtos/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduto(int id)
        {
            try
            {
                var produto = await _context.Produtos.FindAsync(id);
                if (produto == null)
                {
                    return NotFound();
                }

                _context.Produtos.Remove(produto);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return await HandleGenericError(ex, Funcoes.ObterMensagemErroDeletarProduto(id), id);
            }
        }

        private async Task<ActionResult> HandleProdutoNaoEncontrado(int id)
        {
            var erro = new Exception(Constantes.ProdutoNaoEncontrado);
            _logger.LogError(erro, Funcoes.ObterMensagemErroObterProduto(id), id);
            await _logErroService.LogErroAsync(erro, Request.Path, Request.Method, await ObterCorpoRequisicao(), nameof(ProdutosController));
            return NotFound(new { id, Mensagem = Constantes.ProdutoNaoEncontrado });
        }

        private async Task<ActionResult> HandleGenericError(Exception ex, string mensagemErro, int? id = null)
        {
            _logger.LogError(ex, mensagemErro, id?.ToString() ?? "null");
            await _logErroService.LogErroAsync(ex, Request.Path, Request.Method, await ObterCorpoRequisicao(), nameof(ProdutosController));
            return StatusCode(500, Constantes.ErroInternoRequisicao);
        }

        private bool ProdutoExists(int id)
        {
            return _context.Produtos.Any(e => e.Id == id);
        }

        private async Task<string> ObterCorpoRequisicao()
        {
            if (Request?.ContentLength > 0)
            {
                Request.EnableBuffering();
                Request.Body.Position = 0;
                using var reader = new System.IO.StreamReader(Request.Body);
                var corpoRequisicao = await reader.ReadToEndAsync();
                Request.Body.Position = 0;
                return corpoRequisicao;
            }
            return string.Empty;
        }

        public class ProdutoRequest
        {
            public int Id { get; set; }

            [Required(ErrorMessage = Constantes.ErroNomeProdutoObrigatorio)]
            [StringLength(255, MinimumLength = 3, ErrorMessage = Constantes.ErroNomeProduto3Caracteres)]
            public string? Nome { get; set; }

            [Range(0.50, double.MaxValue, ErrorMessage = Constantes.ErroPrecoMinimoProduto)]
            public double Preco { get; set; }

            public string? Imagem { get; set; }
        }
    }
}